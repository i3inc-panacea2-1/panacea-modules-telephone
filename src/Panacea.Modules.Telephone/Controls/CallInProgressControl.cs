using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Panacea.Modules.Telephone.Controls
{
    public class CallInProgressControl : Control
    {
        static CallInProgressControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CallInProgressControl), new FrameworkPropertyMetadata(typeof(CallInProgressControl)));
        }



        public string CurrentNumber
        {
            get { return (string)GetValue(CurrentNumberProperty); }
            set { SetValue(CurrentNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentNumberProperty =
            DependencyProperty.Register("CurrentNumber", typeof(string), typeof(CallInProgressControl), new PropertyMetadata(null));


    }
}
