using Panacea.Controls;
using Panacea.Modules.Telephone.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(NavigationButton))]
    class NavigationButtonViewModel:ViewModelBase
    {
        public NavigationButtonViewModel()
        {
            ClickCommand = new RelayCommand(args =>
            {

            });
        }

        public ICommand ClickCommand { get; }
    }
}
