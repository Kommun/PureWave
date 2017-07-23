using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace PureWave.Controls
{
    public sealed class NotificationMessage : ContentControl
    {
        public NotificationMessage()
        {
            this.DefaultStyleKey = typeof(NotificationMessage);
            Height = Window.Current.Bounds.Height;
            Width = Window.Current.Bounds.Width;
        }

        protected override void OnApplyTemplate()
        {
            var btnClose = GetTemplateChild("btnClose") as CustomButton;
            btnClose.Click += btnClose_Click;
        }

        /// <summary>
        /// Закрыть подсказку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                (this.Parent as Popup).IsOpen = false;
            }
            catch { }
        }
    }
}
