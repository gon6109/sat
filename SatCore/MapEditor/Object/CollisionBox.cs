using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PhysicAltseed;
using SatCore.Attribute;

namespace SatCore.MapEditor
{
    /// <summary>
    /// 障害物(四角形)
    /// </summary>
    public class CollisionBox : asd.GeometryObject2D, INotifyPropertyChanged, IMovable, ICopyPasteObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public new PhysicalRectangleShape Shape
        {
            get
            {
                if (base.Shape != null)
                    base.Shape = base.Shape;
                return (PhysicalRectangleShape)base.Shape;
            }
            set => base.Shape = value;
        }

        [VectorInput("左上座標")]
        public asd.Vector2DF RectPosition
        {
            get => _rectPosition;
            set
            {
                _rectPosition = value;
                if (Shape != null)
                    Shape.DrawingArea = new asd.RectF(value, Shape.DrawingArea.Size);
                OnPropertyChanged();
            }
        }

        [VectorInput("サイズ")]
        public asd.Vector2DF RectSize
        {
            get => _rectSize;
            set
            {
                _rectSize = value;
                if (Shape != null)
                    Shape.DrawingArea = new asd.RectF(Shape.DrawingArea.Position, value);
                OnPropertyChanged();
            }
        }

        public asd.RectF Rect
        {
            get => Shape.DrawingArea;
            set
            {
                RectPosition = value.Position;
                RectSize = value.Size;
            }
        }

        public CollisionBox()
        {
            CameraGroup = 1;
            Color = new asd.Color(0, 0, 255, 100);
            DrawingPriority = 4;
        }

        asd.Vector2DF _rectPosition;
        asd.Vector2DF _rectSize;
        asd.RectF rect;

        public void StartMove()
        {
            rect = Rect;
        }

        public void EndMove()
        {
            UndoRedoManager.ChangeProperty(this, Rect, rect, "Rect");
        }

        [Button("消去")]
        public void OnClickRemove()
        {
            UndoRedoManager.ChangeObject2D(Layer, this, false);
            Layer.RemoveObject(this);
        }

        protected override void OnRemoved()
        {
            Shape.IsActive = false;
            base.OnRemoved();
        }

        protected override void OnAdded()
        {
            if (Layer is MapLayer map)
            {
                Shape = new PhysicalRectangleShape(PhysicalShapeType.Static, map.PhysicalWorld);
                Shape.DrawingArea = new asd.RectF(RectPosition, RectSize);
                Shape.IsActive = true;
            }
            base.OnAdded();
        }

        public ICopyPasteObject Copy()
        {
            CollisionBox copy = new CollisionBox();
            copy.RectSize = RectSize;
            copy.RectPosition = RectPosition + new asd.Vector2DF(50, 50);
            return copy;
        }

        public SatIO.CollisionBoxIO ToIO()
        {
            SatIO.CollisionBoxIO collisionBoxIO = new SatIO.CollisionBoxIO()
            {
                Position = RectPosition,
                Size = RectSize
            };
            return collisionBoxIO;
        }

        public static CollisionBox CreateCollsiionBox(SatIO.CollisionBoxIO boxIO)
        {
            var collisionBox = new CollisionBox();
            collisionBox.CameraGroup = 1;
            collisionBox._rectPosition = boxIO.Position;
            collisionBox._rectSize = boxIO.Size;
            collisionBox.Color = new asd.Color(0, 0, 255, 100);
            return collisionBox;
        }
    }
}
