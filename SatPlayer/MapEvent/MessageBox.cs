using BaseComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.MapEvent
{
    public partial class TalkComponent
    {
        public class MessageBox : asd.TextureObject2D
        {
            public int TextSpeed { get; set; }

            public float Margin { get; set; }

            public asd.Font Font { get; set; }

            public string Name
            {
                get => NameOutput.Name;
                set => NameOutput.Name = value;
            }

            public int Index
            {
                get => NameOutput.Index;
                set => NameOutput.Index = value;
            }

            public NameArea NameOutput { get; private set; }

            List<TextLine> texts;

            public MessageBox()
            {
                Color = new asd.Color(255, 255, 255, 0);
                texts = new List<TextLine>();
                DrawingPriority = 3;
                Position = new asd.Vector2DF(110, 780);
                Texture = TextureManager.LoadTexture("Static/textbox.png");
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 30, new asd.Color(255, 255, 255), 0, new asd.Color());
                TextSpeed = 2;
                Margin = 10;
                NameOutput = new NameArea();
                AddChild(NameOutput,
                    asd.ChildManagementMode.IsUpdated | asd.ChildManagementMode.RegistrationToLayer | asd.ChildManagementMode.Disposal,
                    asd.ChildTransformingMode.Nothing);
            }

            protected override void OnUpdate()
            {
                base.OnUpdate();
            }

            public IEnumerator Open()
            {
                for (int i = 0; i < 15; i++)
                {
                    var temp = Color;
                    temp.A = (byte)(temp.A > 235 ? 255 : temp.A + 20);
                    Color = temp;
                    NameOutput.Color = new asd.Color(NameOutput.Color.R, NameOutput.Color.G, NameOutput.Color.B, Color.A);
                    yield return 0;
                }
            }

            public IEnumerator Close()
            {
                for (int i = 0; i < 15; i++)
                {
                    var temp = Color;
                    temp.A = (byte)(temp.A < 20 ? 0 : temp.A - 20);
                    Color = temp;
                    NameOutput.Color = new asd.Color(NameOutput.Color.R, NameOutput.Color.G, NameOutput.Color.B, Color.A);
                    yield return 0;
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
                    else if (Font.CalcTextureSize(temp + item, asd.WritingDirection.Horizontal).X < Texture.Size.X - Margin * 2)
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
                    Layer.AddObject(item);
                }
            }

            public class NameArea : asd.TextureObject2D
            {
                private int _index;
                private asd.TextObject2D name;

                public int Index
                {
                    get => _index;
                    set
                    {
                        if (value > -1 && value < 4)
                        {
                            _index = value;
                        }
                    }
                }

                public string Name
                {
                    get => name.Text;
                    set => name.Text = value;
                }

                asd.Vector2DF Target0 => new asd.Vector2DF(275, 730);
                asd.Vector2DF Target1 => new asd.Vector2DF(635, 730);
                asd.Vector2DF Target2 => new asd.Vector2DF(1035, 730);
                asd.Vector2DF Target3 => new asd.Vector2DF(1395, 730);

                public NameArea()
                {
                    Position = new asd.Vector2DF(275, 730);
                    DrawingPriority = 3;
                    name = new asd.TextObject2D();
                    Texture = TextureManager.LoadTexture("Static/namebox.png");
                    name.Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 30, new asd.Color(255, 255, 255), 0, new asd.Color());
                    AddChild(name, (asd.ChildManagementMode)0b1111, asd.ChildTransformingMode.Nothing);
                    name.DrawingPriority = 4;
                }

                asd.Vector2DF velocity = new asd.Vector2DF();
                protected override void OnUpdate()
                {
                    switch (Index)
                    {
                        case 0:
                            velocity.X = GetVelocity(Target0.X - Position.X);
                            velocity.Y = GetVelocity(Target0.Y - Position.Y);
                            break;
                        case 1:
                            velocity.X = GetVelocity(Target1.X - Position.X);
                            velocity.Y = GetVelocity(Target1.Y - Position.Y);
                            break;
                        case 2:
                            velocity.X = GetVelocity(Target2.X - Position.X);
                            velocity.Y = GetVelocity(Target2.Y - Position.Y);
                            break;
                        case 3:
                            velocity.X = GetVelocity(Target3.X - Position.X);
                            velocity.Y = GetVelocity(Target3.Y - Position.Y);
                            break;
                    }
                    Position += velocity;
                    name.Position = Position + Texture.Size.To2DF() / 2 - name.Font.CalcTextureSize(Name, asd.WritingDirection.Horizontal).To2DF() / 2;
                    name.Color = new asd.Color(name.Color.R, name.Color.G, name.Color.B, Color.A);
                    base.OnUpdate();
                }

                float GetVelocity(float distance)
                {
                    if (Math.Abs(distance) < 1.5f) return 0;
                    return Math.Abs(distance * 0.1f) > 1.0f ? distance * 0.1f : Math.Sign(distance) * 1.0f;
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
