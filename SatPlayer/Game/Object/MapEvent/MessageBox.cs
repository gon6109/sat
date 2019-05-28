using BaseComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game.Object.MapEvent
{
    public partial class TalkComponent
    {
        /// <summary>
        /// テキストボックス
        /// </summary>
        public class MessageBox : asd.TextureObject2D
        {
            public static asd.Vector2DF Size => new asd.Vector2DF(1700f, 300f);

            /// <summary>
            /// テキストを表示するスピード
            /// </summary>
            public int TextSpeed { get; set; }

            /// <summary>
            /// テキストとボックスの間隔
            /// </summary>
            public float Margin { get; set; }

            /// <summary>
            /// フォント
            /// </summary>
            public asd.Font Font { get; set; }

            /// <summary>
            /// 話しているキャラクター名
            /// </summary>
            public string Name
            {
                get => NameOutput.Name;
                set => NameOutput.Name = value;
            }

            /// <summary>
            /// 話しているキャラクターの場所
            /// </summary>
            public int Index
            {
                get => NameOutput.Index;
                set => NameOutput.Index = value;
            }

            /// <summary>
            /// キャラクター名表示欄
            /// </summary>
            public NameArea NameOutput { get; private set; }

            public AnimationComponent Animation => GetComponent("animation") as AnimationComponent;

            List<TextLine> texts;

            public MessageBox()
            {
                Color = new asd.Color(255, 255, 255, 0);
                texts = new List<TextLine>();
                DrawingPriority = 3;
                Position = new asd.Vector2DF(110, 780);
                Texture = MapEventResource.Instance.MessageBoxTexture;
                Font = MapEventResource.Instance.MessageFont;
                Scale = new asd.Vector2DF(Size.X / Texture.Size.X, Size.Y / Texture.Size.Y);
                TextSpeed = 2;
                Margin = 10;
                NameOutput = new NameArea();
                AddComponent(new AnimationComponent(), "animation");
                AddDrawnChild(NameOutput,
                    asd.ChildManagementMode.IsUpdated | asd.ChildManagementMode.RegistrationToLayer | asd.ChildManagementMode.Disposal,
                    asd.ChildTransformingMode.Nothing, asd.ChildDrawingMode.Color);
            }

            protected override void OnUpdate()
            {
                base.OnUpdate();
            }

            public IEnumerator Open()
            {
                var animation = new Animation();
                animation.Alpha(0, 255, 30);
                Animation.AddAnimation(this, animation);
                Animation.AddAnimation(NameOutput, animation, 1);
                while (Animation.IsAnimating)
                {
                    yield return null;
                }
            }

            public IEnumerator Close()
            {
                var animation = new Animation();
                animation.Alpha(255, 0, 30);
                Animation.AddAnimation(this, animation);
                Animation.AddAnimation(NameOutput, animation, 1);
                while (Animation.IsAnimating)
                {
                    yield return null;
                }
            }

            public IEnumerator ShowText()
            {
                int count = 0;
                int total = 0;
                while (true)
                {
                    if (count % TextSpeed == 0)
                    {
                        bool isEnd = true;
                        foreach (var item in texts)
                        {
                            if (item.Text == item.Line) continue;
                            item.Text = item.Line.Substring(0, count / TextSpeed - total);
                            if (item.Text == item.Line) total += item.Line.Length;
                            isEnd = false;
                            break;
                        }
                        if (isEnd) break;
                    }
                    count++;
                    yield return 0;
                }
                yield return 0;
            }

            /// <summary>
            /// テキストを表示させる
            /// </summary>
            /// <param name="text">表示するテキスト</param>
            public void SetMessage(string text)
            {
                string temp = "";
                int l = 0;
                foreach (var item in texts)
                {
                    item.Dispose();
                }
                texts.Clear();
                TextLine textObject;
                foreach (var item in text)
                {
                    if (item == '\n')
                    {
                        textObject = new TextLine();
                        textObject.Font = Font;
                        textObject.Position = Position +
                            new asd.Vector2DF(Margin,
                            Margin + Font.CalcTextureSize(" ", asd.WritingDirection.Horizontal).Y * 1.2f * (l++));
                        textObject.Line = temp;
                        temp = "";
                        texts.Add(textObject);
                    }
                    else if (Font.CalcTextureSize(temp + item, asd.WritingDirection.Horizontal).X < Size.X - Margin * 2)
                        temp += item;
                    else
                    {
                        textObject = new TextLine();
                        textObject.Font = Font;
                        textObject.Position = Position +
                            new asd.Vector2DF(Margin,
                           Margin + Font.CalcTextureSize(" ", asd.WritingDirection.Horizontal).Y * 1.2f * (l++));
                        textObject.Line = temp;
                        temp = item.ToString();
                        texts.Add(textObject);
                    }
                }
                textObject = new TextLine();
                textObject.Font = Font;
                textObject.Position = Position +
                    new asd.Vector2DF(Margin,
                    Margin + Font.CalcTextureSize(" ", asd.WritingDirection.Horizontal).Y * 1.2f * (l++));
                textObject.Line = temp;
                texts.Add(textObject);
                foreach (var item in texts)
                {
                    AddDrawnChild(item,
                    asd.ChildManagementMode.IsUpdated | asd.ChildManagementMode.RegistrationToLayer | asd.ChildManagementMode.Disposal,
                    asd.ChildTransformingMode.Nothing, asd.ChildDrawingMode.Color);
                }
            }

            public class NameArea : asd.TextureObject2D
            {
                public static asd.Vector2DF Size => new asd.Vector2DF(300f, 50f);

                private int _index;
                private asd.TextObject2D name;

                public AnimationComponent Animation => GetComponent("animation") as AnimationComponent;

                public int Index
                {
                    get => _index;
                    set
                    {
                        if (value > -1 && value < 4)
                        {
                            _index = value;
                            var animation = new Animation();
                            animation.MoveTo(Target[_index], 20, BaseComponent.Animation.Easing.OutSine);
                            Animation.AddAnimation(this, animation);
                        }
                    }
                }

                public string Name
                {
                    get => name.Text;
                    set => name.Text = value;
                }

                asd.Vector2DF[] Target { get; } = new[]
                {
                    new asd.Vector2DF(275, 730),
                    new asd.Vector2DF(635, 730),
                    new asd.Vector2DF(1035, 730),
                    new asd.Vector2DF(1395, 730)
                };

                public NameArea()
                {
                    Position = new asd.Vector2DF(275, 730);
                    DrawingPriority = 3;
                    name = new asd.TextObject2D();
                    Texture = MapEventResource.Instance.NameBoxTexture;
                    name.Font = MapEventResource.Instance.NameFont;
                    Scale = new asd.Vector2DF(Size.X / Texture.Size.X, Size.Y / Texture.Size.Y);
                    AddDrawnChild(name, (asd.ChildManagementMode)0b1111, asd.ChildTransformingMode.Nothing, asd.ChildDrawingMode.Color);
                    name.DrawingPriority = 4;
                    AddComponent(new AnimationComponent(), "animation");
                }

                asd.Vector2DF velocity = new asd.Vector2DF();
                protected override void OnUpdate()
                {
                    base.OnUpdate();
                    name.Position = Position + Size / 2 - name.Font.CalcTextureSize(Name, asd.WritingDirection.Horizontal).To2DF() / 2;
                }
            }

            class TextLine : asd.TextObject2D
            {
                public string Line { get; set; }

                public TextLine()
                {
                    DrawingPriority = 4;
                }
            }
        }
    }
}
