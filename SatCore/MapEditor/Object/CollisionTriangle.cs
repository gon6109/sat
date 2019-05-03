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
    /// 障害物(三角形)
    /// </summary>
    public class CollisionTriangle : asd.GeometryObject2D, INotifyPropertyChanged, IMovable, ICopyPasteObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public new PhysicalTriangleShape Shape
        {
            get
            {
                if (base.Shape != null)
                    base.Shape = base.Shape;
                return (PhysicalTriangleShape)base.Shape;
            }
            set => base.Shape = value;
        }

        public CollisionTriangle()
        {
            CameraGroup = 1;
            Color = new asd.Color(0, 0, 255, 100);
            DrawingPriority = 4;
        }

        [VectorInput("頂点1")]
        public asd.Vector2DF Vertex1
        {
            get => _vertex[0];
            set
            {
                _vertex[0] = value;
                if (Shape != null)
                    Shape.SetPointByIndex(value, 0);
                OnPropertyChanged();
            }
        }

        [VectorInput("頂点2")]
        public asd.Vector2DF Vertex2
        {
            get => _vertex[1];
            set
            {
                _vertex[1] = value;
                if (Shape != null)
                    Shape.SetPointByIndex(value, 1);
                OnPropertyChanged();
            }
        }

        [VectorInput("頂点3")]
        public asd.Vector2DF Vertex3
        {
            get => _vertex[2];
            set
            {
                _vertex[2] = value;
                if (Shape != null)
                    Shape.SetPointByIndex(value, 2);
                OnPropertyChanged();
            }
        }

        asd.Vector2DF[] _vertex = new asd.Vector2DF[3];

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
                Shape = new PhysicalTriangleShape(PhysicalShapeType.Static, map.PhysicalWorld);
                Shape.IsActive = true;
                for (int i = 0; i < 3; i++) Shape.SetPointByIndex(_vertex[i], i);
            }
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
            CollisionTriangle copy = new CollisionTriangle();
            copy.Vertex1 = new asd.Vector2DF(50, 50) + Vertex1;
            copy.Vertex2 = new asd.Vector2DF(50, 50) + Vertex2;
            copy.Vertex3 = new asd.Vector2DF(50, 50) + Vertex3;
            return copy;
        }

        public SatIO.CollisionTriangleIO ToIO()
        {
            SatIO.CollisionTriangleIO collisionTriangleIO = new SatIO.CollisionTriangleIO()
            {
                vertexes = new SatIO.VectorIO[] { Vertex1, Vertex2, Vertex3 }
            };
            return collisionTriangleIO;
        }

        public struct Triangle
        {
            public asd.Vector2DF vec1, vec2, vec3;
        }

        public static CollisionTriangle CreateCollisionTriangle(SatIO.CollisionTriangleIO triangleIO)
        {
            var collisionTriangle = new CollisionTriangle();
            collisionTriangle.CameraGroup = 1;
            for (int i = 0; i < 3; i++) collisionTriangle._vertex[i] = triangleIO.vertexes[i];
            collisionTriangle.Color = new asd.Color(0, 0, 255, 100);
            collisionTriangle.DrawingPriority = 4;
            return collisionTriangle;
        }
    }
}
