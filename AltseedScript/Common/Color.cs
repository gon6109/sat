using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AltseedScript.Common
{
    /// <summary>
    /// 色
    /// </summary>
    public struct Color
    {
        private byte _r;
        private byte _g;
        private byte _b;
        private float _hue;
        private float _saturation;
        private float _value;

        /// <summary>
        /// Red
        /// </summary>
        public byte R
        {
            get => _r;
            set
            {
                _r = value;
                UpdateHsv();
            }
        }

        /// <summary>
        /// Green
        /// </summary>
        public byte G
        {
            get => _g;
            set
            {
                _g = value;
                UpdateHsv();
            }
        }

        /// <summary>
        /// Blue
        /// </summary>
        public byte B
        {
            get => _b;
            set
            {
                _b = value;
                UpdateHsv();
            }
        }

        /// <summary>
        /// Alpha
        /// </summary>
        public byte A { get; set; }

        /// <summary>
        /// 色相
        /// </summary>
        public float Hue
        {
            get => _hue;
            set
            {
                if (value < 0f || value > 360f) return;
                _hue = value;
                UpdateRgb();
            }
        }

        /// <summary>
        /// 彩度
        /// </summary>
        public float Saturation
        {
            get => _saturation;
            set
            {
                if (value < 0f || value > 1f) return;
                _saturation = value;
                UpdateRgb();
            }
        }

        /// <summary>
        /// 明度
        /// </summary>
        public float Value
        {
            get => _value;
            set
            {
                if (value < 0f || value > 1f) return;
                _value = value;
                UpdateRgb();
            }
        }

        public Color(byte r, byte g, byte b) : this(r, g, b, 255)
        {
        }

        public Color(byte r, byte g, byte b, byte a)
        {
            _r = r;
            _g = g;
            _b = b;
            A = a;
            _hue = 0;
            _saturation = 0;
            _value = 0;
            UpdateHsv();
        }

        public Color(float hue, float saturation, float value) : this(hue, saturation, value, 255)
        {
        }

        public Color(float hue, float saturation, float value, byte a)
        {
            _r = 0;
            _g = 0;
            _b = 0;
            _hue = hue;
            _saturation = saturation;
            _value = value;
            A = a;
            UpdateRgb();
        }

        void UpdateHsv()
        {
            float r = R / 255f;
            float g = G / 255f;
            float b = B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            _value = max;

            if (max == min)
            {
                //undefined
                _hue = 0f;
                _saturation = 0f;
            }
            else
            {
                float c = max - min;

                if (max == r)
                {
                    _hue = (g - b) / c;
                }
                else if (max == g)
                {
                    _hue = (b - r) / c + 2f;
                }
                else
                {
                    _hue = (r - g) / c + 4f;
                }
                _hue *= 60f;
                if (_hue < 0f)
                {
                    _hue += 360f;
                }

                _saturation = c / max;
            }
        }

        void UpdateRgb()
        {
            float r = Value;
            float g = Value;
            float b = Value;
            if (Saturation > 0)
            {
                int Hi = (int)(Math.Floor(Hue / 60.0f) % 6.0f);
                float f = (Hue / 60.0f) - Hi;

                float p = Value * (1 - Saturation);
                float q = Value * (1 - f * Saturation);
                float t = Value * (1 - (1 - f) * Saturation);

                switch (Hi)
                {
                    case 0: r = Value; g = t; b = p; break;
                    case 1: r = q; g = Value; b = p; break;
                    case 2: r = p; g = Value; b = t; break;
                    case 3: r = p; g = q; b = Value; break;
                    case 4: r = t; g = p; b = Value; break;
                    case 5: r = Value; g = p; b = q; break;
                    default:
                        break;
                }
            }
            _r = (byte)r;
            _g = (byte)g;
            _b = (byte)b;
        }
    }
}
