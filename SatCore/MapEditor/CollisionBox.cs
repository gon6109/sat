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

        PhysicalWorld refWorld;

        public new PhysicalRectangleShape Shape
        {
            get
            {
                base.Shape = base.Shape;
                return (PhysicalRectangleShape)base.Shape;
            }
            set => base.Shape = value;
        }

        public CollisionBox(SatIO.CollisionBoxIO boxIO, PhysicalWorld world)
        {
            CameraGroup = 1;
            refWorld = world;
            Shape = new PhysicalRectangleShape(PhysicalShapeType.Static, world);
            Shape.DrawingArea = new asd.RectF(boxIO.Position, boxIO.Size);
            Color = new asd.Color(0, 0, 255, 100);
            DrawingPriority = 4;
        }

        public CollisionBox(PhysicalWorld world)
        {
            CameraGroup = 1;
            refWorld = world;
            Shape = new PhysicalRectangleShape(PhysicalShapeType.Static, world);
            Color = new asd.Color(0, 0, 255, 100);
            DrawingPriority = 4;
        }

        [VectorInput("左上座標")]
        public asd.Vector2DF RectPosition
        {
            get => Shape.DrawingArea.Position;
            set
            {
                Shape.DrawingArea = new asd.RectF(value, Shape.DrawingArea.Size);
                OnPropertyChanged();
            }
        }

        [VectorInput("サイズ")]
        public asd.Vector2DF RectSize
        {
            get => Shape.DrawingArea.Size;
            set
            {
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
            Shape.IsActive = true;
            base.OnAdded();
        }

        public ICopyPasteObject Copy()
        {
            CollisionBox copy = new CollisionBox(refWorld);
            copy.RectSize = RectSize;
            copy.RectPosition = RectPosition + new asd.Vector2DF(50, 50);
            return copy;
        }
    }
}
