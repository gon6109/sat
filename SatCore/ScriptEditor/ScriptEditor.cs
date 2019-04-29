using BaseComponent;
using SatCore.Attribute;
using SatPlayer.Game.Object;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.ScriptEditor
{
    /// <summary>
    /// スクリプト編集シーン
    /// </summary>
    public class ScriptEditor : UndoRedoScene
    {
        public IScriptObject ScriptObject { get; private set; }

        PhysicAltseed.PhysicalWorld PhysicalWorld { get; set; }
        MapLayer MainLayer { get; set; }

        public ScriptType Script { get; }

        public string Path { get; set; }

        [Button("オブジェクトをクリア")]
        public void ClearScriptObject()
        {
            if (ScriptObject.IsSingle) return;

            foreach (var item in MainLayer.Objects.Where(obj => obj is IScriptObject || obj is MultiAnimationObject2D))
            {
                item.Dispose();
            }
        }

        public ScriptEditor(ScriptType scriptType, string path = "")
        {
            Path = path;
            Script = scriptType;

            MainLayer = new MapLayer();
            PhysicalWorld = MainLayer.PhysicalWorld;

            CreateObject();
            try
            {
                ScriptObject.Code = Encoding.UTF8.GetString(IO.GetStream(path).ToArray());
            }
            catch
            {
                ScriptObject.Code = "";
            }
            ScriptObject.Run();

            MainLayer.IsPreparePlayer = ScriptObject.IsPreparePlayer;

            asd.RectF[] rects = { new asd.RectF(0, 30, 30, 1080), new asd.RectF(30, 1050, 1890, 30), new asd.RectF(1890, 0, 30, 1050), new asd.RectF(0, 0, 1890, 30) };
            foreach (var item in rects)
            {
                var groundShape = new PhysicAltseed.PhysicalRectangleShape(PhysicAltseed.PhysicalShapeType.Static, PhysicalWorld);
                groundShape.DrawingArea = item;
                groundShape.Friction = 0;
                MainLayer.AddObject(new asd.GeometryObject2D() { Shape = groundShape, Color = new asd.Color(255, 255, 255) });
                MainLayer.Obstacles.Add(groundShape);
            }

            if (ScriptObject.IsSingle && ScriptObject is asd.Object2D obj) MainLayer.AddObject(obj);
            AddLayer(MainLayer);

            MainLayer.IsUpdateScalingAuto = true;
            MainLayer.IsFixAspectRatio = true;
        }

        void CreateObject()
        {
            switch (Script)
            {
                case ScriptType.MapObject:
                    ScriptObject = new EditableMapObject();
                    break;
                case ScriptType.EventObject:
                    ScriptObject = new EditableEventObject();
                    break;
                case ScriptType.Player:
                    ScriptObject = new EditablePlayer();
                    break;
                case ScriptType.BackGround:
                    ScriptObject = new EditableBackGround();
                    break;
                default:
                    throw new NotImplementedException(Script.ToString());
            }
        }

        protected override void OnUpdating()
        {
            PhysicalWorld.Update();
            base.OnUpdating();
        }

        protected override void OnUpdated()
        {
            if (Mouse.LeftButton == asd.ButtonState.Push && ScriptObject.IsSuccessBuild && !ScriptObject.IsSingle)
            {
                if (ScriptObject.Clone() is asd.Object2D obj) SetObject(obj);
            }

            foreach (asd.GeometryObject2D item in MainLayer.Objects.Where(obj => obj is asd.GeometryObject2D))
            {
                item.Color = ScriptObject.IsSuccessBuild ? new asd.Color(0, 255, 0) : new asd.Color(255, 0, 0);
            }
            
            base.OnUpdated();
        }

        void SetObject(asd.Object2D obj)
        {
            if (obj is MapObject mapObject)
            {
                if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= ScalingLayer2D.OriginDisplaySize.X / ScalingLayer2D.OriginDisplaySize.Y)
                    mapObject.Position = ((Mouse.Position - new asd.Vector2DF((asd.Engine.WindowSize.X - ScalingLayer2D.OriginDisplaySize.X * asd.Engine.WindowSize.Y / ScalingLayer2D.OriginDisplaySize.Y) / 2, 0)))
                        * ScalingLayer2D.OriginDisplaySize.Y / asd.Engine.WindowSize.Y;
                else mapObject.Position = (Mouse.Position - new asd.Vector2DF(0, (asd.Engine.WindowSize.Y - ScalingLayer2D.OriginDisplaySize.Y * asd.Engine.WindowSize.X / ScalingLayer2D.OriginDisplaySize.X) / 2))
                        * ScalingLayer2D.OriginDisplaySize.X / asd.Engine.WindowSize.X;
            }
            else
            {
                if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= ScalingLayer2D.OriginDisplaySize.X / ScalingLayer2D.OriginDisplaySize.Y)
                    obj.Position = ((Mouse.Position - new asd.Vector2DF((asd.Engine.WindowSize.X - ScalingLayer2D.OriginDisplaySize.X * asd.Engine.WindowSize.Y / ScalingLayer2D.OriginDisplaySize.Y) / 2, 0)))
                        * ScalingLayer2D.OriginDisplaySize.Y / asd.Engine.WindowSize.Y;
                else obj.Position = (Mouse.Position - new asd.Vector2DF(0, (asd.Engine.WindowSize.Y - ScalingLayer2D.OriginDisplaySize.Y * asd.Engine.WindowSize.X / ScalingLayer2D.OriginDisplaySize.X) / 2))
                        * ScalingLayer2D.OriginDisplaySize.X / asd.Engine.WindowSize.X;
            }

            MainLayer.AddObject(obj);
        }

        public void SaveScript(string path)
        {
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(ScriptObject.Code);
            writer.Close();
        }

        public enum ScriptType
        {
            MapObject,
            EventObject,
            Player,
            BackGround,
        }
    }
}
