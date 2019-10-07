using Panacea.Controls;
using Panacea.Modularity.Telephone;
using Panacea.Modules.Telephone.Models;
using Panacea.Modules.Telephone.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(AppBarControl))]
    class AppBarControlViewModel : ViewModelBase
    {

        public AppBarControlViewModel()
        {
            ClickCommand = new RelayCommand(args =>
            {
                PopupOpen = !PopupOpen;
            });
            CallCommand = new RelayCommand(args =>
            {
                var dial = args as SpeedDial;
                TelephonePage?.Call(dial.Number);
            });

            HangupCommand = new RelayCommand(args => TelephonePage?.HangUpCommand?.Execute(null));
        }

        TelephonePageViewModel _telephonePage;
        public TelephonePageViewModel TelephonePage
        {
            get => _telephonePage;
            set
            {
                _telephonePage = value;
                OnPropertyChanged();
            }
        }

        bool _popupOpen;
        public bool PopupOpen
        {
            get => _popupOpen;
            set
            {
                _popupOpen = value;
                OnPropertyChanged();
            }
        }

        List<SpeedDial> _speedDials;
        public List<SpeedDial> SpeedDials
        {
            get => _speedDials;
            set
            {
                _speedDials = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand ClickCommand { get; }

        public RelayCommand CallCommand { get; }

        public RelayCommand HangupCommand { get; }

        public override void Deactivate()
        {
            base.Deactivate();
            PopupOpen = false;
        }
    }
}
