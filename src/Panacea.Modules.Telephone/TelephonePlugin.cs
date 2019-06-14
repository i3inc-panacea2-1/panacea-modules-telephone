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
    class TelephonePlugin : ICallablePlugin
    {
        private readonly PanaceaServices _core;
        GetVoipSettingsResponse _settings;
        TelephonePageViewModel _telephonePage;
        public TelephonePlugin(PanaceaServices core)
        {
            _core = core;
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

        public async Task EndInit()
        {
            _telephonePage = new TelephonePageViewModel(_core);
            await GetSettingsAsync();
        }

        private async Task GetSettingsAsync()
        {
            try
            {
                var settingsResponse = await _core.HttpClient.GetObjectAsync<GetVoipSettingsResponse>("get_voip_settings/");
                if (!settingsResponse.Success)
                {
                    _core.Logger.Debug(this, "Failed to get voip settings: " + (settingsResponse.Error));
                    return;
                }

                _settings = settingsResponse.Result;
                _telephonePage.TerminalAccount = _settings.TerminalAccount;
                _telephonePage.UserAccount = _settings.UserAccount;

            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, "Telephone failed to get settings: " + ex.Message);
            }
        }

        public void Call()
        {
            if(_core.TryGetUiManager(out IUiManager ui))
            {
                ui.Navigate(_telephonePage);
            }
        }
    }
}