using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using PureWave.Utils;

namespace PureWave.ViewModel
{
    public class MainViewModel : PropertyChangedBase
    {
        private double _volume = 1;

        #region Properties

        /// <summary>
        /// Показать/скрыть меню
        /// </summary>
        public CustomCommand ShowHideMenuCommand { get; set; }

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

        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainViewModel()
        {
            SoundCommand = new CustomCommand(Sound);

            GetBackground();

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
        /// Получить ссылку на фоновое изображение
        /// </summary>
        private async void GetBackground()
        {
            var googleUrl = await (new System.Net.Http.HttpClient().GetStringAsync("https://drive.google.com/uc?export=download&id=0B1OTy-YRdX7uek9BOFpYd3pRV2s"));
            var id = Regex.Match(googleUrl, "id=(.+)").Groups[1].Value;
            BackgroundSource = $"https://drive.google.com/uc?export=download&id={id}";
            OnPropertyChanged("BackgroundSource");
        }

        /// <summary>
        /// Обновить трек
        /// </summary>
        /// <returns></returns>
        private async void RefreshTrack()
        {
            try
            {
                var trackInfo = await new System.Net.Http.HttpClient().GetStringAsync("http://purewave.ru/assets/snippets/id3/id3.txt");
                var trackParams = trackInfo.Split(':');

                Artist = trackParams[0];
                Track = trackParams[1];
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => OnPropertiesChanged("Artist", "Track"));
            }
            catch { }
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
