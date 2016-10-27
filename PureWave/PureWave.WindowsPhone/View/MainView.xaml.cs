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
using Windows.Media.Playback;
using PureWave.ViewModel;
using PureWave.Model;
using PureWave.Utils;

namespace PureWave.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page
    {
        private MainViewModel _vm = new MainViewModel();

        public MainView()
        {
            this.InitializeComponent();
            DrawerLayout.InitializeDrawerLayout();
            DataContext = _vm;

            Messenger.Default.Register<VolumeMessage>(this, (vm) => BackgroundMediaPlayer.Current.Volume = vm.Volume);
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.Current.SetUriSource(new Uri(_vm.StreamUrl));
            BackgroundMediaPlayer.Current.Play();
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            _vm.UpdatePlayPauseIcon(BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing);
        }

        /// <summary>
        /// Обработчик нажатия на кнопку Play/Pause
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
                    BackgroundMediaPlayer.Current.Pause();
                else
                    BackgroundMediaPlayer.Current.Play();
            }
            catch { }
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
