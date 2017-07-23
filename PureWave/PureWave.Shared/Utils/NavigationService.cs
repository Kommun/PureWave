using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace PureWave.Utils
{
    public class NavigationService
    {
        private readonly Frame frame;
        private bool _canGoBack;

        #region Fields

        /// <summary>
        /// Возможен ли переход назад
        /// </summary>
        public bool CanGoBack
        {
            get { return _canGoBack && frame.CanGoBack; }
            set
            {
                _canGoBack = value;
                if (!_canGoBack)
                    frame.BackStack.Clear();
            }
        }

        /// <summary>
        /// Тип текущей страницы
        /// </summary>
        public Type CurrentPageType
        {
            get { return frame.CurrentSourcePageType; }
        }

        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="frame">Главный фрэйм</param>
        public NavigationService(Frame frame)
        {
            this.frame = frame;
            frame.Navigated += Frame_Navigated;
        }

        /// <summary>
        /// Обработчик навигации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            CanGoBack = true;
        }

        /// <summary>
        /// Переход назад
        /// </summary>
        /// <returns>Выполнен ли переход</returns>
        public bool GoBack()
        {
            if (CanGoBack)
            {
                frame.GoBack();
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Переход вперед
        /// </summary>
        public void GoForward()
        {
            if (frame.CanGoForward)
                frame.GoForward();
        }

        /// <summary>
        /// Переход к странице
        /// </summary>
        /// <typeparam name="T">Тип страницы</typeparam>
        /// <param name="parameter">Параметр навигации</param>
        /// <returns></returns>
        public bool Navigate<T>(object parameter = null)
        {
            var type = typeof(T);

            return Navigate(type, parameter);
        }

        /// <summary>
        /// Переход к странице
        /// </summary>
        /// <param name="source">Тип страницы</param>
        /// <param name="parameter">Параметр навигации</param>
        /// <returns></returns>
        public bool Navigate(Type source, object parameter = null)
        {
            return frame.Navigate(source, parameter);
        }

        /// <summary>
        /// Очистить историю переходов за исключением первой (главной) страницы
        /// </summary>
        public void ClearBackStackButFirst()
        {
            if (frame.BackStackDepth <= 1)
                return;

            var last = frame.BackStack.First();
            frame.BackStack.Clear();
            frame.BackStack.Add(last);
        }

        /// <summary>
        /// Удалить последнюю страницу из истории переходов
        /// </summary>
        public void DeleteLastFromBackStack()
        {
            if (frame.BackStackDepth <= 1)
                return;

            frame.BackStack.Remove(frame.BackStack.Last());
        }

        /// <summary>
        /// Установить видимость нижней панели
        /// </summary>
        /// <param name="IsVisible">Видимость</param>
        public void SetAppBarVisibility(bool IsVisible)
        {
            try
            {
                var appBar = (frame.Content as Page).BottomAppBar;
                if (appBar != null)
                    appBar.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            catch { }
        }
    }
}
