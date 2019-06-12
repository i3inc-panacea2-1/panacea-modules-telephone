using Panacea.Controls;
using Panacea.Modularity.UiManager;
using Panacea.Multilinguality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.ViewModels
{
    class CallPopupViewModel:PopupViewModelBase<bool>
    {
        public TranslatableObject Message { get; }
        public CallPopupViewModel(TranslatableObject message)
        {
            Message = message;
            CallCommand = new RelayCommand(args => SetResult(true));
            CancelCommand = new RelayCommand(args => SetResult(false));
        }

        public RelayCommand CallCommand { get; }

        public RelayCommand CancelCommand { get; }
    }
}
