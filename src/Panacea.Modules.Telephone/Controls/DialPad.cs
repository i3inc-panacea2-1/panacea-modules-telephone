using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Panacea.Modules.Telephone.Controls
{
    class DialPad : Control
    {
        static DialPad()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DialPad), new FrameworkPropertyMetadata(typeof(DialPad)));
        }



        public Visibility VideoCallButtonVisibility
        {
            get { return (Visibility)GetValue(VideoCallButtonVisibilityProperty); }
            set { SetValue(VideoCallButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoCallButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoCallButtonVisibilityProperty =
            DependencyProperty.Register("VideoCallButtonVisibility", typeof(Visibility), typeof(DialPad), new PropertyMetadata(Visibility.Visible));



        public Visibility InputVisibility
        {
            get { return (Visibility)GetValue(InputVisibilityProperty); }
            set { SetValue(InputVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputVisibilityProperty =
            DependencyProperty.Register("InputVisibility", typeof(Visibility), typeof(DialPad), new PropertyMetadata(Visibility.Visible));



        public Visibility ButtonsVisibility
        {
            get { return (Visibility)GetValue(ButtonsVisibilityProperty); }
            set { SetValue(ButtonsVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonsVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonsVisibilityProperty =
            DependencyProperty.Register("ButtonsVisibility", typeof(Visibility), typeof(DialPad), new PropertyMetadata(Visibility.Visible));



        public ICommand KeyPressCommand
        {
            get { return (ICommand)GetValue(KeyPressCommandProperty); }
            set { SetValue(KeyPressCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyPressCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyPressCommandProperty =
            DependencyProperty.Register("KeyPressCommand", typeof(ICommand), typeof(DialPad), new PropertyMetadata(null));



        public ICommand AudioCallCommand
        {
            get { return (ICommand)GetValue(AudioCallCommandProperty); }
            set { SetValue(AudioCallCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AudioCallCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AudioCallCommandProperty =
            DependencyProperty.Register("AudioCallCommand", typeof(ICommand), typeof(DialPad), new PropertyMetadata(null));


        public ICommand VideoCallCommand
        {
            get { return (ICommand)GetValue(VideoCallCommandProperty); }
            set { SetValue(VideoCallCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VideoCallCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoCallCommandProperty =
            DependencyProperty.Register("VideoCallCommand", typeof(ICommand), typeof(DialPad), new PropertyMetadata(null));



    }
}
