using BaseComponent;
using SatIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    /// <summary>
    /// セーブデータ表示コンポーネント
    /// </summary>
    public class SaveDataComponent : UI.Button
    {
        public asd.TextObject2D DataName { get; private set; }
        public asd.TextObject2D MapName { get; private set; }
        public asd.TextObject2D PlayerNames { get; private set; }
        public asd.TextObject2D PlayTime { get; private set; }

        public SaveDataIO SaveDataIO { get; private set; }
        public string Path { get; private set; }

        public SaveDataComponent(string path, float hue = 0f)
        {
            Path = path;
            SaveDataIO = new SaveDataIO();
            if (asd.Engine.File.Exists(path)) SaveDataIO = SaveDataIO.Load<SaveDataIO>(path);
            Texture = TextureManager.LoadTexture("Static/save_data.png");
            Color = new HsvColor(hue, 0.6f, 1).ToRgb();

            DataName = new asd.TextObject2D()
            {
                Text = path.Split('/').Last(),
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 60, new asd.Color(255, 255, 255), 0, new asd.Color()),
            };
            DataName.Position = new asd.Vector2DF(-Texture.Size.X / 4, 0) - DataName.Font.CalcTextureSize(DataName.Text, asd.WritingDirection.Horizontal).To2DF() / 2;

            MapName = new asd.TextObject2D()
            {
                Text = SaveDataIO.MapName != null ? SaveDataIO.MapName : "",
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 40, new asd.Color(255, 255, 255), 0, new asd.Color()),
            };
            MapName.Position = new asd.Vector2DF(Texture.Size.X / 4, -Texture.Size.Y / 4) - MapName.Font.CalcTextureSize(MapName.Text, asd.WritingDirection.Horizontal).To2DF() / 2;

            PlayerNames = new asd.TextObject2D()
            {
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 40, new asd.Color(255, 255, 255), 0, new asd.Color()),
            };
            foreach (var item in SaveDataIO.PlayingChacacter)
            {
                PlayerNames.Text += item + " ";
            }
            PlayerNames.Position = new asd.Vector2DF(Texture.Size.X / 4, 0) - PlayerNames.Font.CalcTextureSize(PlayerNames.Text, asd.WritingDirection.Horizontal).To2DF() / 2;

            PlayTime = new asd.TextObject2D()
            {
                Text = new TimeSpan(0, 0, SaveDataIO.Time).ToString(),
                Font = asd.Engine.Graphics.CreateDynamicFont(Base.MainFont, 40, new asd.Color(255, 255, 255), 0, new asd.Color()),
            };
            PlayTime.Position = new asd.Vector2DF(Texture.Size.X / 4, Texture.Size.Y / 4) - PlayTime.Font.CalcTextureSize(PlayTime.Text, asd.WritingDirection.Horizontal).To2DF() / 2;

            NextScenePath = "";
            OnPushed = (obj) => { };
        }

        protected override void OnAdded()
        {
            AddDrawnChild(DataName, (asd.ChildManagementMode)0b1111, asd.ChildTransformingMode.All, asd.ChildDrawingMode.DrawingPriority);
            AddDrawnChild(MapName, (asd.ChildManagementMode)0b1111, asd.ChildTransformingMode.All, asd.ChildDrawingMode.DrawingPriority);
            AddDrawnChild(PlayerNames, (asd.ChildManagementMode)0b1111, asd.ChildTransformingMode.All, asd.ChildDrawingMode.DrawingPriority);
            AddDrawnChild(PlayTime, (asd.ChildManagementMode)0b1111, asd.ChildTransformingMode.All, asd.ChildDrawingMode.DrawingPriority);

            base.OnAdded();
        }
        
        protected override void OnUpdate()
        {
            if (!IsEnable)
            {
                foreach (asd.DrawnObject2D item in Children)
                {
                    item.Color = new asd.Color(100, 100, 100);
                }
            }
            base.OnUpdate();
        }

        protected override void OnDispose()
        {
            OnPushed = (obj) => { };
            base.OnDispose();
        }
    }
}
