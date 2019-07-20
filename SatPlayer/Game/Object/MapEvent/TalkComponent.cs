using BaseComponent;
using SatIO.MapEventIO;
using SatPlayer.Game.Object.MapEvent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object.MapEvent
{
    /// <summary>
    /// テキスト表示コンポーネント
    /// </summary>
    public partial class TalkComponent : MapEventComponent
    {
        /// <summary>
        /// テキストの要素
        /// </summary>
        public List<BaseTalkElement> TalkElements { get; set; }

        /// <summary>
        /// キャラクターグラフィック
        /// </summary>
        public List<CharacterImage> CharacterImages { get; set; }

        /// <summary>
        /// ポジションごとのキャラクターグラフィック
        /// </summary>
        public Dictionary<int, CharacterImage> Index { get; set; }

        public TalkComponent(List<CharacterImage> characterImages)
        {
            CharacterImages = characterImages;
            TalkElements = new List<BaseTalkElement>();
            Index = new Dictionary<int, CharacterImage>();
        }

        public static TalkComponent LoadTalkComponent(TalkComponentIO talkComponentIO, List<CharacterImage> characterImages)
        {
            var component = new TalkComponent(characterImages);
            foreach (TalkComponentIO.BaseTalkElementIO item in talkComponentIO.TalkElements)
            {
                if (item is TalkComponentIO.ShowCharacterElementIO)
                {
                    component.TalkElements.Add(
                        new ShowCharacterElement()
                        {
                            Index = ((TalkComponentIO.ShowCharacterElementIO)item).Index,
                            CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                        });
                }
                if (item is TalkComponentIO.TalkElementIO)
                {
                    component.TalkElements.Add(
                        new TalkElement()
                        {
                            Text = ((TalkComponentIO.TalkElementIO)item).Text,
                            CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                        });
                }
                if (item is TalkComponentIO.ChangeDiffElementIO)
                {
                    component.TalkElements.Add(
                        new ChangeDiffElement()
                        {
                            DiffImage = ((TalkComponentIO.ChangeDiffElementIO)item).DiffImage,
                            CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                        });
                }
                if (item is TalkComponentIO.HideCharacterElementIO)
                {
                    component.TalkElements.Add(
                        new HideCharacterElement()
                        {
                            CharacterImage = component.CharacterImages.First(obj => obj.Name == item.CharacterName),
                        });
                }
            }
            return component;
        }

        public override IEnumerator Update()
        {
            var layer = new ScalingLayer2D();
            layer.IsFixAspectRatio = true;
            layer.IsUpdateScalingAuto = true;
            asd.Engine.CurrentScene.AddLayer(layer);
            layer.DrawingPriority = 3;

            var messageBox = new MessageBox();
            layer.AddObject(messageBox);
            foreach (var item in CharacterImages)
            {
                item.Position = new asd.Vector2DF(-800, 0);
                layer.AddObject(item);
            }
            yield return 0;

            var messageIterator = messageBox.Open();
            while (messageIterator.MoveNext())
            {
                CharacterImages.ForEach(obj => obj.Color = new asd.Color(255, 255, 255, (int)messageBox.Color.A));
                yield return 0;
            }
            foreach (var item in TalkElements)
            {
                var updateIterator = item.Update(this, messageBox);
                while (updateIterator.MoveNext())
                {
                    yield return 0;
                }
            }
            messageIterator = messageBox.Close();
            while (messageIterator.MoveNext())
            {
                CharacterImages.ForEach(obj => obj.Color = new asd.Color(255, 255, 255, (int)messageBox.Color.A));
                yield return 0;
            }

            layer.Dispose();
            yield return 0;
        }

        public abstract class BaseTalkElement
        {
            public CharacterImage CharacterImage { get; set; }

            public BaseTalkElement()
            {

            }

            public virtual IEnumerator Update(TalkComponent component, MessageBox messageBox)
            {
                yield return 0;
            }
        }

        /// <summary>
        /// キャラクターを表示させる
        /// </summary>
        public class ShowCharacterElement : BaseTalkElement
        {
            public int Index { get; set; }

            public ShowCharacterElement()
            {

            }

            public override IEnumerator Update(TalkComponent component, MessageBox messageBox)
            {
                CharacterImage.Position = new asd.Vector2DF(Index < 2 ? -200 : ScalingLayer2D.OriginDisplaySize.X + 200, Index == 1 || Index == 2 ? 100 : 50);
                messageBox.Index = Index;
                messageBox.Name = CharacterImage.Name;
                var animation = new Animation();
                animation.Move(new asd.Vector2DF(Index < 2 ? -500 : ScalingLayer2D.OriginDisplaySize.X + 500, Index == 1 || Index == 2 ? 100 : 50), 
                    new asd.Vector2DF(GetXByIndex(), Index == 1 || Index == 2 ? 100 : 50), 30, Animation.Easing.OutSine);
                CharacterImage.Animation.AddAnimation(CharacterImage, animation);
                while (CharacterImage.Animation.IsAnimating)
                {
                    yield return 0;
                }
                component.Index[Index] = CharacterImage;
                yield return 0;
            }

            float GetXByIndex()
            {
                if (Index == 0) return 200;
                else if (Index == 1) return 560;
                else if (Index == 2) return 960;
                else return 1320;
            }
        }

        /// <summary>
        /// テキストを表示させる
        /// </summary>
        public class TalkElement : BaseTalkElement
        {
            public string Text { get; set; }

            public TalkElement()
            {
            }

            public override IEnumerator Update(TalkComponent component, MessageBox messageBox)
            {
                messageBox.SetMessage(Text);
                if (component.Index.Any(obj => obj.Value == CharacterImage))
                {
                    messageBox.Name = CharacterImage.Name;
                    messageBox.Index = component.Index.First(obj => obj.Value == CharacterImage).Key;
                }
                else
                {
                    messageBox.Name = new string('?', CharacterImage.Name.Length);
                    messageBox.Index = 3;
                }
                var iterator = messageBox.ShowText();
                while (iterator.MoveNext())
                {
                    if (messageBox.NameOutput.Color.A < 255)
                    {
                        var temp = messageBox.NameOutput.Color;
                        int v = temp.A > 235 ? 255 : temp.A + 20;
                        temp.A = (byte)v;
                        messageBox.NameOutput.Color = temp;
                    }
                    yield return 0;
                }
                while (Input.GetInputState(Inputs.A) != 1)
                {
                    if (messageBox.NameOutput.Color.A < 255)
                    {
                        var temp = messageBox.NameOutput.Color;
                        int v = temp.A > 235 ? 255 : temp.A + 20;
                        temp.A = (byte)v;
                        messageBox.NameOutput.Color = temp;
                    }
                    yield return 0;
                }
                yield return 0;
            }
        }

        /// <summary>
        /// 差分を変更する
        /// </summary>
        public class ChangeDiffElement : BaseTalkElement
        {
            public string DiffImage { get; set; }

            public ChangeDiffElement()
            {
            }

            public override IEnumerator Update(TalkComponent component, MessageBox messageBox)
            {
                CharacterImage.SelectedDiff = DiffImage;
                yield return 0;
            }
        }

        /// <summary>
        /// キャラクターを隠す
        /// </summary>
        public class HideCharacterElement : BaseTalkElement
        {
            public HideCharacterElement()
            {
            }

            public override IEnumerator Update(TalkComponent component, MessageBox messageBox)
            {
                if (component.Index.ContainsValue(CharacterImage))
                {
                    int index = component.Index.First(obj => obj.Value == CharacterImage).Key;
                    var targetPosition = new asd.Vector2DF(index < 2 ? -500 : ScalingLayer2D.OriginDisplaySize.X + 500, index == 1 || index == 2 ? 100 : 50);
                    var animation = new Animation();
                    animation.MoveTo(targetPosition, 30, Animation.Easing.OutSine);
                    CharacterImage.Animation.AddAnimation(CharacterImage, animation);
                    while (CharacterImage.Animation.IsAnimating)
                    {
                        yield return 0;
                    }
                    component.Index.Remove(index);
                }
                yield return 0;
            }
        }
    }
}
