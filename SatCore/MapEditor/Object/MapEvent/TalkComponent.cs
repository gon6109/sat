using BaseComponent;
using SatCore.Attribute;
using SatCore.MapEditor.Object.MapEvent;
using SatIO.MapEventIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor.Object.MapEvent
{
    /// <summary>
    /// テキスト表示系
    /// </summary>
    public class TalkComponent : MapEventComponent
    {
        public new string Name => "Talk : " + TalkElements.Count;

        [ListInput("要素")]
        public UndoRedoCollection<BaseTalkElement> TalkElements { get; set; }

        public ObservableCollection<CharacterImage> CharacterImages { get; set; }

        public TalkComponent(ObservableCollection<CharacterImage> characterImages)
        {
            CharacterImages = characterImages;
            TalkElements = new UndoRedoCollection<BaseTalkElement>();
            TalkElements.CollectionChanged += TalkElements_CollectionChanged;
        }

        private void TalkElements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("Name");
        }

        [Button("キャラを表示")]
        public void AddShowElement()
        {
            if (CharacterImages.Count == 0) return;
            TalkElements.Add(new ShowCharacterElement(CharacterImages));
        }

        [Button("テキストを表示")]
        public void AddTalkElement()
        {
            if (CharacterImages.Count == 0) return;
            TalkElements.Add(new TalkElement(CharacterImages));
        }

        [Button("キャラ差分を変更")]
        public void AddChangeDiffElement()
        {
            if (CharacterImages.Count == 0) return;
            TalkElements.Add(new ChangeDiffElement(CharacterImages));
        }

        [Button("キャラを隠す")]
        public void AddHideElement()
        {
            if (CharacterImages.Count == 0) return;
            TalkElements.Add(new HideCharacterElement(CharacterImages));
        }

        public static TalkComponent LoadTalkComponent(TalkComponentIO talkComponentIO, ObservableCollection<CharacterImage> characterImages)
        {
            var component = new TalkComponent(characterImages);
            foreach (TalkComponentIO.BaseTalkElementIO item in talkComponentIO.TalkElements)
            {
                try
                {
                    if (item is TalkComponentIO.ShowCharacterElementIO)
                    {
                        component.TalkElements.Add(
                            new ShowCharacterElement(component.CharacterImages)
                            {
                                Index = ((TalkComponentIO.ShowCharacterElementIO)item).Index,
                                CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                            });
                    }
                    if (item is TalkComponentIO.TalkElementIO)
                    {
                        component.TalkElements.Add(
                            new TalkElement(component.CharacterImages)
                            {
                                Text = ((TalkComponentIO.TalkElementIO)item).Text.Replace("\n", "\r\n"),
                                CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                            });
                    }
                    if (item is TalkComponentIO.ChangeDiffElementIO)
                    {
                        component.TalkElements.Add(
                            new ChangeDiffElement(component.CharacterImages)
                            {
                                CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                                DiffImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName).DiffImages.First
                                (obj => obj.Name == ((TalkComponentIO.ChangeDiffElementIO)item).DiffImage),
                            });
                    }
                    if (item is TalkComponentIO.HideCharacterElementIO)
                    {
                        component.TalkElements.Add(
                            new HideCharacterElement(component.CharacterImages)
                            {
                                CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                            });
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            return component;
        }

        public static explicit operator TalkComponentIO(TalkComponent talkeComponent)
        {
            TalkComponentIO talkComponentIO = new TalkComponentIO()
            {
                TalkElements = talkeComponent.TalkElements.Select<BaseTalkElement, TalkComponentIO.BaseTalkElementIO>(obj =>
                     {
                         ShowCharacterElement showCharacterElement = obj as ShowCharacterElement;
                         if (showCharacterElement != null)
                             return new TalkComponentIO.ShowCharacterElementIO()
                             {
                                 CharacterName = showCharacterElement.CharacterImage.Name,
                                 Index = showCharacterElement.Index
                             };

                         TalkElement talkElement = obj as TalkElement;
                         if (talkElement != null)
                             return new TalkComponentIO.TalkElementIO()
                             {
                                 CharacterName = talkElement.CharacterImage.Name,
                                 Text = talkElement.Text.Replace("\r\n", "\n")
                             };

                         ChangeDiffElement changeDiffElement = obj as ChangeDiffElement;
                         if (changeDiffElement != null)
                             return new TalkComponentIO.ChangeDiffElementIO()
                             {
                                 CharacterName = changeDiffElement.CharacterImage.Name,
                                 DiffImage = changeDiffElement.DiffImage.Name
                             };

                         HideCharacterElement hideCharacterElement = obj as HideCharacterElement;
                         if (hideCharacterElement != null)
                             return new TalkComponentIO.HideCharacterElementIO()
                             {
                                 CharacterName = hideCharacterElement.CharacterImage.Name,
                             };
                         return null;
                     }).ToList()
            };
            return talkComponentIO;
        }

        public abstract class BaseTalkElement : IListInput, INotifyPropertyChanged
        {
            private CharacterImage _characterImage;

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string Name => "Talk Element";
            public CharacterImage CharacterImage
            {
                get => _characterImage;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _characterImage = value;
                    OnPropertyChanged("Name");
                    OnPropertyChanged();
                }
            }
            public ObservableCollection<CharacterImage> CharacterImages { get; set; }

            ObservableCollection<CharacterImage> refCharacterImages;

            public BaseTalkElement(ObservableCollection<CharacterImage> characterImages)
            {
                CharacterImages = new ObservableCollection<CharacterImage>(characterImages);
                characterImages.CollectionChanged += CharacterImages_CollectionChanged;
                refCharacterImages = characterImages;
            }

            ~BaseTalkElement()
            {
                refCharacterImages.CollectionChanged -= CharacterImages_CollectionChanged;
            }

            private void CharacterImages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                switch (e.Action)
                {
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                        CharacterImages.Add((CharacterImage)e.NewItems[0]);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                        CharacterImages.Remove((CharacterImage)e.OldItems[0]);
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                        break;
                    case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                        break;
                    default:
                        break;
                }
            }
        }

        public class ShowCharacterElement : BaseTalkElement
        {
            private int _index;

            public new string Name => "Show " + Index.ToString() + ":" + (CharacterImage != null ? CharacterImage.Name : "");

            [NumberInput("配置場所")]
            public int Index
            {
                get => _index;
                set
                {
                    if (value > -1 && value < 4)
                    {
                        UndoRedoManager.ChangeProperty(this, value);
                        _index = value;
                        OnPropertyChanged();
                        OnPropertyChanged("Name");
                    }
                }
            }

            [ListInput("キャラ一覧", "CharacterImage", isVisibleRemoveButtton: false)]
            public new ObservableCollection<CharacterImage> CharacterImages { get => base.CharacterImages; set => base.CharacterImages = value; }

            public ShowCharacterElement(ObservableCollection<CharacterImage> characterImages) : base(characterImages)
            {
            }
        }

        public class TalkElement : BaseTalkElement
        {
            private string _text;

            public new string Name => (CharacterImage != null ? CharacterImage.Name : "") + " Speach \""
                + (Text != null ? Text.Length > 8 ? Text.Substring(0, 8) : Text : "") + "\"";

            [TextAreaInput("表示テキスト")]
            public string Text
            {
                get => _text;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _text = value.Replace("\r", "");
                    OnPropertyChanged();
                    OnPropertyChanged("Name");
                }
            }

            [ListInput("キャラ一覧", "CharacterImage", isVisibleRemoveButtton: false)]
            public new ObservableCollection<CharacterImage> CharacterImages { get => base.CharacterImages; set => base.CharacterImages = value; }

            public TalkElement(ObservableCollection<CharacterImage> characterImages) : base(characterImages)
            {
            }
        }

        public class ChangeDiffElement : BaseTalkElement
        {
            private CharacterImage.DiffImage _diffImage;

            public new string Name => "Change " + (CharacterImage != null ? CharacterImage.Name : "") + "'s diff image to " + DiffImage?.Name;

            public CharacterImage.DiffImage DiffImage
            {
                get => _diffImage;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _diffImage = value;
                    OnPropertyChanged();
                    OnPropertyChanged("Name");
                }
            }

            public new CharacterImage CharacterImage
            {
                get => base.CharacterImage;
                set
                {
                    UndoRedoManager.Enable = false;
                    if (value == null) DiffImages.Clear();
                    else if (base.CharacterImage != value)
                    {
                        DiffImages.Clear();
                        foreach (var item in value.DiffImages)
                        {
                            DiffImages.Add(item);
                        }
                    }
                    DiffImage = DiffImages.Count > 0 ? DiffImages[0] : null;
                    UndoRedoManager.Enable = true;
                    base.CharacterImage = value;
                }
            }

            [ListInput("差分一覧", "DiffImage", isVisibleRemoveButtton: false)]
            public ObservableCollection<CharacterImage.DiffImage> DiffImages { get; private set; }

            [ListInput("キャラ一覧", "CharacterImage", isVisibleRemoveButtton: false)]
            public new ObservableCollection<CharacterImage> CharacterImages { get => base.CharacterImages; set => base.CharacterImages = value; }

            public ChangeDiffElement(ObservableCollection<CharacterImage> characterImages) : base(characterImages)
            {
                DiffImages = new ObservableCollection<CharacterImage.DiffImage>();
            }
        }

        public class HideCharacterElement : BaseTalkElement
        {
            public new string Name => "Hide " + (CharacterImage != null ? CharacterImage.Name : "");

            [ListInput("キャラ一覧", "CharacterImage", isVisibleRemoveButtton: false)]
            public new ObservableCollection<CharacterImage> CharacterImages { get => base.CharacterImages; set => base.CharacterImages = value; }

            public HideCharacterElement(ObservableCollection<CharacterImage> characterImages) : base(characterImages)
            {
            }
        }
    }
}
