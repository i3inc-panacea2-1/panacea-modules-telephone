using Panacea.Core;
using Panacea.Modularity;
using Panacea.Modularity.AppBar;
using Panacea.Modularity.Billing;
using Panacea.Modularity.Hardware;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Telephone.Models;
using Panacea.Modules.Telephone.ViewModels;
using Panacea.Multilinguality;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone
{
    class TelephonePlugin : ITelephonePlugin, ICallablePlugin, ILiveTilesPlugin
    {
        private readonly PanaceaServices _core;
        GetVoipSettingsResponse _settings;
        TelephonePageViewModel _telephonePage;
        Translator _translator = new Translator("Telephone");
        NavigationButtonViewModel _navButton;
        public ObservableCollection<LiveTileFrame> Frames { get; private set; }
        /*
      SpeakerPhoneAudioOutDevice=None
HandsetAudioOutDevice=None
SpeakerPhoneAudioInDevice=None
HandsetPhoneAudioInDevice=None   
    */

        [PanaceaInject("SpeakerPhoneAudioOutDevice", "Set the device to be used as speaker in speakerphone mode. Separate multiple devices with ';'. Case sensitive.", "SpeakerPhoneAudioOutDevice=Realtek;AMD")]
        protected string SpeakerPhoneAudioOutDevice { get; set; } = "None";

        [PanaceaInject("HandsetAudioOutDevice", "Set the device to be used as speaker for handset mode. Separate multiple devices with ';'. Case sensitive.", "HandsetAudioOutDevice=Realtek;AMD")]
        protected string HandsetAudioOutDevice { get; set; } = "None";

        [PanaceaInject("SpeakerPhoneAudioInDevice", "Set the devices to be used as microphone for speakerphone mode. Separate multiple devices with ';'. Case sensitive.", "SpeakerPhoneAudioInDevice=Realtek;AMD")]
        protected string SpeakerPhoneAudioInDevice { get; set; } = "None";

        [PanaceaInject("HandsetAudioInDevice", "Set the devices to be used as microphone for handet mode. Separate multiple devices with ';'. Case sensitive.", "HandsetAudioInDevice=Realtek;AMD")]
        protected string HandsetAudioInDevice { get; set; } = "None";

        public TelephonePlugin(PanaceaServices core)
        {
            _core = core;
            Frames = new ObservableCollection<LiveTileFrame>();
            _telephonePage = new TelephonePageViewModel(core, Frames);
        }

        public Task BeginInit()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        public Task Shutdown()
        {
            if (_navButton != null && _core.TryGetUiManager(out IUiManager ui))
            {
                ui.RemoveNavigationBarControl(_navButton);
            }
            var hw = _core.GetHardwareManager();
            hw.HandsetStateChanged -= Hw_HandsetStateChanged;
            return Task.CompletedTask;
        }

        public Task EndInit()
        {
            _core.UserService.UserLoggedIn += UserService_UserLoggedIn;
            _core.UserService.UserLoggedOut += UserService_UserLoggedOut;
            _ = _telephonePage.GetSettingsAsync();
            var hw = _core.GetHardwareManager();
            hw.HandsetStateChanged += Hw_HandsetStateChanged;
            _telephonePage.SetAudioDevices(
                    hw.HandsetState == HardwareStatus.On ? SpeakerPhoneAudioOutDevice : HandsetAudioOutDevice,
                    hw.HandsetState == HardwareStatus.On ? SpeakerPhoneAudioInDevice : HandsetAudioInDevice);
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                _navButton = new NavigationButtonViewModel(_core);
                ui.AddNavigationBarControl(_navButton);
            }
            
            return Task.CompletedTask;
        }

        private async Task UserService_UserLoggedOut(IUser user)
        {
            await _telephonePage.GetSettingsAsync();
        }

        private async Task UserService_UserLoggedIn(IUser user)
        {
            await _telephonePage.GetSettingsAsync();
        }

        private void Hw_HandsetStateChanged(object sender, HardwareStatus e)
        {
            if (_core.TryGetUiManager(out IUiManager ui))
            {
                Call();
            }
            if(_telephonePage != null)
            {
                _telephonePage.SetAudioDevices(
                    e == HardwareStatus.On ? SpeakerPhoneAudioOutDevice : HandsetAudioOutDevice,
                    e == HardwareStatus.On ? SpeakerPhoneAudioInDevice: HandsetAudioInDevice );
            }
        }

        public void Call()
        {

            if (_core.TryGetUiManager(out IUiManager ui))
            {
                if (_telephonePage != null)
                {
                    ui.Navigate(_telephonePage);
                }
                else
                {
                    ui.Toast(_translator.Translate("Telephone is not yet available. Please, try again shortly"));
                }
            }
        }

        public void Call(string number)
        {

        }
    }
}