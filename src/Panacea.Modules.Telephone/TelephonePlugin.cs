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
            _ = GetSettingsAsync();
            return Task.CompletedTask;
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
                _telephonePage = _telephonePage ?? new TelephonePageViewModel(_core, _settings);
                _telephonePage.Settings = _settings;

                _telephonePage.TerminalSpeedDials = _settings.Categories.Telephone.SpeedDialCategories.SelectMany(s => s.SpeedDials.Select(sd=>sd.SpeedDial)).ToList();
                _telephonePage.TerminalAccount = _settings.TerminalAccount;
                _telephonePage.UserAccount = _settings.UserAccount;
            }
            catch (Exception ex)
            {
                _core.Logger.Error(this, "Telephone failed to get settings: " + ex.Message);
                await Task.Delay(new Random().Next(20000, 40000));
                await GetSettingsAsync();
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
            throw new NotImplementedException();
        }
    }
}