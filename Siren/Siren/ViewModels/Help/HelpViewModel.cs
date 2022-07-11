using Siren.Views.Help;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Web;
using Xamarin.Forms;

namespace Siren.ViewModels.Help
{
    public class HelpViewModel : BaseViewModel, IQueryAttributable
    {
        public Command GoBackCommand { get => new Command(async () => await Shell.Current.GoToAsync("..")); }

        private string _title;
        public string MessageTitle
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private bool _isRUS = false;
        public bool IsRUS
        {
            get => _isRUS;
            set
            {
                SetProperty(ref _isRUS, value);
                Refresh();
            }
        }

        private string _buttonText;
        public string ButtonText
        {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        private EHelpTopic Topic { get; set; }

        public void ApplyQueryAttributes(IDictionary<string, string> query)
        {
            string intentString = HttpUtility.UrlDecode(query["topic"]);
            Enum.TryParse(intentString, out EHelpTopic topic);
            Topic = topic;

            Refresh();
        }

        private void Refresh()
        {
            MessageTitle = IsRUS ? Messages[Topic].MessageTitle.RUS : Messages[Topic].MessageTitle.ENG;
            Message = IsRUS ? Messages[Topic].Message.RUS : Messages[Topic].Message.ENG;
            ButtonText = IsRUS ? "Хорошо, понятно!" : "Ok, understand!";
        }

        public Dictionary<EHelpTopic, HelpMassage> Messages = new Dictionary<EHelpTopic, HelpMassage>
        {
            {
                EHelpTopic.Setting,
                new HelpMassage()
                {
                    MessageTitle = new LocalizedString { ENG = "What is Siren Setting?", RUS = "Что такое Setting?" },
                    Message = new LocalizedString
                    { 
                        ENG = "A setting is a group of «Scenes», «Elements», «Effects» and a playlist with «Music». The setting is indicated by a title and an illustration. They can represent locations. Examples of such settings: «Pirate Ship», «Tavern», «Elven wood», «Desert», «Dungeon», «Wizard's Tower». In addition to locations, settings can combine categories of scenes or simply moods. Examples of such settings: «Chase», «Battle scenes», «Heist», «Tense negotiations», «Comedy scenes», «Dramatic moments». You decide yourself how to group and combine audio and scenes into settings. To view an example with Settings, it is recommended to install the «Siren basic bundle.siren» bundle into the application.",
                        RUS = "Сеттинг — это группа из «Сцен», «Элементов», «Эффектов» и плейлиста с «Музыкой». Сеттинг обозначается заголовком и иллюстрацией. Они могут обозначать локации. Примеры таких сеттингов: «Пиратский корабль», «Таверна», «Эльфийский лес», «Пустыня», «Подземелье», «Башня волшебника». Кроме локаций сеттинги могут объединять категории сцен или просто настроение. Примеры таких сеттингов: «Погоня», «Боевые сцены», «Ограбление», «Напряжённые переговоры», «Комедийные сцены», «Драматические моменты». Вы сами решаете как группировать и комбинировать аудио и сцены в сеттинги. Для просмотра примера с сеттингами рекомендуется установить в приложение бандл «Siren basic bundle.siren»."
                    }
                }
            },
            {
                EHelpTopic.Scene,
                new HelpMassage()
                {
                    MessageTitle = new LocalizedString { ENG = "What is Siren Scene?", RUS = "Что такое Scene?" },
                    Message = new LocalizedString
                    {
                        ENG = "A Scene is a preset of Elements and Music located in a certain Setting. For example, if the setting is «Tavern», then it is expected to see the scenes «Noisy merry evening», «Brawl», «Quiet morning», «Goblin attack», «Concert» in it. Scenes are designed to play certain audio elements at a certain volume at the same time. To save the adjusted audio, you must click the save button located on the scene card in the scenes panel. To select a scene and play it, select it by clicking on any free space on the scene card. Scene transitions are seamless, with volume fading in and out smoothly.",
                        RUS = "Сцена — это преднастройка Элементов и Музыки, расположенная в определённом Сеттинге. Например, если сеттингом является «Таверна», то ожидаемо увидеть в ней сцены «Шумный весёлый вечер», «Потасовка», «Тихое утро», «Нападение гоблинов», «Концерт». Сцены созданы для того, чтобы включать определённые аудио-элементы на определённой громкости одновременно. Для сохранения настроенного аудио необходимо нажать кнопку сохранения, расположенную на карточке сцены на панели сцен. Для запуска сцены необходимо выбрать её, кликнув в любое свободное место на карточке сцены. Переходы между сценами происходят бесшовно, с плавным нарастанием и затуханием громкости."
                    }
                }
            },
            {
                EHelpTopic.Element,
                new HelpMassage()
                {
                    MessageTitle = new LocalizedString { ENG = "What is Siren Element?", RUS = "Что такое Element?" },
                    Message = new LocalizedString
                    {
                        RUS = "Элементом называется аудио, которое будет проигрываться приложением в цикле, поэтому для элементов стоит подбирать бесшовные аудио-треки, в которых не будет сильно заметным запуск аудио сначала, после того, как оно завершилось. Подразумевается, что в качестве элементов будут использоваться звуки окружения и ambient-музыка. Элементами могут быть звуки дождя, развевающихся парусов, булькание лабораторного оборудования, церковное пение, шум листвы, гомон толпы и т.п. Несколько элементов можно запускать одноверменно, создавая таким образом насыщенную сцену, которая позволит погрузится в атмосферу игры лучше.",
                        ENG = "An «Element» — is audio that will be played by the application in a loop, so for elements you should choose seamless audio tracks that do not have a noticeable transition from the end to the beginning. It is assumed that environmental sounds and ambient music will be used as elements. Elements can be the sounds of rain, fluttering sails, the gurgling of laboratory equipment, temple choir, the noise of leaves, the hubbub of the crowd, etc. Several elements can be triggered at the same time, thus creating a rich scene that will allow you to immerse in the atmosphere of the game better. To add audio files as Elements, click the plus button above the Elements panel."
                    }
                }
            },
            {
                EHelpTopic.Effect,
                new HelpMassage()
                {
                    MessageTitle = new LocalizedString { ENG = "What is Siren Effect?", RUS = "Что такое Effect?" },
                    Message = new LocalizedString
                    {
                        RUS = "Эффект — это аудио-файл Сеттинга, который не участвует в сценах и проигрывается только вручную. Подразумевается, что Эффекты будут использоваться для разовых звуковых эффектов. Такими эффактами может быть звук разбитого стекла, выстрел пушки, рык дракона, звук сработавшего заклинания и т.п.",
                        ENG = "An effect is an audio file of the Setting that does not participate in scenes and is only played manually. It is understood that Effects will be used for one-time sound effects. Such effects can be the sound of breaking glass, the sound of a cannon, the roar of a dragon, the sound of a spell, and so on. To add audio files as Effects, click the plus button above the Effects panel."
                    }
                }
            },
            {
                EHelpTopic.Music,
                new HelpMassage()
                {
                    MessageTitle = new LocalizedString { ENG = "How Siren music works?", RUS = "Как в Siren работает музыка?" },
                    Message = new LocalizedString
                    {
                        ENG = "When creating a Setting, you can add music to its playlist. The scene stores data on whether to include music in it, whether to shuffle tracks during playback, and what volume value should be set. Music, like any other audio component, can also be run separately from Scenes. In the music player of the Setting, only one track can be played at a time.",
                        RUS = "Создавая Сеттинг, вы можете добавить в его плейлист музыку. Сцена хранит данные о том, нужно ли включать музыку в ней, перемешивать ли треки при проигрываниии и какую громкость следует установить. Музыку, как и любой другой аудио-компонент, можно запускать и отдельно от сцен. В музыкальном плеере Сеттинга одновременно может проигрываться только один трек."
                    }
                }
            },
        };
    }

    public class HelpMassage
    {
        public LocalizedString MessageTitle { get; set; }
        public LocalizedString Message { get; set; }
    }

    public class LocalizedString
    {
        public string RUS { get; set; }
        public string ENG { get; set; }
    }

    public enum EHelpTopic
    {
        Setting = 1,
        Scene = 2,
        Element = 3,
        Effect = 4,
        Music = 5
    }
}
