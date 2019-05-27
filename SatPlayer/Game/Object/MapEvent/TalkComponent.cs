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
        /// テキストボックス
        /// </summary>
        public MessageBox Text { get; private set; }

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
            asd.Engine.CurrentScene.AddLayer(layer);
            layer.DrawingPriority = 3;

            Text = new MessageBox();
            layer.AddObject(Text);
            foreach (var item in CharacterImages)
            {
                item.Position = new asd.Vector2DF(-200, 0);
                layer.AddObject(item);
            }

            var messageIterator = Text.Open();
            while (messageIterator.MoveNext())
            {
                CharacterImages.ForEach(obj => obj.Color = new asd.Color(255, 255, 255, (int)Text.Color.A));
                yield return 0;
            }
            foreach (var item in TalkElements)
            {
                var updateIterator = item.Update(this);
                while (updateIterator.MoveNext())
                {
                    yield return 0;
                }
            }
            messageIterator = Text.Close();
            while (messageIterator.MoveNext())
            {
                CharacterImages.ForEach(obj => obj.Color = new asd.Color(255, 255, 255, (int)Text.Color.A));
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

            public virtual IEnumerator Update(TalkComponent component)
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

            public override IEnumerator Update(TalkComponent component)
            {
                CharacterImage.Position = new asd.Vector2DF(Index < 2 ? -200 : ScalingLayer2D.OriginDisplaySize.X + 200, Index == 1 || Index == 2 ? 100 : 50);
                component.Text.Index = Index;
                component.Text.Name = CharacterImage.Name;
                var targetPosition = new asd.Vector2DF(GetXByIndex(), Index == 1 || Index == 2 ? 100 : 50);
                while ((targetPosition - CharacterImage.Position).Length > 2)
                {
                    asd.Vector2DF velocity = new asd.Vector2DF();

                    velocity.X = GetVelocity((targetPosition - CharacterImage.Position).X);
                    velocity.Y = GetVelocity((targetPosition - CharacterImage.Position).Y);

                    CharacterImage.Position += velocity;

                    if (component.Text.NameOutput.Color.A < 255)
                    {
                        var temp = component.Text.NameOutput.Color;
                        int v = temp.A > 235 ? 255 : temp.A + 20;
                        temp.A = (byte)v;
                        component.Text.NameOutput.Color = temp;
                    }
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

            float GetVelocity(float distance)
            {
                if (Math.Abs(distance) < 1.5f) return 0;
                return Math.Abs(distance * 0.1f) > 1.0f ? distance * 0.1f : Math.Sign(distance) * 1.0f;
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

            public override IEnumerator Update(TalkComponent component)
            {
                component.Text.SetMessage(Text);
                if (component.Index.Any(obj => obj.Value == CharacterImage))
                {
                    component.Text.Name = CharacterImage.Name;
                    component.Text.Index = component.Index.First(obj => obj.Value == CharacterImage).Key;
                }
                else
                {
                    component.Text.Name = new string('?', CharacterImage.Name.Length);
                    component.Text.Index = 3;
                }
                var iterator = component.Text.ShowText();
                while (iterator.MoveNext())
                {
                    if (component.Text.NameOutput.Color.A < 255)
                    {
                        var temp = component.Text.NameOutput.Color;
                        int v = temp.A > 235 ? 255 : temp.A + 20;
                        temp.A = (byte)v;
                        component.Text.NameOutput.Color = temp;
                    }
                    yield return 0;
                }
                while (Input.GetInputState(Inputs.A) != 1)
                {
                    if (component.Text.NameOutput.Color.A < 255)
                    {
                        var temp = component.Text.NameOutput.Color;
                        int v = temp.A > 235 ? 255 : temp.A + 20;
                        temp.A = (byte)v;
                        component.Text.NameOutput.Color = temp;
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

            public override IEnumerator Update(TalkComponent component)
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

            public override IEnumerator Update(TalkComponent component)
            {
                if (component.Index.ContainsValue(CharacterImage))
                {
                    int index = component.Index.First(obj => obj.Value == CharacterImage).Key;
                    var targetPosition = new asd.Vector2DF(index < 2 ? -200 : ScalingLayer2D.OriginDisplaySize.X + 200, index == 1 || index == 2 ? 100 : 50);
                    while ((targetPosition - CharacterImage.Position).Length > 2)
                    {
                        asd.Vector2DF velocity = new asd.Vector2DF();

                        velocity.X = GetVelocity((targetPosition - CharacterImage.Position).X);
                        velocity.Y = GetVelocity((targetPosition - CharacterImage.Position).Y);
                        CharacterImage.Position += velocity;

                        if (component.Text.NameOutput.Color.A > 0)
                        {
                            var temp = component.Text.NameOutput.Color;
                            int v = temp.A < 20 ? 0 : temp.A - 20;
                            temp.A = (byte)v;
                            component.Text.NameOutput.Color = temp;
                        }
                        yield return 0;
                    }
                    component.Index.Remove(index);
                }
                yield return 0;
            }

            float GetVelocity(float distance)
            {
                if (Math.Abs(distance) < 1.5f) return 0;
                return Math.Abs(distance * 0.1f) > 1.0f ? distance * 0.1f : Math.Sign(distance) * 1.0f;
            }
        }
    }
}
