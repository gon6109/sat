using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.UI
{
    /// <summary>
    /// ゲージ
    /// </summary>
    public class Gauge : UIElement
    {
        private float _value;
        private float _min;
        private float _max;
        private string _bindingPath;
        private asd.TextureObject2D back;

        /// <summary>
        /// 同期プロパティの元インスタンス
        /// </summary>
        public object BindingSource { get; set; }

        /// <summary>
        /// 同期プロパティ名
        /// </summary>
        public string BindingPath
        {
            get => _bindingPath;
            set
            {
                if (BindingSource?.GetType().GetProperty(value)?.GetValue(BindingSource) != null)
                {
                    try
                    {
                        _value = (float)BindingSource.GetType().GetProperty(value).GetValue(BindingSource);
                        _bindingPath = value;
                    }
                    catch
                    {
                        _bindingPath = null;
                    }
                }
            }
        }

        /// <summary>
        /// [Min,Max]の値
        /// </summary>
        public float Value
        {
            get => _value;
            set
            {
                if (value >= Min && value <= Max)
                {
                    _value = value;
                    if (BindingPath != null)
                    {
                        BindingSource.GetType().GetProperty(BindingPath).SetValue(BindingSource, value);
                    }
                }
            }
        }

        /// <summary>
        /// 最小値
        /// </summary>
        public float Min
        {
            get => _min;
            set
            {
                if (Max > value) _min = value;
            }
        }

        /// <summary>
        /// 最大値
        /// </summary>
        public float Max
        {
            get => _max;
            set
            {
                if (Min < value) _max = value;
            }
        }

        /// <summary>
        /// 表示するテクスチャ
        /// </summary>
        public new asd.Texture2D Texture
        {
            get => base.Texture;
            set
            {
                base.Texture = value;
                back.Texture = value;
            }
        }

        /// <summary>
        /// コンストラクタ（初期化）
        /// </summary>
        /// <param name="bindingSouce">同期プロパティの元インスタンス</param>
        /// <param name="bindingPath">同期プロパティ名</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        public Gauge(object bindingSouce, string bindingPath, float min, float max)
        {
            if (min >= max) throw new FormatException("引数が条件を満たしてません");
            _min = min;
            _max = max;
            BindingSource = bindingSouce;
            BindingPath = bindingPath;
            back = new asd.TextureObject2D();
            back.Color = new asd.Color(50, 50, 50);
            DrawingPriority = 1;
            AddChild(back, asd.ChildManagementMode.Disposal | asd.ChildManagementMode.RegistrationToLayer, asd.ChildTransformingMode.Position);
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        protected override void OnUpdate()
        {
            if (Texture == null) return;
            if (Input.GetInputState(Inputs.A) == 1 && IsSelected)
            {
                IsFocused = true;
            }
            if (IsFocused)
            {
                if (Input.GetInputState(Inputs.Left) > 0) Value -= (Max - Min) / 100;
                if (Input.GetInputState(Inputs.Right) > 0) Value += (Max - Min) / 100;
            }
            var rect = new asd.RectF(new asd.Vector2DF(), Texture.Size.To2DF());
            rect.Width *= (Value - Min) / (Max - Min);
            Src = rect;
        }
    }
}
