using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseComponent;
using SatIO;
using SatPlayer.MapEvent;
using PhysicAltseed;
using SatScript.MapObject;
using AlteseedScript.Common;

namespace SatPlayer
{
    /// <summary>
    /// NPCオブジェクト
    /// </summary>
    public class EventObject : MapObject, IEventObject
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

        public asd.RectangleShape GroundShape { get; set; }

        /// <summary>
        /// 地面と接しているか
        /// </summary>
        public virtual bool IsColligedWithGround { get; private set; }

        public Queue<Dictionary<BaseComponent.Inputs, bool>> MoveCommands { get; private set; }
        Dictionary<BaseComponent.Inputs, int> inputState;

        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; private set; }

        public string Name => "";

        public bool IsUseName => false;

        Action<IEventObject> IEventObject.Update { get; set; } = obj => { };

        public EventObject(BlockingCollection<Action> subThreadQueue, BlockingCollection<Action> mainThreadQueue, string scriptPath, PhysicalWorld world, string eventObjectPath)
            : base(subThreadQueue, mainThreadQueue, scriptPath, world)
        {
            MoveCommands = new Queue<Dictionary<BaseComponent.Inputs, bool>>();
            inputState = new Dictionary<BaseComponent.Inputs, int>();
            foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
            {
                inputState[item] = 0;
            }
            GroundShape = new asd.RectangleShape();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (Layer is MainMapLayer2D)
            {
                IsColligedWithGround = ((MainMapLayer2D)Layer).CollisionShapes.Any(obj => obj.GetIsCollidedWith(GroundShape));
            }

            var currentCommand = MoveCommands.Dequeue();
            if (IsEvent)
            {
                foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
                {
                    if (currentCommand[item] && inputState[item] > -1) inputState[item]++;
                    else if (currentCommand[item] && inputState[item] == -1) inputState[item] = 1;
                    else if (!currentCommand[item] && inputState[item] > 0) inputState[item] = -1;
                    else inputState[item] = 0;
                }
            }

            Update(this);
        }

        public int GetInputState(AlteseedScript.Common.Inputs inputs)
            => inputState[(BaseComponent.Inputs)inputs];
    }
}
