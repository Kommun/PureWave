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
        private BackgroundTaskDeferral deferral; // Used to keep task alive

        /// <summary>
        /// The Run method is the entry point of a background task. 
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += TaskInstance_Canceled;
            deferral = taskInstance.GetDeferral();
            systemmediatransportcontrol = SystemMediaTransportControls.GetForCurrentView();
            systemmediatransportcontrol.ButtonPressed += Systemmediatransportcontrol_ButtonPressed;
            systemmediatransportcontrol.IsEnabled = true;
            systemmediatransportcontrol.IsPauseEnabled = true;
            systemmediatransportcontrol.IsPlayEnabled = true;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
            BackgroundMediaPlayer.SendMessageToForeground(new ValueSet());
        }

        /// <summary>
        /// Handles background task cancellation. Task cancellation happens due to :
        /// 1. Another Media app comes into foreground and starts playing music 
        /// 2. Resource pressure. Your task is consuming more CPU and memory than allowed.
        /// In either case, save state so that if foreground app resumes it can know where to start.
        /// </summary>
        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                systemmediatransportcontrol.ButtonPressed -= Systemmediatransportcontrol_ButtonPressed;
                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch { }

            deferral.Complete(); // signals task completion. 
        }

        /// <summary>
        /// Обработчик сообщений из основного потока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Обработчик нажатия на системные кнопки управления аудиозаписями
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
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
