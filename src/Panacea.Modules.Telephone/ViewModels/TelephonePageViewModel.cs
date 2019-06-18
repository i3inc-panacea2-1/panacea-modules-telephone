using Panacea.Controls;
using Panacea.Core;
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(TelephonePage))]
    class TelephonePageViewModel : ViewModelBase
    {
        private readonly PanaceaServices _core;
        private TelephoneBase _terminalPhone, _userPhone;
        bool _autoRejected, _wasIncoming;
        private List<SpeedDial> _terminalSpeedDials, _userSpeedDials;
        DateTime _lastCallStartTime;
        Translator _translator = new Translator("Telephone");
        Service _currentService;

        public TelephonePageViewModel(PanaceaServices core, GetVoipSettingsResponse settings)
        {
            _core = core;
            _settings = settings;
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
        }

        public override async void Activate()
        {

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

        private async Task UpdateTelephoneLabel()
        {
            var hasService = true;
            var isFree = true;
            try
            {
                if (_core.TryGetBilling(out IBillingManager billing))
                {
                    hasService = _core.UserService.User.Id != null
                        && (await billing.GetServiceForQuantityAsync("Telephone")) != null;
                    isFree = billing.IsPluginFree("Telephone");
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
                    if (isFree)
                    {
                        CurrentServiceSelectedIndex = 3; // empty
                    }
                    else if (!hasService)
                    {
                        CurrentServiceSelectedIndex = 1; //BuyServicesAndSignInBlock
                    }
                }


                if (!string.IsNullOrEmpty(_settings.TerminalAccount?.Username) && !_settings.Settings.RequiresUserSignedIn ||
                    (string.IsNullOrEmpty(_settings.UserAccount?.Username) && _core.UserService.User.Id != null))
                {

                    CurrentNumber = _settings.TerminalAccount?.DisplayNumber;
                    CurrentNumberSelectedIndex = 1;
                    //tile1.number = resp.TerminalAccount?.DisplayNumber;
                    //_myButton?.Frames.Add(tile1);
                }
                else if (!string.IsNullOrEmpty(_settings.UserAccount?.Username) && _settings.Settings.RequiresUserSignedIn)
                {

                    CurrentNumber = _settings.UserAccount?.DisplayNumber;
                    CurrentNumberSelectedIndex = 1;
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


        private async Task Call(string number2, bool video = false)
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
            else if (!_settings.Settings.RequiresUserSignedIn && (!_core.TryGetBilling(out IBillingManager bill) || bill.IsPluginFree("Telephone")))
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

                    if (_core.TryGetBilling(out IBillingManager billing) && billing.IsPluginFree("Telephone"))
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
                                var service = await billing.GetOrRequestServiceAsync("Telephone requires service.", "Telephone");

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
                                else
                                {
                                    _ = Call(number2, video);
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

        async Task CallWithLine(TelephoneBase telephone, string number, bool video = false)
        {
            StatusText = "";
            _lastCallStartTime = DateTime.Now;
            //CallPage.LocalVideoVisibility = video ? Visibility.Visible : Visibility.Collapsed;
            //Host.AudioManager.MicrophoneVolume = 1;
            try
            {
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
                if (_terminalAccount?.Compare(value) == true)
                    return;
                _terminalAccount = value;
                var local = _terminalAccount;
                OnPropertyChanged();
                if (_terminalPhone != null)
                {
                    _terminalPhone.Unregister()
                        .ContinueWith(t =>
                        {
                            if (local != _terminalAccount) return;
                            _terminalPhone.Dispose();
                            CreatePhone(_terminalAccount)
                            .ContinueWith(t2 =>
                            {
                                _terminalPhone = t2.Result;
                            });
                        });
                }
                else
                {
                    if (local != _terminalAccount) return;
                    CreatePhone(_terminalAccount)
                    .ContinueWith(t2 =>
                    {
                        _terminalPhone = t2.Result;
                    });
                }
            }
        }

        public TelephoneAccount UserAccount
        {
            get => _userAccount;
            set
            {

                if (_userAccount?.Compare(value) == true)
                    return;
                _userAccount = value;
                var local = _userAccount;
                OnPropertyChanged();
                if (_userPhone != null)
                {
                    _userPhone.Unregister()
                        .ContinueWith(t =>
                        {
                            if (local != _userAccount) return;
                            _userPhone.Dispose();
                            CreatePhone(_userAccount)
                            .ContinueWith(t2 =>
                            {
                                _userPhone = t2.Result;
                            });
                        });
                }
                else
                {
                    if (local != _userAccount) return;
                    CreatePhone(_userAccount)
                    .ContinueWith(t2 =>
                    {
                        _userPhone = t2.Result;
                    });
                }
            }
        }


        GetVoipSettingsResponse _settings;

        public List<SpeedDial> TerminalSpeedDials
        {
            get => _terminalSpeedDials;
            set
            {
                _terminalSpeedDials = value;
                OnPropertyChanged();
            }
        }

        public List<SpeedDial> UserSpeedDials
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
            }
        }



        private bool _availabilityEnabled = true;
        public bool AvailabilityEnabled
        {
            get => _availabilityEnabled;
            set
            {
                _availabilityEnabled = value;
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

        private async Task<TelephoneBase> CreatePhone(TelephoneAccount account)
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

        private void Telephone_Busy(object sender, string e)
        {
            OnCallEnded();
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.Toast(_translator.Translate("{0} was busy..", e));
            }
        }

        private void Telephone_Rejected(object sender, string e)
        {
            OnCallEnded();
        }

        private void Telephone_Failed(object sender, string e)
        {
            OnCallEnded();
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                ui.Toast(_translator.Translate("Call to {0} failed", e));
            }

        }

        private void Telephone_MissedCall(object sender, string e)
        {
            OnCallEnded();
        }

        private void Telephone_Answered(object sender, string e)
        {
            CallInProgress = true;

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

        private void Telephone_Cancelled(object sender, string e)
        {
            OnCallEnded();
        }

        private void Telephone_Closed(object sender, string e)
        {
            OnCallEnded();
        }

        void OnCallEnded()
        {
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
                hasService = billing.IsPluginFree("Telephone")
                    || (await billing.GetActiveUserServicesAsync()).Any(s => s.Plugin == "Telephone");
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
            if ((sender == _userPhone && _terminalPhone.IsBusy) || (sender == _terminalPhone && _userPhone.IsBusy))
            {
                if (sender == _userPhone) await _userPhone.Reject();
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
            StatusText = _translator.Translate("Incoming call from {0} ...", e);
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
                    }
                    catch
                    {
                    }
                };
            }
            _counter.Start();
        }

        private void StopCount()
        {
            if (_counter == null) return;
            _counter.Stop();
            _counter.Enabled = false;
            _counter.Dispose();
            _counter = null;
        }

        public ICommand DialPadKeyPressCommand { get; }

        public ICommand DialPadBackspaceCommand { get; }

        public ICommand DialPadAudioCallCommand { get; }

        public ICommand DialPadVideoCallCommand { get; }

        public ICommand SpeedDialCallCommand { get; }

        public ICommand HangUpCommand { get; }
    }
}
