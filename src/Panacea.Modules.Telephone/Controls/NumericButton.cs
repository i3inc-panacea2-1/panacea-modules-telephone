using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Panacea.Modules.Telephone.Controls
{
    public class NumericButton : Button
    {
        public static readonly DependencyProperty LabelProperty =
             DependencyProperty.Register("Label", typeof(string),
             typeof(NumericButton), new FrameworkPropertyMetadata(null));


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty BorderVisibilityProperty =
             DependencyProperty.Register("BorderVisibility", typeof(Visibility),
             typeof(NumericButton), new FrameworkPropertyMetadata(Visibility.Visible));


        public Visibility BorderVisibility
        {
            get { return (Visibility)GetValue(BorderVisibilityProperty); }
            set { SetValue(BorderVisibilityProperty, value); }
        }

        static NumericButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericButton), new FrameworkPropertyMetadata(typeof(NumericButton)));
        }
    }
}
