using BaseComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.UI
{
    /// <summary>
    /// UI用シーン
    /// </summary>
    public class UIScene : asd.Scene
    {
        /// <summary>
        /// ファイルとデータを紐付ける
        /// </summary>
        public static Dictionary<string, object> DataContexts = new Dictionary<string, object>();

        protected UILayer2D layer; //privateは付けなくて良き
        private Sound move;

        /// <summary>
        /// 戻るしたときに遷移するシーンへのパス
        /// </summary>
        public string PreviousScenePath { get; set; } 

        /// <summary>
        /// 選択されているUIElement
        /// </summary>
        public UIElement SelectedElement
        {
            get => layer.SelectedElement;
            set => layer.SelectedElement = value;
        }

        public UIScene(string uiDataPath)
        {
            layer = new UILayer2D(uiDataPath);
            AddLayer(layer);

            move = new Sound("Sound/UI/select4.wav", true);
        }

        public UIScene()
        {
            layer = new UILayer2D();
            AddLayer(layer);

            move = new Sound("Sound/UI/select4.wav", true);
        }

        public void Connect(List<UIElement> uiElements = null)
        {
            layer.Connect(uiElements);
        }
    }

    /// <summary>
    /// UI用レイヤー
    /// </summary>
    public class UILayer2D : ScalingLayer2D
    {
        private UIElement _selectedElement;
        private Sound move;

        /// <summary>
        /// 選択されているUIElement
        /// </summary>
        public UIElement SelectedElement
        {
            get => _selectedElement;
            set
            {
                if (SelectedElement == value) return;
                value.IsSelected = true;
                if (SelectedElement != null) _selectedElement.IsSelected = false;
                _selectedElement = value;
            }
        }

        public UILayer2D(string uiDataPath)
        {
            move = new Sound("Sound/UI/select4.wav", true);

            // イメージパッケージを読み込む
            asd.ImagePackage imagePackage = asd.Engine.Graphics.CreateImagePackage(uiDataPath);
            var uiElements = new List<UIElement>();

            for (int i = 0; i < imagePackage.ImageCount; i++)
            {
                // テクスチャを取り出す
                asd.Texture2D texture = imagePackage.GetImage(i);
                asd.RectI area = imagePackage.GetImageArea(i);

                string layerName = imagePackage.GetImageName(i);
                string[] elements = layerName.Split(' ');
                if (elements[0] == "Button")
                {
                    Dictionary<string, string> options = new Dictionary<string, string>();
                    foreach (var item in elements.Where(obj => obj != "Button"))
                    {
                        var temp = item.Split('=');
                        options.Add(temp[0], temp[1]);
                    }
                    Button button = new Button();
                    button.Texture = texture;
                    button.Position = new asd.Vector2DF(area.X + area.Width / 2, area.Y + area.Height / 2);
                    button.NextScenePath = options.ContainsKey("to") && options["to"].Contains(".aip") ? options["to"] : "";
                    if (options.ContainsKey("to") && !options["to"].Contains(".aip")
                        && UIScene.DataContexts.ContainsKey(uiDataPath))
                        button.OnPushed = (Action<object>)UIScene.DataContexts[uiDataPath].GetType().GetMethod(options["to"]).CreateDelegate(typeof(Action<object>), UIScene.DataContexts[uiDataPath]);
                    try
                    {
                        button.IsEnable = options.ContainsKey("enable") ? Convert.ToBoolean(options["enable"]) : true;
                    }
                    catch
                    {
                        button.IsEnable = (bool)UIScene.DataContexts[uiDataPath].GetType().GetProperty(options["enable"]).GetValue(UIScene.DataContexts[uiDataPath]);
                    }
                    AddObject(button);
                    //TO DO buttonsに全buttonに入れる n番目まで入れたい
                    uiElements.Add(button); //ctrl + rr

                }
                else if (elements[0] == "Guage")
                {
                    Dictionary<string, string> options = new Dictionary<string, string>();
                    foreach (var item in elements.Where(obj => obj != "Guage"))
                    {
                        var temp = item.Split('=');
                        options.Add(temp[0], temp[1]);
                    }
                    Gauge gauge = new Gauge(UIScene.DataContexts.ContainsKey(uiDataPath) ? UIScene.DataContexts[uiDataPath] : null,
                        options.ContainsKey("path") ? options["path"] : null,
                        options.ContainsKey("min") ? Convert.ToSingle(options["min"]) : 0,
                        options.ContainsKey("max") ? Convert.ToSingle(options["max"]) : 1);
                    gauge.Texture = texture;
                    gauge.Position = new asd.Vector2DF(area.X + area.Width / 2, area.Y + area.Height / 2);
                    gauge.IsEnable = options.ContainsKey("enable") ? Convert.ToBoolean(options["enable"]) : true;
                    AddObject(gauge);
                    uiElements.Add(gauge);
                }
                else
                {
                    asd.TextureObject2D textureObject2D = new asd.TextureObject2D();
                    textureObject2D.Texture = texture;
                    textureObject2D.Position = new asd.Vector2DF(area.X, area.Y);
                    AddObject(textureObject2D);
                }
            }

            Connect(uiElements.Where(obj => obj.IsEnable).ToList());
        }

        public UILayer2D()
        {
            move = new Sound("Sound/UI/select4.wav", true);
        }

        public void Connect(List<UIElement> uiElements = null)
        {
            if (uiElements == null) uiElements = Objects.Where(obj => obj is UIElement)
                    .Cast<UIElement>()
                    .Where(obj => obj.IsEnable).ToList();
            if (uiElements == null) return;

            foreach (var item in uiElements)
            {
                item.ResetConnection();
            }

            //並び変える(全部listに入った後)
            uiElements.Sort((a, b) => a.Texture.Size.X * a.Texture.Size.Y - b.Texture.Size.X * b.Texture.Size.Y);
            //TO DO ここから手つける　メイン！！
            foreach (var item in uiElements)
            {
                foreach (var item2 in uiElements.Where(obj => obj != item)) //linq : とりま selectで写像 whereでフィルター
                {
                    var angle = (item2.Position - item.Position).Degree;
                    if (angle >= (-item.Texture.Size.To2DF()).Degree && angle < new asd.Vector2DF(item.Texture.Size.X / 2, -item.Texture.Size.Y / 2).Degree)
                    {
                        if (item.Up == null) item.Up = item2;
                        else
                        {
                            if ((item.Up.Position - item.Position).Length >= (item2.Position - item.Position).Length) item.Up = item2;
                        }
                    }
                    if (angle >= new asd.Vector2DF(item.Texture.Size.X / 2, -item.Texture.Size.Y / 2).Degree && angle < (item.Texture.Size.To2DF()).Degree)
                    {
                        if (item.Right == null) item.Right = item2;
                        else
                        {
                            if ((item.Right.Position - item.Position).Length >= (item2.Position - item.Position).Length) item.Right = item2;
                        }
                    }
                    if (angle >= (item.Texture.Size.To2DF()).Degree && angle < new asd.Vector2DF(-item.Texture.Size.X / 2, item.Texture.Size.Y / 2).Degree)
                    {
                        if (item.Down == null) item.Down = item2;
                        else
                        {
                            if ((item.Down.Position - item.Position).Length >= (item2.Position - item.Position).Length) item.Down = item2;
                        }
                    }
                    if ((angle >= new asd.Vector2DF(-item.Texture.Size.X / 2, item.Texture.Size.Y / 2).Degree && angle <= 180)
                        || (angle >= -180 && angle < (-item.Texture.Size.To2DF()).Degree))
                    {
                        if (item.Left == null) item.Left = item2;
                        else
                        {
                            if ((item.Left.Position - item.Position).Length >= (item2.Position - item.Position).Length) item.Left = item2;
                        }
                    }
                }
            }
            SelectedElement = uiElements.Count != 0 ? uiElements[uiElements.Count - 1] : null;
        }

        protected override void OnUpdated()
        {
            if (Objects.Where(obj => obj is UIElement).Cast<UIElement>().Any(obj => obj.IsFocused)) return;
            if (Input.GetInputState(Inputs.Up) == 1 || Input.GetInputState(Inputs.Right) == 1
                || Input.GetInputState(Inputs.Left) == 1 || Input.GetInputState(Inputs.Down) == 1) move.Play();
            if (Input.GetInputState(Inputs.Up) == 1) SelectedElement = SelectedElement.Up != null ? SelectedElement.Up : SelectedElement;
            if (Input.GetInputState(Inputs.Right) == 1) SelectedElement = SelectedElement.Right != null ? SelectedElement.Right : SelectedElement;
            if (Input.GetInputState(Inputs.Left) == 1) SelectedElement = SelectedElement.Left != null ? SelectedElement.Left : SelectedElement;
            if (Input.GetInputState(Inputs.Down) == 1) SelectedElement = SelectedElement.Down != null ? SelectedElement.Down : SelectedElement;
            base.OnUpdated();
        }
    }
}
