using Panacea.Core;
using Panacea.Modularity.Billing;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Telephone.Models;
using Panacea.Modules.Telephone.Views;
using Panacea.Multilinguality;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(TelephonePage))]
    class TelephonePageViewModel : ViewModelBase
    {
        private readonly PanaceaServices _core;
        private TelephoneBase _terminalPhone, _userPhone;
        bool _autoRejected;
        private List<SpeedDial> _terminalSpeedDials, _userSpeedDials;
        DateTime _lastCallStartTime;

        public TelephonePageViewModel(PanaceaServices core)
        {
            _core = core;
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
        }

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

        public TelephoneAccount UserAccount
        {
            get => _userAccount;
            set
            {
                _userAccount = value;
                OnPropertyChanged();
            }
        }

        private bool _availabilityEnabled;
        public bool AvailabilityEnabled
        {
            get => _availabilityEnabled;
            set
            {
                _availabilityEnabled = value;
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


            if (ui.CurrentPage != this)
            {
                //todo ShowWindow();
            }
        }
    }
}
