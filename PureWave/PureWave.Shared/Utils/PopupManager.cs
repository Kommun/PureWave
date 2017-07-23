using Windows.UI;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using PureWave.Controls;

namespace PureWave.Utils
{
    public static class PopupManager
    {
        /// <summary>
        /// Текущее уведомление
        /// </summary>
        public static Popup CurrentPopup { get; set; }

        /// <summary>
        /// Создает уведомление
        /// </summary>
        /// <returns></returns>
        public static void ShowNotificationPopup(string message)
        {
            var popup = new Popup();
            popup.ChildTransitions = new TransitionCollection { new ContentThemeTransition() };
            popup.Child = new NotificationMessage { Content = message };
            popup.Closed += (a, e) =>
            {
                App.NavigationService.SetAppBarVisibility(true);
                CurrentPopup = null;
            };
            App.NavigationService.SetAppBarVisibility(false);
            CurrentPopup = popup;
            popup.IsOpen = true;
        }

        /// <summary>
        /// Показать информацию о пожертвованиях
        /// </summary>
        /// <returns></returns>
        public async static Task ShowDonateDialog()
        {
            var dialog = new DonateDialog();
            await ShowDialog(dialog);
        }

        /// <summary>
        /// Показать диалог
        /// </summary>
        /// <param name="dialog"></param>
        /// <returns></returns>
        public async static Task ShowDialog(Control dialog)
        {
            bool closed = false; ;
            var popup = new Popup();
            popup.ChildTransitions = new TransitionCollection { new ContentThemeTransition() };

            popup.Child = dialog;
            popup.Closed += (a, e) =>
            {
                App.NavigationService.SetAppBarVisibility(true);
                CurrentPopup = null;
                closed = true;
            };
            App.NavigationService.SetAppBarVisibility(false);
            CurrentPopup = popup;
            popup.IsOpen = true;
            await Task.Factory.StartNew(() => { while (!closed) { } });
        }

        /// <summary>
        /// Закрыть текущее уведомление
        /// </summary>
        public static void CloseCurrentPopup()
        {
            if (CurrentPopup != null)
            {
                CurrentPopup.IsOpen = false;
                CurrentPopup = null;
            }
        }
    }
}
