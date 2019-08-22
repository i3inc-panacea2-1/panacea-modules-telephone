using Panacea.Controls;
using Panacea.Core;
using Panacea.Modularity.AppBar;
using Panacea.Modularity.Billing;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modularity.UserAccount;
using Panacea.Modules.Telephone.Models;
using Panacea.Modules.Telephone.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(TelephonePage))]
    public class TelephonePageViewModel : ViewModelBase
    {
        const string TELEPHONE = "Telephone";
        private readonly PanaceaServices _core;
        private readonly ObservableCollection<LiveTileFrame> _tiles;
        private TelephoneBase _terminalPhone, _userPhone;
        bool _autoRejected, _wasIncoming;
        private ObservableCollection<SpeedDial> _terminalSpeedDials;
        private ObservableCollection<UserSpeedDial> _userSpeedDials;
        DateTime _lastCallStartTime;
        Translator _translator = new Translator("Telephone");
        Service _currentService;

        IServiceMonitor _serviceMonitor;
        public TelephonePageViewModel(PanaceaServices core, ObservableCollection<LiveTileFrame> tiles)
        {
            _core = core;
            _tiles = tiles;
            BuyServiceCommand = new RelayCommand(args =>
            {
                if (_core.TryGetBilling(out IBillingManager bill))
                {
                    bill.NavigateToBuyServiceWizard();
                }
            });
            SignInCommand = new RelayCommand(args =>
            {
                if (_core.TryGetUserAccountManager(out IUserAccountManager user))
                {
                    user.LoginAsync();
                }
            });
            DialPadKeyPressCommand = new RelayCommand(args =>
            {
                var character = args.ToString();
                if (int.TryParse(character, out int number))
                {
                    Number += number.ToString();
                    if (Number.StartsWith("00"))
                    {
                        Number = "+" + Number.Substring(2, Number.Length - 2);
                    }
                }
                else if (character == "*" || character == "#")
                {
                    Number += character.ToString();
                }
            });
            DialPadBackspaceCommand = new RelayCommand(args =>
            {
                if (Number?.Length > 0)
                {
                    Number = Number.Substring(0, Number.Length - 1);
                }
            },
            args => Number?.Length > 0);
            DialPadAudioCallCommand = new RelayCommand(async args =>
            {
                await Call(Number);
            },
            args => Number?.Length > 2);

            DialPadVideoCallCommand = new RelayCommand(args =>
            {

            },
            args => Number?.Length > 2);
            HangUpCommand = new RelayCommand(async args =>
            {
                //Host.StopRinging();
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    await ui.DoWhileBusy(async () =>
                    {
                        if (_userPhone?.IsIncoming == true && _userPhone?.IsInCall == false) await _userPhone.Reject();
                        if (_terminalPhone?.IsIncoming == true && _terminalPhone.IsInCall == false) await _terminalPhone.Reject();
                        if (_userPhone?.IsInCall == true || _userPhone?.IsBusy == true) await _userPhone.HangUp();
                        if (_terminalPhone?.IsInCall == true || _terminalPhone?.IsBusy == true) await _terminalPhone.HangUp();
                    });
                }
            });

            SpeedDialCallCommand = new RelayCommand(async args =>
            {
                await Call(args.ToString());
            });

            CallInProgressKeyPressCommand = new RelayCommand(async args =>
            {
                await _currentPhone.StartDtmf(args.ToString());
                await Task.Delay(600);
                await _currentPhone.StopDtmf(args.ToString());
            });

            CallInProgressAnswerCommand = new RelayCommand(args =>
            {
                _currentPhone?.Answer();
            },
            args =>
            {
                return _currentPhone?.IsIncoming == true;
            });
            CallInProgressVideoAnswerCommand = new RelayCommand(args =>
            {

            },
            args =>
            {
                return _currentPhone?.IsIncoming == true;
            });

            CallInProgressMuteCommand = new RelayCommand(args =>
            {
                Muted = true;
                _currentPhone.Mute();
            },
            args => !Muted);

            CallInProgressUnmuteCommand = new RelayCommand(args =>
            {
                Muted = false;
                _currentPhone.Unmute();
            },
            args => Muted);

            RemoveCallHistoryItemCommand = new AsyncCommand(async args =>
            {
                var item = args as CallLogItem;
                if (item == null) return;
                try
                {
                    await _core.HttpClient.GetObjectAsync<object>("telephone/removecall/" + item.Id + "/");
                    CallHistory.Remove(item);
                }
                catch
                {
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        ui.Toast(_translator.Translate("Action failed. Please try again later."));
                    }
                }
            });
            ManageContactsCommand = new RelayCommand(async args =>
            {
                if (_core.TryGetUserAccountManager(out IUserAccountManager user))
                {
                    if (await user.RequestLoginAsync(_translator.Translate("You must create an account to use Telephone."))
                    && _core.TryGetUiManager(out IUiManager ui))
                    {
                        if (ui.CurrentPage != this) ui.Navigate(this);
                        var source = new TaskCompletionSource<bool>();
                        var vm = new ContactsPageViewModel(_userSpeedDials, source, _core);
                        ui.Navigate(vm, false);
                        var res = await source.Task;
                        if (ui.CurrentPage != this)
                        {
                            ui.Navigate(this);
                        }
                        if (res)
                        {
                            IsBusy = true;
                            try
                            {
                                await GetUserDialsAsync();
                            }
                            finally
                            {
                                IsBusy = false;
                            }
                        }
                    }
                }
            });
        }

        bool _muted = false;
        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                if (value) _currentPhone?.Mute();
                else _currentPhone?.Unmute();
                OnPropertyChanged();
            }
        }

        public override async void Activate()
        {

            if (_loadingTask != null)
            {
                await _loadingTask;
            }
            if (!AvailabilityEnabled && _core.TryGetUiManager(out IUiManager ui))
            {
                ui.Toast(new Translator("Telephone").Translate("Your availability is set to OFF. You cannot receive incoming calls. Set your availability back on to receive incoming calls"));
            }
            try
            {
                await UpdateTelephoneLabel();
            }
            catch (Exception ex)
            {
                //Host.Logger.Error(this, "Telephone configuration: " + ex.Message);
                //Host.ThemeManager.Toast("Telephone is not configured. Please contact the administrator.");
                //Host.GoBack();
            }
            if (_core.TryGetUiManager(out IUiManager ui2))
            {
                ui2.PreviewKeyDown += Ui_PreviewKeyDown;
            }

            //var nots = _notifications.ToList();
            //_notifications.Clear();
            //foreach (var c in nots)
            //{
            //    Host.ThemeManager.Refrain(c);
            //}

            //Gma.UserActivityMonitor.HookManager.KeyDown -= HookManager_KeyDown2;
            //Gma.UserActivityMonitor.HookManager.KeyUp -= HookManager_KeyUp2;
            //Gma.UserActivityMonitor.HookManager.KeyDown += HookManager_KeyDown2;
            //Gma.UserActivityMonitor.HookManager.KeyUp += HookManager_KeyUp2;
        }

        public override void Deactivate()
        {
            if (_core.TryGetUiManager(out IUiManager ui2))
            {
                ui2.PreviewKeyDown -= Ui_PreviewKeyDown;
                if (_userPhone?.IsBusy == true || _terminalPhone?.IsBusy == true)
                {
                    ui2.AddNavigationBarControl(new CallInProgressButtonViewModel()
                    {
                        TelephonePage = this
                    });
                }
            }
        }

        private void Ui_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.D3 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                DialPadKeyPressCommand?.Execute("#");
            }
            else if (e.Key == Key.Multiply || (e.Key == Key.D8 && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
            {
                DialPadKeyPressCommand?.Execute("*");
            }
            else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && (e.Key == Key.D4 || e.Key == Key.D3))
            {
                if (e.Key == Key.D4)
                {
                    DialPadAudioCallCommand?.Execute(null);
                }
                else if (e.Key == Key.D3)
                {
                    HangUpCommand?.Execute(null);
                }
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                DialPadKeyPressCommand?.Execute(e.Key.ToString().Replace("D", "".Replace("NumPad", "")));
            }
            else if (e.Key == Key.Back || e.Key == Key.BrowserBack)
            {
                DialPadBackspaceCommand?.Execute(null);
            }

        }

        private async Task UpdateTelephoneLabel()
        {
            var hasService = true;
            var isFree = true;
            try
            {
                if (_core.TryGetBilling(out IBillingManager billing))
                {
                    hasService = _core.UserService.User.Id != null
                        && (await billing.GetServiceForQuantityAsync(TELEPHONE)) != null;
                    isFree = billing.IsPluginFree(TELEPHONE);
                }
                if (_core.UserService.User.Id == null)
                {
                    if (isFree)
                    {
                        if (_settings.Settings.RequiresUserSignedIn)
                        {
                            CurrentServiceSelectedIndex = 2; //SignInBlock
                        }
                        else
                        {
                            CurrentServiceSelectedIndex = 3; // empty
                        }
                    }
                    else
                    {
                        CurrentServiceSelectedIndex = 0; //BuyServicesAndSignInBlock
                    }
                }
                else
                {
                    if (isFree || hasService)
                    {
                        CurrentServiceSelectedIndex = 3; // empty
                    }
                    else if (!hasService)
                    {
                        CurrentServiceSelectedIndex = 1; //BuyServicesAndSignInBlock
                    }
                }
                _tiles.Clear();

                if (!string.IsNullOrEmpty(_settings.TerminalAccount?.Username) && !_settings.Settings.RequiresUserSignedIn ||
                    (string.IsNullOrEmpty(_settings.UserAccount?.Username) && _core.UserService.User.Id != null))
                {

                    CurrentNumber = _settings.TerminalAccount?.DisplayNumber;
                    CurrentNumberSelectedIndex = 1;
                    _tiles.Add(new LiveTileFrame(new TileYourNumberIsViewModel(CurrentNumber), 5000));
                    //tile1.number = resp.TerminalAccount?.DisplayNumber;
                    //_myButton?.Frames.Add(tile1);
                }
                else if (!string.IsNullOrEmpty(_settings.UserAccount?.Username) && _settings.Settings.RequiresUserSignedIn)
                {

                    CurrentNumber = _settings.UserAccount?.DisplayNumber;
                    CurrentNumberSelectedIndex = 1;

                    _tiles.Add(new LiveTileFrame(new TileYourNumberIsViewModel(CurrentNumber), 5000));
                    //tile1.number = _settings.UserAccount?.DisplayNumber;
                    //_myButton?.Frames.Add(tile1);
                }
                else
                {
                    CurrentNumberSelectedIndex = 0;
                }

            }
            catch
            {

            }

        }

        void StartServiceMonitor(Service service)
        {
            if (_serviceMonitor != null)
            {
                _serviceMonitor.StopMonitor();
            }

            if (_core.TryGetBilling(out IBillingManager bill))
            {
                _serviceMonitor = bill.CreateServiceMonitor();
                _serviceMonitor.ServiceExpired += _serviceMonitor_ServiceExpired;
                _serviceMonitor.Monitor(service);
            }
        }

        private void _serviceMonitor_ServiceExpired(object sender, Service e)
        {
            _currentPhone?.HangUp();
        }

        internal async Task Call(string number2, bool video = false)
        {
            if (_terminalPhone?.IsBusy == true || _userPhone?.IsBusy == true) return;

            if (string.IsNullOrEmpty(number2)) return;

            //_mediaWasPaused = Host.MediaPlayer.IsPlaying;
            //if (_mediaWasPaused) Host.MediaPlayer.Stop();

            _wasIncoming = false;
            Number = number2;
            //todo number.Text = number2;
            if (_terminalSpeedDials != null && _terminalSpeedDials.Any(d => d.Number == number2))
            {
                var forceVideo = _terminalSpeedDials.First(d => d.Number == number2).VideoCall;
                await CallWithLine(_terminalPhone, number2, forceVideo || video);
            }
            else if (!_settings.Settings.RequiresUserSignedIn && (!_core.TryGetBilling(out IBillingManager bill) || bill.IsPluginFree(TELEPHONE)))
            {

                if (_core.UserService.User.Id == null && _terminalPhone != null)
                    await CallWithLine(_terminalPhone, number2, video);
                else if (_userPhone != null) await CallWithLine(_userPhone, number2, video);


            }
            else
            {

                if (_core.TryGetUserAccountManager(out IUserAccountManager user))
                {
                    if (_core.UserService.User.Id == null && !await user.RequestLoginAsync("You must create an account to use Telephone."))
                    {
                        return;
                    }
                    if (_core.TryGetUiManager(out IUiManager ui))
                    {
                        if (ui.CurrentPage != this)
                        {
                            //CloseWindow();
                            ui.Navigate(this);
                        }
                    }

                    if (_core.TryGetBilling(out IBillingManager billing) && billing.IsPluginFree(TELEPHONE))
                    {
                        if (_core.UserService.User.Id == null || _userPhone == null)
                            await CallWithLine(_terminalPhone, number2, video);
                        else await CallWithLine(_userPhone, number2, video);
                    }
                    else
                    {
                        try
                        {

                            if (_core.TryGetBilling(out IBillingManager billing2))
                            {
                                var service = await billing.GetOrRequestServiceAsync("Telephone requires service.", TELEPHONE);
                                _currentService = null;
                                if (service != null)
                                {
                                    if (service.Quantity == -1 || service.Quantity > 0)
                                    {
                                        _currentService = service;
                                        if (_userPhone == null)
                                            await CallWithLine(_terminalPhone, number2, video);
                                        else await CallWithLine(_userPhone, number2, video);
                                    }
                                    else
                                    {
                                        _ = Call(number2, video);
                                    }

                                }
                            }
                        }
                        catch
                        {
                            ui.Toast(_translator.Translate("Unable to get service information due to network problem. Try again later."));
                        }

                    }

                }
            }
        }

        TelephoneBase _currentPhone;
        async Task CallWithLine(TelephoneBase telephone, string number, bool video = false)
        {
            StatusText = "";
            _callStart = DateTime.Now.AddYears(100);
            _lastCallStartTime = DateTime.Now;
            //CallPage.LocalVideoVisibility = video ? Visibility.Visible : Visibility.Collapsed;
            //Host.AudioManager.MicrophoneVolume = 1;
            try
            {
                if (_settings.Settings.DigitConfiguration?.Any() == true)
                {
                    var first = _settings.Settings.DigitConfiguration.FirstOrDefault(d => d.Length == number.Length);
                    if (first != null)
                    {
                        number = first.Digits + number;
                    }
                }
                _currentPhone = telephone;
                if (_core.TryGetUiManager(out IUiManager ui))
                {
                    await ui.DoWhileBusy(async () =>
                    {
                        await telephone.Call(number, video);
                    });
                }
                else
                {
                    await telephone.Call(number, video);
                }
                //MyDialer.ClearText();
            }
            catch (Exception ex)
            {
                _core.Logger.Debug(this, ex.Message);
            }
        }

        int _currentNumberSelectedIndex;
        public int CurrentNumberSelectedIndex
        {
            get => _currentNumberSelectedIndex;
            set
            {
                _currentNumberSelectedIndex = value;
                OnPropertyChanged();
            }
        }

        public GetVoipSettingsResponse Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }


        int _currentServiceSelectedIndex;
        public int CurrentServiceSelectedIndex
        {
            get => _currentServiceSelectedIndex;
            set
            {
                _currentServiceSelectedIndex = value;
                OnPropertyChanged();
            }
        }

        private TelephoneAccount _terminalAccount, _userAccount;

        public TelephoneAccount TerminalAccount
        {
            get => _terminalAccount;
            set
            {
                _terminalAccount = value;
                OnPropertyChanged();
            }
        }

        async Task<TelephoneBase> RegisterPhone(ITelephoneAccount account, TelephoneBase telephone)
        {
            if (telephone != null)
            {
                await telephone.Unregister();
                telephone.Dispose();

            }
            return await CreatePhone(account);
        }

        public TelephoneAccount UserAccount
        {
            get => _userAccount;
            set
            {
                _userAccount = value;
                OnPropertyChanged();
            }
        }

        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }


        GetVoipSettingsResponse _settings;

        public ObservableCollection<SpeedDial> TerminalSpeedDials
        {
            get => _terminalSpeedDials;
            set
            {
                _terminalSpeedDials = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UserSpeedDial> UserSpeedDials
        {
            get => _userSpeedDials;
            set
            {
                _userSpeedDials = value;
                OnPropertyChanged();
            }
        }

        bool _callInProgress;
        public bool CallInProgress
        {
            get => _callInProgress;
            set
            {
                _callInProgress = value;
                OnPropertyChanged();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CallInProgressAnswerCommand.RaiseCanExecuteChanged();
                    CallInProgressVideoAnswerCommand.RaiseCanExecuteChanged();
                });
            }
        }



        private bool _availabilityEnabled = true;
        public bool AvailabilityEnabled
        {
            get => _availabilityEnabled;
            set
            {
                _availabilityEnabled = value;
                if (!AvailabilityEnabled && _core.TryGetUiManager(out IUiManager ui))
                {
                    ui.Toast(new Translator("Telephone").Translate("Your availability is set to OFF. You cannot receive incoming calls. Set your availability back on to receive incoming calls"));
                }
                OnPropertyChanged();
            }
        }

        private string _number;
        public string Number
        {
            get => _number;
            set
            {
                _number = value;
                OnPropertyChanged();
            }
        }

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        string _currentNumber;
        public string CurrentNumber
        {
            get => _currentNumber;
            set
            {
                _currentNumber = value;
                OnPropertyChanged();
            }
        }

        ObservableCollection<CallLogItem> _callHistory = new ObservableCollection<CallLogItem>();
        public ObservableCollection<CallLogItem> CallHistory
        {
            get => _callHistory;
            set
            {
                _callHistory = value;
                OnPropertyChanged();
            }

        }

        private async Task<TelephoneBase> CreatePhone(ITelephoneAccount account)
        {
            if (account == null) return null;
            TelephoneBase phone = null;
            var plugins = _core.PluginLoader.GetPlugins<ITelephoneEnginePlugin>();
            if (!plugins.Any())
            {
                _core.Logger.Error(this, "No telephone engine plugins are loaded");
                return null;
            }
            var selected = plugins.FirstOrDefault(p => p.SupportsType(account.VoipType));
            if (selected == null)
            {
                _core.Logger.Error(this, $"No telephone engine plugins found that can support '{account.VoipType}'");
                return null;
            }
            phone = selected.CreateInstance(account);
            AttachHandlers(phone);
            await phone.Register();
            return phone;
        }

        DateTime _callStart;
        DateTime _callEnd;

        private void AttachHandlers(TelephoneBase telephone)
        {
            telephone.IncomingCall += Telephone_IncomingCall;
            telephone.Ringing += Telephone_Ringing;
            telephone.Answered += Telephone_Answered;
            telephone.Trying += Telephone_Trying;
            telephone.Closed += Telephone_Closed;
            telephone.Cancelled += Telephone_Cancelled;
            telephone.MissedCall += Telephone_MissedCall;
            telephone.Failed += Telephone_Failed;
            telephone.Rejected += Telephone_Rejected;
            telephone.Busy += Telephone_Busy;
        }

        private async void Telephone_Busy(object sender, string e)
        {
            OnCallEnded();
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.Toast(_translator.Translate("{0} was busy..", e));
            }
            await AddCallLogItem((_wasIncoming) ? CallDirection.Incoming : CallDirection.Outgoing, CallStatus.Busy, e,
               _callStart, _callEnd);
        }

        private async void Telephone_Rejected(object sender, string e)
        {
            OnCallEnded();
            await AddCallLogItem((_wasIncoming) ? CallDirection.Incoming : CallDirection.Outgoing, CallStatus.Busy, e,
               _callStart, _callEnd);
        }

        private async void Telephone_Failed(object sender, string e)
        {
            OnCallEnded();
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.Toast(_translator.Translate("Call to {0} failed", e));
            }
            await AddCallLogItem((_wasIncoming) ? CallDirection.Incoming : CallDirection.Outgoing, CallStatus.Failed, e,
               _callStart, _callEnd);
        }

        private async void Telephone_MissedCall(object sender, string e)
        {
            OnCallEnded();
            await AddCallLogItem((_wasIncoming) ? CallDirection.Incoming : CallDirection.Outgoing, CallStatus.Missed, e,
               _callStart, _callEnd);
        }

        private void Telephone_Answered(object sender, string e)
        {
            CallInProgress = true;
            _callStart = DateTime.Now;
            _lastCallStartTime = DateTime.Now;
            //CallPage.HandleCall(dials.Any(d => d.Number == ee) ? dials.First(d => d.Number == ee).Label : ee);
            //CallPage.SetLocalVideo(telephone.LocalVideoControl);
            //CallPage.SetRemoteVideo(telephone.RemoteVideoControl);

            //Host.StopRinging();
            StartCount();
            StatusText = _translator.Translate("Call Answered!");
            //CallPage.SetState(InCallState.Outgoing);
            //CallPage.Visibility = Visibility.Visible;
            ConfigureAudioDevicesVolume();
        }

        private void Telephone_Ringing(object sender, string e)
        {
            CallInProgress = true;
            StatusText = _translator.Translate("Ringing...");
        }

        private async void Telephone_Cancelled(object sender, string e)
        {
            OnCallEnded();
            await AddCallLogItem((_wasIncoming) ? CallDirection.Incoming : CallDirection.Outgoing, CallStatus.Cancelled, e,
                _callStart, _callEnd);
        }

        private async void Telephone_Closed(object sender, string e)
        {
            OnCallEnded();

            await ConsumeAsync();
            _currentService = null;
            await AddCallLogItem((_wasIncoming) ? CallDirection.Incoming : CallDirection.Outgoing, CallStatus.Successful, e,
                _callStart, _callEnd);

        }

        void OnCallEnded()
        {

            _callEnd = DateTime.Now;
            CallInProgress = false;
            StopCount();
            Number = "";
        }

        private void Telephone_Trying(object sender, string e)
        {
            CallInProgress = true;
            StatusText = _translator.Translate("Calling {0}", e);
        }

        private async void Telephone_IncomingCall(object sender, string e)
        {

            var telephone = sender as TelephoneBase;
            if (!_core.TryGetUiManager(out IUiManager ui))
            {
                _core.Logger.Warn(this, "Incoming call rejected. No UI available.");
                await telephone?.Reject();
                return;
            }

            var hasService = true;
            if (_core.UserService.User.Id != null && _core.TryGetBilling(out IBillingManager billing))
            {
                hasService = billing.IsPluginFree(TELEPHONE)
                    || (await billing.GetActiveUserServicesAsync()).Any(s => s.Plugin == TELEPHONE);
            }


            if (telephone == _userPhone)
            {
                if (_terminalPhone != null)
                {
                    if (_terminalPhone.IsInCall || _terminalPhone.IsBusy || _terminalPhone.IsIncoming)
                    {
                        _autoRejected = true;
                        await telephone.Reject();
                        return;
                    }
                }
            }
            else
            {
                if (_userPhone != null)
                {
                    if (_userPhone.IsInCall || _userPhone.IsBusy || _userPhone.IsIncoming)
                    {
                        _autoRejected = true;
                        await telephone.Reject();
                        return;
                    }
                }
            }

            if (_core.UserService.User.Id == null && !hasService)
            {
                if (e.Length > 8)
                {
                    _autoRejected = true;
                    ui.Toast(new Translator("Telephone").Translate("You just missed an incoming call. To activate your extention Buy Services or Sign in."));
                    await telephone.Reject();
                    return;
                }
            }
            else if (!hasService)
            {
                ui.Toast(new Translator("Telephone").Translate("You just missed an incoming call. To activate your extention Buy Services or Sign in."));
                _autoRejected = true;
                await telephone.Reject();
                return;
            }
            if (!telephone.IsIncoming) return; //async


            if (!AvailabilityEnabled)
            {
                if (_terminalSpeedDials.All(d => d.Number != e) || !_terminalSpeedDials.First(d => d.Number == e).OverrideBusySettings)
                {
                    await _userPhone.Reject();
                    await _terminalPhone.Reject();
                    return;
                }
            }
            if ((telephone == _userPhone && _terminalPhone?.IsBusy == true) || (telephone == _terminalPhone && _userPhone?.IsBusy == true))
            {
                if (telephone == _userPhone) await _userPhone.Reject();
                else await _terminalPhone.Reject();
                return;
            }
            _lastCallStartTime = DateTime.Now;
            //todo Host.Window.Unlock();
            //todo _mediaWasPaused = Host.MediaPlayer.IsPlaying;
            //todo if (_mediaWasPaused) Host.MediaPlayer.Stop();



            //todo wasIncoming = true;
            //todo CurrentNumber = ee;
            //todo decline.Visibility = Visibility.Visible;

            //Host.StartRinging();
            _currentPhone = telephone;
            StatusText = _translator.Translate("Incoming call from {0} ...", e);
            _wasIncoming = true;
            CallInProgress = true;

            if (ui.CurrentPage != this)
            {
                //todo ShowWindow();
            }
        }

        void ConfigureAudioDevicesVolume()
        {
            //Host.AudioManager.MicrophoneVolume = 1;
            //Host.AudioManager.MaxDevicesExceptDefault();
            //if (Host.AudioManager.SpeakersVolume <= 0.01)
            //{
            //    Host.AudioManager.SpeakersVolume = .5F;
            //}
        }
        System.Timers.Timer _counter;
        int _inCallSeconds;
        private void StartCount()
        {
            _inCallSeconds = 0;
            if (_counter == null)
            {
                _counter = new System.Timers.Timer { Interval = 1000 };
                _counter.Elapsed += async (oo, ee) =>
                {
                    _inCallSeconds++;
                    if (_currentService != null && _currentService.Quantity > 0 &&
                        _currentService.Quantity - TimeSpan.FromSeconds(_inCallSeconds).Minutes <= 0)
                    {
                        await _userPhone.HangUp();
                        await _terminalPhone.HangUp();
                    }
                    try
                    {
                        if (_currentService != null && _currentService.Quantity > 0 &&
                            _currentService.Quantity - TimeSpan.FromSeconds(_inCallSeconds).Minutes <= 1)
                        {
                            StatusText = string.Format("{0} - {1}",
                                new TimeSpan(0, 0, _inCallSeconds).ToString("hh':'mm':'ss"),
                                _translator.Translate("Call will close automatically in 1 minute"));

                        }
                        else
                        {
                            StatusText = new TimeSpan(0, 0, _inCallSeconds).ToString("hh':'mm':'ss");

                        }
                        if (_inCallSeconds % 60 == 0 && _currentService != null)
                        {
                            try
                            {
                                if (_core.TryGetBilling(out IBillingManager billing))
                                {
                                    //var service = await billing.GetActiveUserServicesSilently("Telephone");
                                    //if (service != null) return;
                                    //await _userPhone.HangUp();
                                    //await _terminalPhone.HangUp();
                                }

                            }
                            catch
                            {
                            }

                        }
                        if (_inCallSeconds >= 60)
                        {
                            await ConsumeAsync();
                        }
                    }
                    catch
                    {
                    }
                };
            }
            if (_currentService != null)
            {
                StartServiceMonitor(_currentService);
            }
            _counter.Start();
        }

        async Task ConsumeAsync()
        {
            if (_currentService == null) return;
            var seconds = _inCallSeconds;

            if (_core.TryGetBilling(out IBillingManager bill)
            && await bill.ConsumeQuantityAsync(TELEPHONE, (int)Math.Ceiling(TimeSpan.FromSeconds(_inCallSeconds).TotalMinutes))) ;
            {
                _inCallSeconds -= seconds;
            }
        }

        private void StopCount()
        {
            _serviceMonitor?.StopMonitor();
            if (_counter == null) return;
            _counter.Stop();
            _counter.Enabled = false;
            _counter.Dispose();
            _counter = null;
        }
        AppBarControlViewModel _appBar;
        Task _loadingTask;
        internal async Task GetSettingsAsync()
        {
            var source = new TaskCompletionSource<object>();
            _loadingTask = source.Task;
            try
            {

                IsBusy = true;
                var settingsResponse = await _core.HttpClient.GetObjectAsync<GetVoipSettingsResponse>("get_voip_settings/");
                if (!settingsResponse.Success)
                {
                    _core.Logger.Debug(this, "Failed to get voip settings: " + (settingsResponse.Error));
                    return;
                }
                Settings = settingsResponse.Result;
                TerminalSpeedDials = new ObservableCollection<SpeedDial>(
                    _settings
                        .Categories
                        .Telephone
                        .SpeedDialCategories
                        .SelectMany(s => s.SpeedDials.Where(d => d.SpeedDial.Visible).Select(sd => sd.SpeedDial)).ToList());
                if (TerminalAccount?.Compare(_settings.TerminalAccount) != true)
                {
                    TerminalAccount = _settings.TerminalAccount;
                    _terminalPhone = await RegisterPhone(TerminalAccount, _terminalPhone);
                }
                if (UserAccount?.Compare(_settings.UserAccount) != true)
                {
                    UserAccount = _settings.UserAccount;
                    _userPhone = await RegisterPhone(UserAccount, _userPhone);
                }
                var dials = _settings
                        .Categories
                        .Telephone
                        .SpeedDialCategories
                        .SelectMany(s => s.SpeedDials.Select(sd => sd.SpeedDial)).ToList();
                if (_appBar == null && _core.TryGetAppBar(out IAppBar bar))
                {

                    _appBar = new AppBarControlViewModel();
                    _appBar.SpeedDials = dials;
                    _appBar.TelephonePage = this;
                    bar.AddButton(_appBar);
                }
                else if (_appBar != null)
                {
                    _appBar.SpeedDials = dials;
                }
                if (_core.UserService.User.Id != null)
                {
                    await GetUserInfoAsync();
                }
                else
                {
                    UserSpeedDials = new ObservableCollection<UserSpeedDial>();
                    CallHistory = new ObservableCollection<CallLogItem>();
                }
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, "Telephone failed to get settings: " + ex.Message);
                await Task.Delay(new Random().Next(20000, 40000));
                await GetSettingsAsync();
            }
            finally
            {
                IsBusy = false;
                source.TrySetResult(null);
                _loadingTask = null;
            }
            await UpdateTelephoneLabel();
        }
        private async Task GetUserDialsAsync()
        {
            var resp = await _core.HttpClient.GetCookieAsync<ObservableCollection<UserSpeedDial>>("Telephone");
            if (resp.Success)
            {
                UserSpeedDials = resp.Result;
            }

        }

        async Task GetUserInfoAsync()
        {
            await GetUserDialsAsync();
            await GetCallHistoryAsync();

        }

        public async Task AddCallLogItem(CallDirection direction, CallStatus status, string number, DateTime callStart, DateTime callEnd)
        {
            if (string.IsNullOrEmpty(number)) return;
            if (callStart > callEnd)
            {
                callStart = callEnd;
            }
            if (_settings.Settings.DigitConfiguration?.Any() == true)
            {
                var first = _settings.Settings.DigitConfiguration.FirstOrDefault(d => d.Length == number.Length - d.Digits.ToString().Length);
                if (first != null)
                {
                    number = number.Substring(first.Digits.ToString().Length, first.Length);
                }
            }
            if (callStart == null) callStart = _lastCallStartTime;
            var display = number;
            var td = TerminalSpeedDials.FirstOrDefault(d => d.Number == number);
            if (td != null) display = td.Label;
            var duration = (int)callEnd.Subtract(callStart).TotalSeconds;
            var item = new CallLogItem()
            {
                Direction = direction,
                Status = status,
                Number = number,
                Seconds = duration,
                TimeStamp = callStart,
                Display = display
            };
            try
            {
                item.Id = await AddCallLogToServerAsync(direction, status, number, duration, CallType.Free);
                if (_core.UserService.User.Id == null) return;
                CallHistory.Insert(0, item);
            }
            catch
            {
                //mistakes were made
            }

        }

        private async Task<string> AddCallLogToServerAsync(CallDirection direction, CallStatus status, string number,
            int duration, CallType type)
        {
            var result = await _core.HttpClient.GetObjectAsync<string>(
                "telephone/addcall/",
                new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("direction", direction.ToString()),
                    new KeyValuePair<string, string>("status", status.ToString()),
                    new KeyValuePair<string, string>("duration", duration.ToString()),
                    new KeyValuePair<string, string>("type", type.ToString()),
                    new KeyValuePair<string, string>("number", number),
                    new KeyValuePair<string, string>("timestamp", _lastCallStartTime.ToUniversalTime().ToString("O"))
                }
                );
            return result.Result;

        }

        async Task GetCallHistoryAsync()
        {
            CallHistory.Clear();
            var response = await _core.HttpClient.GetObjectAsync<List<CallLogItem>>("telephone/get_call_history/");

            foreach (var dial in response.Result.OrderByDescending(d => d.TimeStamp).Take(60))
            {
                var td = TerminalSpeedDials.FirstOrDefault(d => d.Number == dial.Number);
                if (td != null)
                {
                    dial.Display = td.Label;
                }
                else dial.Display = dial.Number;
                CallHistory.Add(dial);
            }
        }

        public ICommand DialPadKeyPressCommand { get; }

        public ICommand CallInProgressKeyPressCommand { get; }

        public RelayCommand CallInProgressAnswerCommand { get; }

        public RelayCommand CallInProgressVideoAnswerCommand { get; }

        public ICommand CallInProgressMuteCommand { get; }

        public ICommand CallInProgressUnmuteCommand { get; }

        public RelayCommand DialPadBackspaceCommand { get; }

        public RelayCommand DialPadAudioCallCommand { get; }

        public RelayCommand DialPadVideoCallCommand { get; }

        public ICommand SpeedDialCallCommand { get; }

        public ICommand HangUpCommand { get; }

        public AsyncCommand RemoveCallHistoryItemCommand { get; }

        public ICommand ManageContactsCommand { get; }

        public ICommand BuyServiceCommand { get; }

        public ICommand SignInCommand { get; }
    }
}
