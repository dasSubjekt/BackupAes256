namespace BackupAes256.ViewModel
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;


    public static class FocusExtension
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(FocusExtension), new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));

        private static void OnIsFocusedPropertyChanged(DependencyObject oDependencyObject, DependencyPropertyChangedEventArgs oEventArgs)
        {
            UIElement oUIElement = (UIElement)oDependencyObject;

            if ((bool)oEventArgs.NewValue && (oUIElement.Dispatcher != null))
            {
                oUIElement.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() => oUIElement.Focus()));
                Keyboard.Focus(oUIElement);
            }
        }
    }
}