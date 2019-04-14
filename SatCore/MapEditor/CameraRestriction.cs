using SatCore.Attribute;
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
    /// カメラ移動制限
    /// </summary>
    public class CameraRestriction : asd.GeometryObject2D, INotifyPropertyChanged, IMovable, ICopyPasteObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public new asd.RectangleShape Shape
        {
            get
            {
                base.Shape = base.Shape;
                return (asd.RectangleShape)base.Shape;
            }
            set => base.Shape = value;
        }

        public CameraRestriction(SatIO.CameraRestrictionIO boxIO)
        {
            CameraGroup = 1;
            Shape = new asd.RectangleShape();
            Shape.DrawingArea = new asd.RectF(boxIO.Position, boxIO.Size);
            Color = new asd.Color(100, 0, 100, 100);
            DrawingPriority = 4;
        }

        public CameraRestriction()
        {
            CameraGroup = 1;
            Shape = new asd.RectangleShape();
            Color = new asd.Color(100, 0, 100, 100);
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
            base.OnRemoved();
        }

        protected override void OnAdded()
        {
            base.OnAdded();
        }

        public ICopyPasteObject Copy()
        {
            CameraRestriction copy = new CameraRestriction();
            copy.RectSize = RectSize;
            copy.RectPosition = RectPosition + new asd.Vector2DF(50, 50);
            return copy;
        }
    }
}
