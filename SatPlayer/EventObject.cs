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
using Microsoft.CodeAnalysis.Scripting;

namespace SatPlayer
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
        public int ID { get; protected set; }

        public string Name => "";

        public bool IsUseName => false;

        Action<IEventObject> IEventObject.Update { get; set; } = obj => { };

        PhysicalShape IActor.CollisionShape => CollisionShape;

        protected EventObject()
        {
            MoveCommands = new Queue<Dictionary<BaseComponent.Inputs, bool>>();
            inputState = new Dictionary<BaseComponent.Inputs, int>();
            foreach (BaseComponent.Inputs item in Enum.GetValues(typeof(BaseComponent.Inputs)))
            {
                inputState[item] = 0;
            }
            GroundShape = new asd.RectangleShape();
        }

        public EventObject(BlockingCollection<Action> subThreadQueue, BlockingCollection<Action> mainThreadQueue, string scriptPath, PhysicalWorld world, string eventObjectPath)
            : base()
        {
            refWorld = world;
            subQueue = subThreadQueue;
            mainQueue = mainThreadQueue;
            Script<object> script;
            subThreadQueue.TryAdd(() =>
            {
                if (scriptPath != "")
                {
                    try
                    {
                        using (var stream = IO.GetStream(scriptPath))
                            script = ScriptOption.ScriptOptions["EventObject"]?.CreateScript<object>(stream.ToString());
                        script.Compile();
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    mainThreadQueue.Add(() =>
                    {
                        var thread = script.RunAsync(this);
                        thread.Wait();
                    });
                }
                mainThreadQueue.TryAdd(() =>
                {
                    try
                    {
                        collisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), AnimationPart.First().Value.Textures.First().Size.To2DF());
                    }
                    catch (Exception e)
                    {
                        ErrorIO.AddError(e);
                    }
                    CenterPosition = collisionShape.DrawingArea.Size / 2;
                    Position = Position;
                });
            });
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

            Update(this);
        }

        public int GetInputState(AlteseedScript.Common.Inputs inputs)
            => inputState[(BaseComponent.Inputs)inputs];

        void IActor.OnUpdate()
        {
            OnUpdate();
        }
    }
}
