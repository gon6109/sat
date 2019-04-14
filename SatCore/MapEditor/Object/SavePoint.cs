using BaseComponent;
using SatCore.Attribute;
using SatIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MapEditor
{
    /// <summary>
    /// セーブポイント
    /// </summary>
    public class SavePoint : asd.TextureObject2D, IMovable, ICopyPasteObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private asd.RectangleShape rect;

        [TextOutput("ID")]
        public int ID { get; set; }

        [VectorInput("座標")]
        public new asd.Vector2DF Position
        {
            get => base.Position;
            set
            {
                base.Position = value;
                OnPropertyChanged();
            }
        }

        public asd.RectangleShape Shape
        {
            get
            {
                rect.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
                return rect;
            }
        }

        public SavePoint()
        {
            CameraGroup = 1;
            rect = new asd.RectangleShape();
            DrawingPriority = 2;
            Texture = TextureManager.LoadTexture("save_point.png");
            CenterPosition = Texture.Size.To2DF() / 2;
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
            SavePoint savePoint = new SavePoint();
            savePoint.Position = Position + new asd.Vector2DF(50, 50);
            return savePoint;
        }

        public SavePointIO ToIO()
        {
            var savePointIO = new SavePointIO()
            {
                ID = ID,
                Position = Position
            };
            return savePointIO;
        }

        public static SavePoint CreateSavePoint(SavePointIO savePointIO)
        {
            var savePoint = new SavePoint();
            savePoint.CameraGroup = 1;
            savePoint.rect = new asd.RectangleShape();
            savePoint.Position = savePointIO.Position;
            savePoint.ID = savePointIO.ID;
            savePoint.DrawingPriority = 2;
            savePoint.Texture = TextureManager.LoadTexture("save_point.png");
            savePoint.CenterPosition = savePoint.Texture.Size.To2DF() / 2;
            return savePoint;
        }
    }
}
