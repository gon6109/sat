using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using SatIO;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaseComponent;
using SatCore.Attribute;

namespace SatCore.MapEditor
{
    /// <summary>
    /// マップ移動オブジェクト
    /// </summary>
    public class Door : MultiAnimationObject2D, INotifyPropertyChanged, IMovable, ICopyPasteObject
    {
        private string _resourcePath;
        private string _moveToMap;
        private string _keyScriptPath;
        private bool _isUseMoveToID;
        private int _moveToID;
        private asd.Vector2DF _moveToPosition;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// IDを設定・取得
        /// </summary>
        [TextOutput("ID")]
        public int ID { get; set; }

        [VectorInput("座標")]
        public new asd.Vector2DF Position
        {
            get
            {
                return base.Position;
            }

            set
            {
                base.Position = value;
                CollisionShape.DrawingArea = new asd.RectF(value - Texture.Size.To2DF() / 2.0f, CollisionShape.DrawingArea.Size);
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 中心座標を取得
        /// </summary>
        public new asd.Vector2DF CenterPosition
        {
            get
            {
                return base.CenterPosition;
            }

            private set
            {
                base.CenterPosition = value;
            }
        }

        /// <summary>
        /// リソースへのパス
        /// </summary>
        [FileInput("アニメーションスクリプトへのパス", "Script File|*.csx|All File|*.*")]
        public string ResourcePath
        {
            get => _resourcePath;
            set
            {
                if (!asd.Engine.File.Exists(value))
                {
                    Texture = TextureManager.LoadTexture(value);
                    CollisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), Texture.Size.To2DF());
                    CenterPosition = Texture.Size.To2DF() / 2.0f;
                    return;
                }
                UndoRedoManager.ChangeProperty(this, value);
                _resourcePath = value;
                AnimationPart.Clear();
                LoadAnimationScript(ResourcePath);
                State = AnimationPart.First().Key;
                CollisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), Texture.Size.To2DF());
                CenterPosition = Texture.Size.To2DF() / 2.0f;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 遷移先のマップ名
        /// </summary>
        [FileInput("遷移先のマップ名", "Binary Map File|*.map|All File|*.*")]
        public string MoveToMap
        {
            get => _moveToMap;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _moveToMap = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 解放条件スクリプト
        /// </summary>
        [FileInput("解放条件スクリプトへのパス", "Script File|*.csx|All File|*.*")]
        public string KeyScriptPath
        {
            get => _keyScriptPath;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _keyScriptPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 遷移先の指定にDoor IDを使用するか
        /// </summary>
        [BoolInput("遷移先の指定にDoor IDを使用するか")]
        public bool IsUseMoveToID
        {
            get => _isUseMoveToID;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _isUseMoveToID = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 遷移先のDoor ID
        /// </summary>
        [NumberInput("遷移先のDoor ID")]
        public int MoveToID
        {
            get => _moveToID;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _moveToID = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 遷移先の座標
        /// </summary>
        [VectorInput("遷移先の座標")]
        public asd.Vector2DF MoveToPosition
        {
            get => _moveToPosition;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _moveToPosition = value;
                OnPropertyChanged();
            }
        }

        public asd.RectangleShape CollisionShape { get; set; }

        public Door()
        {
            try
            {
                CameraGroup = 1;
                CollisionShape = new asd.RectangleShape();
                ResourcePath = "Static/door.csx";
                Color = new asd.Color(255, 255, 255, 200);
                DrawingPriority = 2;
                MoveToID = 0;
                ID = 0;
                IsUseMoveToID = true;
                MoveToMap = "0";
                KeyScriptPath = "";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [Button("消去")]
        public void OnClickRemove()
        {
            UndoRedoManager.ChangeObject2D(Layer, this, false);
            Layer.RemoveObject(this);
        }

        asd.Vector2DF pos;

        public void StartMove()
        {
            pos = Position;
        }

        public void EndMove()
        {
            UndoRedoManager.ChangeProperty(this, Position, pos, "Position");
        }

        public ICopyPasteObject Copy()
        {
            Door copy = new Door();
            copy.Position = Position + new asd.Vector2DF(50, 50);
            copy.ResourcePath = ResourcePath;
            copy.MoveToMap = MoveToMap;
            copy.IsUseMoveToID = IsUseMoveToID;
            copy.MoveToID = MoveToID;
            copy.MoveToPosition = MoveToPosition;
            copy.KeyScriptPath = KeyScriptPath;
            return copy;
        }

        public DoorIO ToIO()
        {
            var result = new DoorIO()
            {
                ID = ID,
                Position = Position,
                ResourcePath = ResourcePath,
                MoveToMap = MoveToMap,
                IsUseMoveToID = IsUseMoveToID,
                MoveToID = MoveToID,
                MoveToPosition = MoveToPosition,
                KeyScriptPath = KeyScriptPath
            };
            return result;
        }

        public static Door CreateDoor(DoorIO door)
        {
            try
            {
                var result = new Door()
                {
                    ID = door.ID,
                    Position = door.Position,
                    ResourcePath = door.ResourcePath,
                    MoveToMap = door.MoveToMap,
                    IsUseMoveToID = door.IsUseMoveToID,
                    MoveToID = door.MoveToID,
                    MoveToPosition = door.MoveToPosition,
                    KeyScriptPath = door.KeyScriptPath
                };
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
