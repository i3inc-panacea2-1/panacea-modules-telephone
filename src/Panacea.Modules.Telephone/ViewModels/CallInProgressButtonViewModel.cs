using Panacea.Modules.Telephone.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(CallInProgressButton))]
    class CallInProgressButtonViewModel:ViewModelBase
    {
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
    }
}
