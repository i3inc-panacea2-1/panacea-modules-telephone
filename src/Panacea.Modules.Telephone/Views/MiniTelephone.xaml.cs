using Panacea.Modules.Telephone.Models;
using Panacea.Modules.Telephone.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Panacea.Modules.Telephone.Views
{
    /// <summary>
    /// Interaction logic for MiniTelephone.xaml
    /// </summary>
    public partial class MiniTelephone : UserControl
    {
        public MiniTelephone()
        {
            InitializeComponent();
        }




        public ICommand CallCommand
        {
            get { return (ICommand)GetValue(CallCommandProperty); }
            set { SetValue(CallCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CallCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CallCommandProperty =
            DependencyProperty.Register("CallCommand", typeof(ICommand), typeof(MiniTelephone), new PropertyMetadata(null));




        public ICommand HangupCommand
        {
            get { return (ICommand)GetValue(HangupCommandProperty); }
            set { SetValue(HangupCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HangupCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HangupCommandProperty =
            DependencyProperty.Register("HangupCommand", typeof(ICommand), typeof(MiniTelephone), new PropertyMetadata(null));





        public TelephonePageViewModel TelephonePage
        {
            get { return (TelephonePageViewModel)GetValue(TelephonePageProperty); }
            set { SetValue(TelephonePageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TelephonePage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TelephonePageProperty =
            DependencyProperty.Register("TelephonePage", typeof(TelephonePageViewModel), typeof(MiniTelephone), new PropertyMetadata(null));




        public List<SpeedDial> SpeedDials
        {
            get { return (List<SpeedDial>)GetValue(SpeedDialsProperty); }
            set { SetValue(SpeedDialsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SpeedDials.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpeedDialsProperty =
            DependencyProperty.Register("SpeedDials", typeof(List<SpeedDial>), typeof(MiniTelephone), new PropertyMetadata(null));


        Size _size = new Size(0, 0);
        protected override Size MeasureOverride(Size constraint)
        {
            var res = base.MeasureOverride(constraint);
            if(res.Width > _size.Width)
            {
                _size = res;
                return res;
            }
            return _size;
        }
    }
}
