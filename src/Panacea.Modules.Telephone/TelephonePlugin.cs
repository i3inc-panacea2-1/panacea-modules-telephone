using Panacea.Core;
using Panacea.Modularity;
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
            return Task.CompletedTask;
        }

        public Task EndInit()
        {
            _ = _telephonePage.GetSettingsAsync();
            var hw = _core.GetHardwareManager();
            hw.HandsetStateChanged += Hw_HandsetStateChanged;
            if(_core.TryGetUiManager(out IUiManager ui))
            {
                _navButton = new NavigationButtonViewModel();
                ui.AddNavigationBarControl(_navButton);
            }
            return Task.CompletedTask;
        }

        private void Hw_HandsetStateChanged(object sender, HardwareStatus e)
        {
            if(_core.TryGetUiManager(out IUiManager ui))
            {
                Call();
            }
        }

        public void Call()
        {
            
            if(_core.TryGetUiManager(out IUiManager ui))
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