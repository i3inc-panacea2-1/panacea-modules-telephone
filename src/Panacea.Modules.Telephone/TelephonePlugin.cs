using Panacea.Core;
using Panacea.Modularity;
using Panacea.Modularity.Billing;
using Panacea.Modularity.Telephone;
using Panacea.Modularity.UiManager;
using Panacea.Modules.Telephone.Models;
using Panacea.Modules.Telephone.ViewModels;
using Panacea.Multilinguality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone
{
    class TelephonePlugin : ITelephonePlugin, ICallablePlugin
    {
        private readonly PanaceaServices _core;
        GetVoipSettingsResponse _settings;
        TelephonePageViewModel _telephonePage;
        Translator _translator = new Translator("Telephone");
        public TelephonePlugin(PanaceaServices core)
        {
            _core = core;
            _telephonePage = new TelephonePageViewModel(core);
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
            return Task.CompletedTask;
        }

        public Task EndInit()
        {
            _ = _telephonePage.GetSettingsAsync();
            return Task.CompletedTask;
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