using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.UI
{
    /// <summary>
    /// ボタン
    /// </summary>
    public class Button : UIElement //継承してるん互
    {
        /// <summary>
        /// アニメーション処理用
        /// </summary>
        protected IEnumerator<int> enumerator;

        private Sound select;

        public Action<object> OnPushed { get; set; }

        /// <summary>
        /// 選択されているか
        /// </summary>
        public override bool IsSelected
        {
            get => base.IsSelected;
            set
            {
                base.IsSelected = value;
                enumerator = Update(value);
            }
        }

        public override bool IsEnable
        {
            get => base.IsEnable;
            set
            {
                base.IsEnable = value;
                var temp = HsvColor.FromRgb(Color);
                if (!value) temp.V = 0.3f;
                else temp.V = 1;
                Color = temp.ToRgb();
            }
        }

        /// <summary>
        /// 決定したときどこに遷移するか
        /// </summary>
        public string NextScenePath { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Button()
        {
            select = new Sound("Sound/UI/select2.wav", false);
            OnPushed = (obj) => { };
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void OnUpdate()
        {
            if (Input.GetInputState(Inputs.A) == 1 && IsSelected)
            {
                select.Play();
                IsFocused = true;
                if (NextScenePath != "") asd.Engine.ChangeSceneWithTransition(new UIScene(NextScenePath), new asd.TransitionFade(0.7f, 0.9f));
                else OnPushed(this);
            }

            if (enumerator != null) enumerator.MoveNext();
            base.OnUpdate();
        }

        protected override void OnDispose()
        {
            OnPushed = (obj) => { };
            base.OnDispose();
        }

        /// <summary>
        /// アニメーション
        /// </summary>
        /// <param name="isSelected">選択されたか</param>
        /// <returns></returns>
        IEnumerator<int> Update(bool isSelected)
        {
            if (isSelected)
            {
                DrawingPriority = 1;
                float angle = (float)Math.PI / 2;
                float extend = 50f;
                for (float i = -(float)Math.PI / 2; i < Math.PI / 2; i += 0.8f)
                {
                    Scale = new asd.Vector2DF(1, 1) +
                        new asd.Vector2DF(
                            extend * (float)Math.Cos(Math.Atan2(Texture.Size.Y, Texture.Size.X)) / Texture.Size.X,
                            extend * (float)Math.Sin(Math.Atan2(Texture.Size.Y, Texture.Size.X)) / Texture.Size.Y)
                        * ((float)Math.Sin(i) / 2 + 0.5f);
                    angle += 0.12f;
                    yield return 0;
                }
                while (true)
                {
                    Scale = new asd.Vector2DF(1, 1) +
                        new asd.Vector2DF(
                            extend * (float)Math.Cos(Math.Atan2(Texture.Size.Y, Texture.Size.X)) / Texture.Size.X,
                            extend * (float)Math.Sin(Math.Atan2(Texture.Size.Y, Texture.Size.X)) / Texture.Size.Y)
                        * ((float)Math.Sin(angle) / 2 + 0.5f);
                    angle += 0.12f;
                    yield return 0;
                }
            }
            else
            {
                DrawingPriority = 0;
                while (Scale.X > 1.0f)
                {
                    Scale = new asd.Vector2DF(Scale.X - 0.01f, Scale.Y - 0.01f);
                    yield return 0;
                }
                Scale = new asd.Vector2DF(1, 1);
                yield return 0;
            }
        }
    }
}
