using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http;
using Windows.System;
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
        private Bitrate _selectedBitrate;
        private string _previousTrackInfo;

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
        /// Вайбер
        /// </summary>
        public CustomCommand ViberCommand { get; set; } = new CustomCommand(async p => await Launcher.LaunchUriAsync(new Uri("viber://add?number=+79607701030")));

        /// <summary>
        /// Пожертвовать
        /// </summary>
        public CustomCommand DonateCommand { get; set; }

        /// <summary>
        /// Доступные битрейты
        /// </summary>
        public List<Bitrate> Bitrates { get; set; } = new List<Bitrate>
        {
            new Bitrate { Name = "64 бит", Url = "http://stream.purewave.ru:8000/stream64.mp3" },
            new Bitrate { Name = "128 бит", Url = "http://stream.purewave.ru:8000/stream128.mp3" },
            new Bitrate { Name = "256 бит", Url =" http://stream.purewave.ru:8000/stream256.mp3" }
        };

        /// <summary>
        /// Ссылка на поток
        /// </summary>
        public string StreamUrl
        {
            get { return _selectedBitrate.Url; }
        }

        /// <summary>
        /// Выбранный битрейт
        /// </summary>
        public Bitrate SelectedBitrate
        {
            get { return _selectedBitrate; }
            set
            {
                _selectedBitrate = value;
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
        public string BackgroundSource { get; set; } = "https://firebasestorage.googleapis.com/v0/b/purewave-ee557.appspot.com/o/MusicImage%2FCurrentImage.png?alt=media&token=067b5235-1ef1-44d6-91d4-545645dbab65";
        //public string BackgroundSource { get; set; } = "ms-appx:///Images/defaultBackground.jpg";

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

            SelectedBitrate = Bitrates.ElementAtOrDefault(1);
            SoundCommand = new CustomCommand(Sound);
            SendFeedbackCommand = new CustomCommand(SendFeedback);
            DonateCommand = new CustomCommand(Donate);
            //GetBackground();
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
                if (string.IsNullOrEmpty(trackInfo) || trackInfo == _previousTrackInfo)
                    return;

                _previousTrackInfo = trackInfo;

                var trackParams = trackInfo.Split(new string[] { ":", @"\\-" }, StringSplitOptions.RemoveEmptyEntries);
                if (trackParams.Length == 0)
                    return;

                Artist = trackParams[0];
                if (trackParams.Length > 1)
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

        private async void Donate(object parameter)
        {
            await PopupManager.ShowDonateDialog();
        }
    }
}
