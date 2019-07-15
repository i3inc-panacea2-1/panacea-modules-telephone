using Panacea.Controls;
using Panacea.Modules.Telephone.Views;
using Panacea.Modularity.Relays;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Panacea.Core;
using System.Windows.Threading;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(NavigationButton))]
    class NavigationButtonViewModel:ViewModelBase
    {
  
        public NavigationButtonViewModel(PanaceaServices core)
        {
            _core = core;

            if (_core.TryGetRelayManager(out IRelayManager relay))
            {
                _relay = relay;
            }

            bool pressed = false;
            ClickCommand = new RelayCommand(async args =>
            {
                if (pressed) return;
                pressed = true;
                try
                {
                    if (_relay != null && _relay.NurseCallAttached)
                    {
                        await _relay.SetNurseCallAsync(true);
                        await Task.Delay(2000);
                        await _relay.SetNurseCallAsync(false);
                    }
                }
                finally
                {
                    pressed = false;
                }
               
            });
        }



        private PanaceaServices _core;
        private IRelayManager _relay;

        public ICommand ClickCommand { get; }
    }
}
