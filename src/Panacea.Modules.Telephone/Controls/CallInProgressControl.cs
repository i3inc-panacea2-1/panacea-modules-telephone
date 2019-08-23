using Panacea.Controls;
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
    public class CallInProgressControl : Control
    {
        static CallInProgressControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CallInProgressControl), new FrameworkPropertyMetadata(typeof(CallInProgressControl)));
        }

        public CallInProgressControl()
        {
            ToggleDialPadCommand = new RelayCommand(args =>
            {
                DialPadVisibile = !DialPadVisibile;
            });
        }


        public string CurrentNumber
        {
            get { return (string)GetValue(CurrentNumberProperty); }
            set { SetValue(CurrentNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentNumber.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentNumberProperty =
            DependencyProperty.Register("CurrentNumber", typeof(string), typeof(CallInProgressControl), new PropertyMetadata(null));



        public AsyncCommand HangUpCommand
        {
            get { return (AsyncCommand)GetValue(HangUpCommandProperty); }
            set { SetValue(HangUpCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HangUpCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HangUpCommandProperty =
            DependencyProperty.Register("HangUpCommand", typeof(AsyncCommand), typeof(CallInProgressControl), new PropertyMetadata(null));



        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(CallInProgressControl), new PropertyMetadata(null));






        public bool DialPadVisibile
        {
            get { return (bool)GetValue(DialPadVisibileProperty); }
            set { SetValue(DialPadVisibileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DialPadVisibile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DialPadVisibileProperty =
            DependencyProperty.Register("DialPadVisibile", typeof(bool), typeof(CallInProgressControl), new PropertyMetadata(false));




        public ICommand ToggleDialPadCommand
        {
            get { return (ICommand)GetValue(ToggleDialPadCommandProperty); }
            set { SetValue(ToggleDialPadCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToggledialPadCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToggleDialPadCommandProperty =
            DependencyProperty.Register("ToggleDialPadCommand", typeof(ICommand), typeof(CallInProgressControl), new PropertyMetadata(null));


        public ICommand KeyPressCommand
        {
            get { return (ICommand)GetValue(KeyPressCommandProperty); }
            set { SetValue(KeyPressCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyPressCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyPressCommandProperty =
            DependencyProperty.Register("KeyPressCommand", typeof(ICommand), typeof(CallInProgressControl), new PropertyMetadata(null));


        public AsyncCommand AnswerCommand
        {
            get { return (AsyncCommand)GetValue(AnswerCommandProperty); }
            set { SetValue(AnswerCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HangUpCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnswerCommandProperty =
            DependencyProperty.Register("AnswerCommand", typeof(AsyncCommand), typeof(CallInProgressControl), new PropertyMetadata(null));

        public AsyncCommand VideoAnswerCommand
        {
            get { return (AsyncCommand)GetValue(VideoAnswerCommandProperty); }
            set { SetValue(VideoAnswerCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HangUpCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VideoAnswerCommandProperty =
            DependencyProperty.Register("VideoAnswerCommand", typeof(AsyncCommand), typeof(CallInProgressControl), new PropertyMetadata(null));

        public ICommand MuteCommand
        {
            get { return (ICommand)GetValue(MuteCommandProperty); }
            set { SetValue(MuteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HangUpCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MuteCommandProperty =
            DependencyProperty.Register("MuteCommand", typeof(ICommand), typeof(CallInProgressControl), new PropertyMetadata(null));


        public ICommand UnmuteCommand
        {
            get { return (ICommand)GetValue(UnmuteCommandProperty); }
            set { SetValue(UnmuteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HangUpCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UnmuteCommandProperty =
            DependencyProperty.Register("UnmuteCommand", typeof(ICommand), typeof(CallInProgressControl), new PropertyMetadata(null));


    }
}
