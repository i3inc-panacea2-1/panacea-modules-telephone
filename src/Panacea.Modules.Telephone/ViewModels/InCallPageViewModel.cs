using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.ViewModels
{
    class InCallPageViewModel:ViewModelBase
    {
        private string _numberText;
        public string NumberText
        {
            get => _numberText;
            set
            {
                _numberText = value;
                OnPropertyChanged();
            }
        }
    }
}
