using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseComponent;

namespace SatPlayer.Game.Object
{
    /// <summary>
    /// メインのカメラ
    /// </summary>
    public class ScrollCamera : asd.CameraObject2D
    {
        private float _homingParameter;

        /// <summary>
        /// 追従に関するパラメータ
        /// </summary>
        public float HomingParameter
        {
            get => _homingParameter;
            set
            {
                if (value < 1 && value > 0) _homingParameter = value;
            }
        }

        /// <summary>
        /// イベント中か
        /// </summary>
        public bool IsEvent { get; set; }

        /// <summary>
        /// 追従するオブジェクト
        /// </summary>
        public asd.Object2D HomingObject { get; set; }

        /// <summary>
        /// マップのサイズ
        /// </summary>
        public asd.Vector2DF MapSize { get; set; }

        /// <summary>
        /// カメラの動作範囲
        /// </summary>
        public asd.RectF MoveRect
        {
            get => new asd.RectF(Src.Size.To2DF() / 2.0f, MapSize - Src.Size.To2DF());
        }

        /// <summary>
        /// マニュアルコントロールキュー
        /// </summary>
        public Queue<Dictionary<Inputs, bool>> MoveCommands { get; private set; }

        /// <summary>
        /// マニュアルコントロール座標指定キュー
        /// </summary>
        public Queue<asd.Vector2DF> WaitStatePoints { get; private set; }

        /// <summary>
        /// マニュアルコントロールターゲット座標
        /// </summary>
        List<asd.Vector2DF> TargetPoint { get; set; }

        /// <summary>
        /// カメラが移動制限領域
        /// </summary>
        List<asd.RectangleShape> Restrictions { get; set; }

        asd.Vector2DF targetPosition;

        /// <summary>
        /// カメラの中心座標
        /// </summary>
        asd.Vector2DF SrcCenter { get => Src.Position.To2DF() + Src.Size.To2DF() / 2.0f; }

        public ScrollCamera(List<SatIO.CameraRestrictionIO> cameraRestrictions)
        {
            CameraGroup = 1;
            HomingParameter = 0.07f;
            MapSize = new asd.Vector2DF();
            MoveCommands = new Queue<Dictionary<Inputs, bool>>();
            WaitStatePoints = new Queue<asd.Vector2DF>();
            TargetPoint = new List<asd.Vector2DF>();
            Restrictions = cameraRestrictions.Select(obj =>
                new asd.RectangleShape()
                {
                    DrawingArea = new asd.RectF(obj.Position, obj.Size),
                }).ToList();
        }

        protected override void OnUpdate()
        {
            if (WaitStatePoints.Count != 0)
            {
                TargetPoint.Add(WaitStatePoints.Dequeue());
                MoveProgrammatic();
            }
            else if (MoveCommands.Count != 0)
            {
                MoveProgrammatic(MoveCommands.Dequeue());
            }
            else if (TargetPoint.Count != 0)
            {
                MoveProgrammatic();
            }
            else if (!IsEvent)
            {
                Holming();
            }

            if (TargetType != MoveType.Input) targetPosition = SrcCenter;

            if ((float)asd.Engine.WindowSize.X / asd.Engine.WindowSize.Y >= ScalingLayer2D.OriginDisplaySize.X / ScalingLayer2D.OriginDisplaySize.Y)
                Dst = new asd.RectI((int)(asd.Engine.WindowSize.X - ScalingLayer2D.OriginDisplaySize.X * asd.Engine.WindowSize.Y / ScalingLayer2D.OriginDisplaySize.Y) / 2, 0,
                    (int)(ScalingLayer2D.OriginDisplaySize.X * asd.Engine.WindowSize.Y / ScalingLayer2D.OriginDisplaySize.Y), asd.Engine.WindowSize.Y);
            else Dst = new asd.RectI(0, (int)(asd.Engine.WindowSize.Y - ScalingLayer2D.OriginDisplaySize.Y * asd.Engine.WindowSize.X / ScalingLayer2D.OriginDisplaySize.X) / 2,
                asd.Engine.WindowSize.X, (int)(ScalingLayer2D.OriginDisplaySize.Y * asd.Engine.WindowSize.X / ScalingLayer2D.OriginDisplaySize.X));

            base.OnUpdate();
        }

        void MoveProgrammatic(Dictionary<Inputs, bool> command)
        {
            if (!GetInputState(command, Inputs.B))
            {
                if (GetInputState(command, Inputs.Up)) targetPosition.Y -= 100f / 60;
                if (GetInputState(command, Inputs.Down)) targetPosition.Y += 100f / 60;
                if (GetInputState(command, Inputs.Left)) targetPosition.X -= 100f / 60;
                if (GetInputState(command, Inputs.Right)) targetPosition.X += 100f / 60;
            }
            else
            {
                if (GetInputState(command, Inputs.Up)) targetPosition.Y -= 200f / 60;
                if (GetInputState(command, Inputs.Down)) targetPosition.Y += 200f / 60;
                if (GetInputState(command, Inputs.Left)) targetPosition.X -= 200f / 60;
                if (GetInputState(command, Inputs.Right)) targetPosition.X += 200f / 60;
            }

            asd.Vector2DF velocity = new asd.Vector2DF();

            velocity.X = GetVelocity((targetPosition - SrcCenter).X);
            velocity.Y = GetVelocity((targetPosition - SrcCenter).Y);

            Src = new asd.RectI(Src.Position + velocity.To2DI(), Src.Size);
            TargetType = MoveType.Input;
        }

