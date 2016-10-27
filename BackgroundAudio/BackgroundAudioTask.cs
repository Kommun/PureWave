using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace BackgroundAudio
{
    public sealed class BackgroundAudioTask : IBackgroundTask
    {
        private SystemMediaTransportControls systemmediatransportcontrol;

        /// <summary>
        /// The Run method is the entry point of a background task. 
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();
            systemmediatransportcontrol = SystemMediaTransportControls.GetForCurrentView();
            systemmediatransportcontrol.ButtonPressed += Systemmediatransportcontrol_ButtonPressed;
            systemmediatransportcontrol.IsEnabled = true;
            systemmediatransportcontrol.IsPauseEnabled = true;
            systemmediatransportcontrol.IsPlayEnabled = true;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            try
            {
                var valueset = e.Data;
                systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
                systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
                systemmediatransportcontrol.DisplayUpdater.MusicProperties.Artist = valueset["Artist"].ToString();
                systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = valueset["Title"].ToString();
                systemmediatransportcontrol.DisplayUpdater.Update();
            }
            catch { }
        }

        private void Systemmediatransportcontrol_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    BackgroundMediaPlayer.Current.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    BackgroundMediaPlayer.Current.Pause();
                    break;
            }
        }
    }
}
