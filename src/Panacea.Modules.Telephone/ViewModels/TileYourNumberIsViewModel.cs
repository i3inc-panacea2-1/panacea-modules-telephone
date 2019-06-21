using Panacea.Modules.Telephone.Views;
using Panacea.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.Telephone.ViewModels
{
    [View(typeof(TileYourNumberIs))]
    class TileYourNumberIsViewModel:ViewModelBase
    {
        public string Number { get; set; }
        public TileYourNumberIsViewModel(string number)
        {
            Number = number;
        }
    }
}
