using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Windows.System;
using Windows.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using PureWave.Utils;
using PureWave.Model;
#if WINDOWS_PHONE_APP
    using Windows.Media.Playback;
#endif

namespace PureWave.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private double _volume = 1;
        private bool _isHightQuality = true;

        #region Properties

        /// <summary>
        /// Отправить отзыв
        /// </summary>
        public CustomCommand SendFeedbackCommand { get; set; }

        /// <summary>
        /// Вкл/выкл звук
        /// </summary>
        public CustomCommand SoundCommand { get; set; }

        /// <summary>
        /// Сайт
        /// </summary>
        public CustomCommand SiteCommand { get; set; } = new CustomCommand(async (p) => await Launcher.LaunchUriAsync(new Uri("http://purewave.ru/")));

        /// <summary>
        /// Группа ВК
        /// </summary>
        public CustomCommand VKCommand { get; set; } = new CustomCommand(async (p) => await Launcher.LaunchUriAsync(new Uri("https://vk.com/pure_wave")));

        /// <summary>
        /// Ссылка на поток
        /// </summary>
        public string StreamUrl
        {
            get { return IsHightQuality ? "http://84.22.137.76:8000/myradio" : "http://84.22.137.76:8000/pure"; }
        }

        /// <summary>
        /// Высокое/низкое качество
        /// </summary>
        public bool IsHightQuality
        {
            get { return _isHightQuality; }
            set
            {
                _isHightQuality = value;
                OnPropertyChanged("StreamUrl");

#if WINDOWS_PHONE_APP
                // Устанавливаем источник вручную, т.к. забиндить нельзя
                BackgroundMediaPlayer.Current.SetUriSource(new Uri(StreamUrl));
#endif
            }
        }

        /// <summary>
        /// Исполнитель
        /// </summary>
        public string Artist { get; set; }

        /// <summary>
        /// Трек
        /// </summary>
        public string Track { get; set; }

        /// <summary>
        /// Фоновое изображение
        /// </summary>
        public string BackgroundSource { get; set; } = "ms-appx:///Images/defaultBackground.jpg";

        /// <summary>
        /// Громкость
        /// </summary>
        public double Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                Messenger.Default.Send(new VolumeMessage { Volume = value });
                OnPropertiesChanged("Volume", "SoundIcon");
            }
        }

        /// <summary>
        /// Иконка звука
        /// </summary>
        public string SoundIcon
        {
            get { return Volume > 0 ? "\uE15D" : "\uE198"; }
        }

        /// <summary>
        /// Иконка кнопки Играть/Пауза
        /// </summary>
        public string PlayPauseIcon { get; set; }

        /// <summary>
        /// Http-клиент
        /// </summary>
        public HttpClient HttpClient
        {
            get
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.IfModifiedSince = DateTime.Now;
                return client;
            }
        }

        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainViewModel()
        {
#if WINDOWS_PHONE_APP
            // Подписываемся на сообщения фонового потока
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground; 
#else
            StartTrackUpdater();
#endif

            IsHightQuality = true;
            SoundCommand = new CustomCommand(Sound);
            SendFeedbackCommand = new CustomCommand(SendFeedback);
            GetBackground();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Обработчик получения сообщения из фонового потока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            StartTrackUpdater();
        }
#endif

        /// <summary>
        /// Запустить фоновый поток для обновления информации о треке
        /// </summary>
        private void StartTrackUpdater()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    RefreshTrack();
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            });
        }

        /// <summary>
        /// Обновить иконку кнопки Играть/Пауза
        /// </summary>
        /// <param name="state"></param>
        public async void UpdatePlayPauseIcon(bool isPlaying)
        {
            PlayPauseIcon = isPlaying ? "/Images/pause.png" : "/Images/play.png";
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertyChanged("PlayPauseIcon"));
        }

        /// <summary>
        /// Получить ссылку на фоновое изображение
        /// </summary>
        private async void GetBackground()
        {
            try
            {
                var googleUrl = await HttpClient.GetStringAsync("https://drive.google.com/uc?export=download&id=0B1OTy-YRdX7uek9BOFpYd3pRV2s");
                var id = Regex.Match(googleUrl, "id=(.+)").Groups[1].Value;
                BackgroundSource = $"https://drive.google.com/uc?export=download&id={id}";
                OnPropertyChanged("BackgroundSource");
            }
            catch { }
        }

        /// <summary>
        /// Обновить трек
        /// </summary>
        /// <returns></returns>
        private async void RefreshTrack()
        {
            try
            {
                var trackInfo = await HttpClient.GetStringAsync("http://purewave.ru/assets/snippets/id3/id3.txt");
                var trackParams = trackInfo.Split(':');

                if (Artist == trackParams[0] && Track == trackParams[1])
                    return;

                Artist = trackParams[0];
                Track = trackParams[1];

#if WINDOWS_PHONE_APP
                // Отправляем фоновому потоку информацию о треке
                var vs = new ValueSet();
                vs.Add(new KeyValuePair<string,object>("Artist", Artist));
                vs.Add(new KeyValuePair<string, object>("Title", Track));
                BackgroundMediaPlayer.SendMessageToBackground(vs);   
#endif

                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertiesChanged("Artist", "Track"));
            }
            catch { }
        }

        /// <summary>
        /// Отправить отзыв
        /// </summary>
        /// <param name="parameter"></param>
        private async void SendFeedback(object parameter)
        {
            var mailto = new Uri($"mailto:?to=purewaveradio@gmail.com&subject={"Обратная связь"}");
            await Launcher.LaunchUriAsync(mailto);
        }

        /// <summary>
        /// Вкл/выкл звук
        /// </summary>
        /// <param name="parameter"></param>
        private void Sound(object parameter)
        {
            if (Volume > 0)
                Volume = 0;
            else Volume = 1;
        }
    }
}
