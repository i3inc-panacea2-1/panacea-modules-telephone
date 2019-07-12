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
        DispatcherTimer ringTimer;
        public NavigationButtonViewModel(PanaceaServices core)
        {
            ringTimer = new DispatcherTimer();
            ringTimer.Interval = TimeSpan.FromSeconds(5);
            ringTimer.Tick += Timer_Tick;
            _core = core;

            if (_core.GetRelayManager(out IRelayManager relay))
            {
                _relay = relay;
            }
            ClickCommand = new RelayCommand(args =>
            {
                if (_relay!=null && _relay.NurseCallAttached)
                {
                    ringTimer.Start();
                    _relay.SetNurseCallAsync(true);
                }
            });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
                if (_relay != null && _relay.NurseCallAttached)
                {
                    ringTimer.Stop();
                    _relay.SetNurseCallAsync(false);
                }
        }

        private PanaceaServices _core;
        private IRelayManager _relay;

        public ICommand ClickCommand { get; }
    }
}
