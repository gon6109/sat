using BaseComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    /// <summary>
    /// 二択選択レイヤ
    /// </summary>
    public class YesNoLayer2D : asd.Layer2D
    {
        private bool _isYes;
        private IEnumerator enumerator;

        public asd.TextObject2D Title { get; private set; }
        public asd.TextObject2D Yes { get; private set; }
        public asd.TextObject2D No { get; private set; }
        public asd.TextureObject2D Cursor { get; private set; }

        /// <summary>
        /// 選択されたか
        /// </summary>
        public bool IsEnd { get; set; }

        /// <summary>
        /// Yesが選択されているか
        /// </summary>
        public bool IsYes
        {
            get => _isYes;
            set
            {
                _isYes = value;
                enumerator = AnimateCursor(value);
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="text">表示テキスト</param>
        public YesNoLayer2D(string text)
        {
            _isYes = true;
            Title = new asd.TextObject2D()
            {
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 100, new asd.Color(255, 255, 255), 0, new asd.Color()),
                Text = text,
            };
            Title.Position = new asd.Vector2DF(Base.ScreenSize.X / 2, 250) - Title.Font.CalcTextureSize(Title.Text, asd.WritingDirection.Horizontal).To2DF() / 2;
            AddObject(Title);

            Yes = new asd.TextObject2D()
            {
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 100, new asd.Color(255, 255, 255), 0, new asd.Color()),
                Text = "Yes",
            };
            Yes.Position = new asd.Vector2DF(Base.ScreenSize.X / 2, 500) - Yes.Font.CalcTextureSize(Yes.Text, asd.WritingDirection.Horizontal).To2DF() / 2;
            AddObject(Yes);

            No = new asd.TextObject2D()
            {
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 100, new asd.Color(255, 255, 255), 0, new asd.Color()),
                Text = "No",
            };
            No.Position = new asd.Vector2DF(Base.ScreenSize.X / 2, 625) - No.Font.CalcTextureSize(No.Text, asd.WritingDirection.Horizontal).To2DF() / 2;
            AddObject(No);

            Cursor = new asd.TextureObject2D()
            {
                Texture = TextureManager.LoadTexture("Static/cursor.png"),
                Position = new asd.Vector2DF(Base.ScreenSize.X / 2, 500),
            };
            Cursor.CenterPosition = Cursor.Texture.Size.To2DF() / 2;
            AddObject(Cursor);

            var bloom = new asd.PostEffectLightBloom();
            bloom.Exposure = 1;
            bloom.Intensity = 10;
            bloom.Threshold = 0;
            AddPostEffect(bloom);
        }

        protected override void OnUpdated()
        {
            if (IsEnd) return;
            if (!(enumerator != null && enumerator.MoveNext()))
            {
                if (Input.GetInputState(Inputs.A) == 1) IsEnd = true;
            }
            if (Input.GetInputState(Inputs.Up) == 1 || Input.GetInputState(Inputs.Down) == 1) IsYes = !IsYes;
            base.OnUpdated();
        }

        IEnumerator AnimateCursor(bool isYes)
        {
            float begin = Cursor.Position.Y;
            float difference = isYes ? 500 - Cursor.Position.Y : 625 - Cursor.Position.Y;
            float frame = 30;
            foreach (var item in Enumerable.Range(0, (int)frame))
            {
                Cursor.Position = new asd.Vector2DF(Cursor.Position.X, EaseOut(item, begin, difference, frame));
                yield return 0;
            }
            Cursor.Position = new asd.Vector2DF(Cursor.Position.X, isYes ? 500 : 625);
            yield return 0;
        }

        float EaseOut(float time, float begin, float difference, float frame)
        {
            time /= frame;
            time = time - 1;
            return difference * (time * time * time + 1) + begin;
        }
    }
}
