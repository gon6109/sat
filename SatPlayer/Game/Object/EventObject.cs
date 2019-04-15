using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseComponent;
using SatIO;
using SatPlayer.Game.Object.MapEvent;
using PhysicAltseed;
using SatScript.MapObject;
using AltseedScript.Common;
using Microsoft.CodeAnalysis.Scripting;

namespace SatPlayer.Game.Object
{
    /// <summary>
    /// NPCオブジェクト
    /// </summary>
    public class EventObject : MapObject, IEventObject, IActor
    {
        ///<summary>
        /// 座標
        /// </summary>
        Vector IEventObject.Position
        {
            get => Position.ToScriptVector();
            set => Position = value.ToAsdVector();
        }

        /// <summary>
        /// イベント時か
        /// </summary>
        public bool IsEvent { get; set; }

        /// <summary>
        /// 接地判定用コリジョン
        /// </summary>
        public asd.RectangleShape GroundCollision { get; set; }

        /// <summary>
        /// 地面と接しているか
        /// </summary>
        public virtual bool IsCollidedWithGround { get; private set; }

        public Queue<Dictionary<BaseComponent.Inputs, bool>> MoveCommands { get; private set; }
        Dictionary<BaseComponent.Inputs, int> inputState;

        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; protected set; }

        public string Name => "";

        public bool IsUseName => false;

        public virtual event Action<IEventObject> Update = delegate { };

        PhysicalShape IActor.CollisionShape => CollisionShape as PhysicalRectangleShape;

        Color IEventObject.Color { get => Color.ToScriptColor(); set => Color = value.ToAsdColor(); }

        protected EventObject()
        {
            MoveCommands = new Queue<Dictionary<BaseComponent.Inputs, bool>>();
            inputState = new Dictionary<BaseComponent.Inputs, int>();
            foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
            {
                inputState[item] = 0;
            }
            GroundCollision = new asd.RectangleShape();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            UpdateGroudShape();

            if (Layer is MapLayer layer)
            {
                IsCollidedWithGround = layer.Obstacles.Any(obj => obj.GetIsCollidedWith(GroundCollision));
            }

            if (IsEvent)
            {
                var currentCommand = MoveCommands.Dequeue();
                foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
                {
                    if (currentCommand[item] && inputState[item] > -1) inputState[item]++;
                    else if (currentCommand[item] && inputState[item] == -1) inputState[item] = 1;
                    else if (!currentCommand[item] && inputState[item] > 0) inputState[item] = -1;
                    else inputState[item] = 0;
                }
            }

            try
            {
                Update(this);
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
                Dispose();
            }
        }

        public int GetInputState(AltseedScript.Common.Inputs inputs)
            => inputState[(BaseComponent.Inputs)inputs];

        void IActor.OnUpdate()
        {
            OnUpdate();
        }

        protected void UpdateGroudShape()
        {
            GroundCollision.DrawingArea = new asd.RectF(CollisionShape.DrawingArea.X + 3, CollisionShape.DrawingArea.Vertexes[2].Y, CollisionShape.DrawingArea.Width - 3, 5);
        }

        public new object Clone()
        {
            EventObject clone = new EventObject();
            clone.sensors = new Dictionary<string, Sensor>(sensors);
            clone.childMapObjectData = new Dictionary<string, MapObject>(childMapObjectData);
            clone.Effects = new Dictionary<string, Effect>(Effects);
            clone.Update = Update;
            clone.State = State;
            clone.Tag = Tag;
            clone.Copy(this);
            clone.MapObjectType = MapObjectType;
            clone.IsAllowRotation = IsAllowRotation;
            try
            {
                clone.collision.DrawingArea = new asd.RectF(new asd.Vector2DF(), clone.AnimationPart.First().Value.Textures.First().Size.To2DF());
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
            }
            clone.CenterPosition = clone.collision.DrawingArea.Size / 2;
            clone.CollisionGroup = CollisionGroup;
            clone.CollisionMask = CollisionMask;
            clone.CollisionCategory = CollisionCategory;
            clone.IsEvent = IsEvent;
            return clone;
        }

        public static async Task<EventObject> CreateEventObjectAsync(EventObjectIO eventObjectIO)
        {
            var eventObject = new EventObject();
            if (eventObjectIO.ScriptPath != "")
            {
                try
                {
                    var stream = await IO.GetStreamAsync(eventObjectIO.ScriptPath);
                    using (stream)
                    {
                        var script = ScriptOption.ScriptOptions["EventObject"]?.CreateScript<object>(stream.ToString());
                        await Task.Run(() => script.Compile());
                        await script.RunAsync(eventObject);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            try
            {
                eventObject.collision.DrawingArea = new asd.RectF(new asd.Vector2DF(), eventObject.AnimationPart.First().Value.Textures.First().Size.To2DF());
            }
            catch (Exception e)
            {
                ErrorIO.AddError(e);
            }
            eventObject.CenterPosition = eventObject.collision.DrawingArea.Size / 2;
            eventObject.Position = eventObjectIO.Position;

            foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
            {
                eventObject.inputState[item] = 0;
            }
            eventObject.UpdateGroudShape();

            return eventObject;
        }
    }
}