        void MoveProgrammatic()
        {
            asd.Vector2DF velocity = new asd.Vector2DF();

            velocity.X = GetVelocity((TargetPoint[0] - SrcCenter).X);
            velocity.Y = GetVelocity((TargetPoint[0] - SrcCenter).Y);

            Src = new asd.RectI(Src.Position + velocity.To2DI(), Src.Size);

            if ((TargetPoint[0] - SrcCenter).Length < 3) TargetPoint.Clear();
            TargetType = MoveType.Point;
        }

        void Holming()
        {
            if (HomingObject == null) return;

            var holmingPosition = HomingObject.Position;

            var restrictionVelocity = new List<asd.Vector2DF>();
            asd.RectangleShape temp = new asd.RectangleShape() { DrawingArea = Src.ToF() };
            foreach (var item in Restrictions.Where(obj => obj.GetIsCollidedWith(temp)))
            {
                restrictionVelocity.Add(ReflectRestriction(item.DrawingArea));
            }

            if (restrictionVelocity.Count != 0)
            {
                asd.Vector2DF vector = new asd.Vector2DF();
                foreach (var item in restrictionVelocity)
                {
                    vector += item;
                }
                vector /= restrictionVelocity.Count;
                holmingPosition += vector;
            }

            asd.Vector2DI pos = new asd.Vector2DI();
            asd.Vector2DF velocity = new asd.Vector2DF();

            if (holmingPosition.X <= MoveRect.X) velocity.X = GetVelocity((Src.Size.To2DF() / 2.0f - SrcCenter).X);
            else if (holmingPosition.X >= MoveRect.Vertexes[1].X) velocity.X = GetVelocity((MoveRect.Vertexes[2] - SrcCenter).X);
            else velocity.X = GetVelocity((holmingPosition - SrcCenter).X);
            if (holmingPosition.Y <= MoveRect.Y) velocity.Y = GetVelocity((Src.Size.To2DF() / 2.0f - SrcCenter).Y);
            else if (holmingPosition.Y >= MoveRect.Vertexes[2].Y) velocity.Y = GetVelocity((MoveRect.Vertexes[2] - SrcCenter).Y);
            else velocity.Y = GetVelocity((holmingPosition - SrcCenter).Y);

            pos = Src.Position + velocity.To2DI();

            if (MapSize.Y < ScalingLayer2D.OriginDisplaySize.Y) pos.Y = (int)(ScalingLayer2D.OriginDisplaySize.Y - MapSize.Y) / 2;
            if (MapSize.X < ScalingLayer2D.OriginDisplaySize.X) pos.X = (int)(ScalingLayer2D.OriginDisplaySize.X - MapSize.X) / 2;

            Src = new asd.RectI(pos, Src.Size);
            TargetType = MoveType.Holming;
        }

        float GetVelocity(float distance)
        {
            if (Math.Abs(distance) < 1.5f) return 0;
            return Math.Abs(distance * HomingParameter) > 1.0f ? distance * HomingParameter : Math.Sign(distance) * 1.0f;
        }

        bool GetInputState(Dictionary<Inputs, bool> moveCommand, Inputs inputs)
        {
            return moveCommand.ContainsKey(inputs) ? moveCommand[inputs] : false;
        }

        public MoveType TargetType { get; private set; }

        asd.Vector2DF ReflectRestriction(asd.RectF restriction)
        {
            List<float> x = new List<float>();
            List<float> y = new List<float>();
            asd.RectF rectF = Src.ToF();
            rectF.X = HomingObject.Position.X - rectF.Size.X / 2;
            rectF.Y = HomingObject.Position.Y - rectF.Size.Y / 2;
            x.Add(rectF.Position.X);
            x.Add(rectF.Position.X + rectF.Size.X);
            x.Add(restriction.Position.X);
            x.Add(restriction.Position.X + restriction.Size.X);
            y.Add(rectF.Position.Y);
            y.Add(rectF.Position.Y + rectF.Size.Y);
            y.Add(restriction.Position.Y);
            y.Add(restriction.Position.Y + restriction.Size.Y);
            x.Sort();
            y.Sort();
            var center = restriction.Position + restriction.Size / 2;
            for (int i = 0; i < 4; i++)
            {
                if ((HomingObject.Position - center).Radian < (restriction.Vertexes[i % 4] - center).Radian)
                {
                    if (i == 0) return new asd.Vector2DF(x[1] - x[2], 0);
                    if (i == 1) return new asd.Vector2DF(0, y[1] - y[2]);
                    if (i == 2) return new asd.Vector2DF(x[2] - x[1], 0);
                    if (i == 3) return new asd.Vector2DF(0, y[2] - y[1]);
                }
                if (i == 3 && (HomingObject.Position - center).Radian >= (restriction.Vertexes[i % 4] - center).Radian) return new asd.Vector2DF(x[2] - x[1], 0);
            }
            return new asd.Vector2DF();
        }

        public enum MoveType
        {
            Holming,
            Input,
            Point,
        }
    }
}
