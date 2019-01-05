using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PhysicAltseed;

namespace SatCore.MapEditor
{
    /// <summary>
    /// 障害物(三角形)
    /// </summary>
    class CollisionTriangle : asd.GeometryObject2D, INotifyPropertyChanged, IMovable, ICopyPasteObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        PhysicalWorld refWorld;

        public new PhysicalTriangleShape Shape
        {
            get
            {
                base.Shape = base.Shape;
                return (PhysicalTriangleShape)base.Shape;
            }
            set => base.Shape = value;
        }

        public CollisionTriangle(SatIO.CollisionTriangleIO triangleIO, PhysicalWorld world)
        {
            CameraGroup = 1;
            refWorld = world;
            Shape = new PhysicalTriangleShape(PhysicalShapeType.Static, world);
            for (int i = 0; i < 3; i++) Shape.SetPointByIndex(triangleIO.vertexes[i], i);
            Color = new asd.Color(0, 0, 255, 100);
            DrawingPriority = 4;
        }

        public CollisionTriangle(PhysicalWorld world)
        {
            CameraGroup = 1;
            refWorld = world;
            Shape = new PhysicalTriangleShape(PhysicalShapeType.Static, world);
            for (int i = 0; i < 3; i++) Shape.SetPointByIndex(new asd.Vector2DF(), i);
            Color = new asd.Color(0, 0, 255, 100);
            DrawingPriority = 4;
        }

        [VectorInput("頂点1")]
        public asd.Vector2DF Vertex1
        {
            get => Shape.GetPointByIndex(0);
            set
            {
                Shape.SetPointByIndex(value, 0);
                OnPropertyChanged();
            }
        }

        [VectorInput("頂点2")]
        public asd.Vector2DF Vertex2
        {
            get => Shape.GetPointByIndex(1);
            set
            {
                Shape.SetPointByIndex(value, 1);
                OnPropertyChanged();
            }
        }

        [VectorInput("頂点3")]
        public asd.Vector2DF Vertex3
        {
            get => Shape.GetPointByIndex(2);
            set
            {
                Shape.SetPointByIndex(value, 2);
                OnPropertyChanged();
            }
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

        public void SetVertexesByIndex(asd.Vector2DF position, int index)
        {
            switch (index)
            {
                case 0:
                    Vertex1 = position;
                    break;
                case 1:
                    Vertex2 = position;
                    break;
                case 2:
                    Vertex3 = position;
                    break;
                default:
                    break;
            }
        }

        public Triangle Vertexes
        {
            get => new Triangle()
            {
                vec1 = Vertex1,
                vec2 = Vertex2,
                vec3 = Vertex3,
            };
            set
            {
                Vertex1 = value.vec1;
                Vertex2 = value.vec2;
                Vertex3 = value.vec3;
            }
        }

        Triangle triangle;

        public void StartMove()
        {
            triangle = Vertexes;
        }

        public void EndMove()
        {
            UndoRedoManager.ChangeProperty(this, Vertexes, triangle, "Vertexes");
        }

        public ICopyPasteObject Copy()
        {
            CollisionTriangle copy = new CollisionTriangle(refWorld);
            copy.Vertex1 = new asd.Vector2DF(50, 50) + Vertex1;
            copy.Vertex2 = new asd.Vector2DF(50, 50) + Vertex2;
            copy.Vertex3 = new asd.Vector2DF(50, 50) + Vertex3;
            return copy;
        }

        public struct Triangle
        {
            public asd.Vector2DF vec1, vec2, vec3;
        }
    }
}
