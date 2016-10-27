using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media;
using PureWave.ViewModel;

namespace PureWave.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        private MainViewModel _vm = new MainViewModel();
        private SystemMediaTransportControls _systemControls;

        public MainView()
        {
            this.InitializeComponent();
            DrawerLayout.InitializeDrawerLayout();
            DataContext = _vm;
            InitializeTransportControls();
        }

        /// <summary>
        /// Инициализировать кнопки управления проигрывателем
        /// </summary>
        private void InitializeTransportControls()
        {
            // Hook up app to system transport controls.
            _systemControls = SystemMediaTransportControls.GetForCurrentView();
            _systemControls.ButtonPressed += SystemControls_ButtonPressed;

            // Register to handle the following system transpot control buttons.
            _systemControls.IsPlayEnabled = true;
            _systemControls.IsPauseEnabled = true;
        }

        private void SystemControls_ButtonPressed(SystemMediaTransportControls sender,
                SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    backgroundMusic.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    backgroundMusic.Pause();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку Play/Pause
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (backgroundMusic.CurrentState == MediaElementState.Playing)
                backgroundMusic.Pause();
            else
                backgroundMusic.Play();
        }

        /// <summary>
        /// Обработчик изменения состояния проигрывателя
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundMusic_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            _vm.UpdatePlayPauseIcon(backgroundMusic.CurrentState);
            switch (backgroundMusic.CurrentState)
            {
                case MediaElementState.Playing:
                    _systemControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    break;
                case MediaElementState.Paused:
                    _systemControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    break;
                case MediaElementState.Stopped:
                    _systemControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    break;
                case MediaElementState.Closed:
                    _systemControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки открытия меню
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowMenu_Click(object sender, RoutedEventArgs e)
        {
            if (DrawerLayout.IsDrawerOpen)
                DrawerLayout.CloseDrawer();
            else
                DrawerLayout.OpenDrawer();
        }
    }
}
