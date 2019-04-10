using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SatIO;
using BaseComponent;
using System.Collections.Concurrent;
using SatPlayer.Game.Object;
using System.Threading.Tasks;

namespace SatPlayer
{
    /// <summary>
    /// マップ移動オブジェクト
    /// </summary>
    public class Door : MultiAnimationObject2D
    {
        static ScriptOptions options = ScriptOptions.Default.WithImports("SatPlayer", "PhysicAltseed", "System")
                                                         .WithReferences(System.Reflection.Assembly.GetAssembly(typeof(MapObject))
                                                                         , System.Reflection.Assembly.GetAssembly(typeof(asd.Vector2DF)));
        /// <summary>
        /// IDを取得
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// 座標
        /// </summary>
        public new asd.Vector2DF Position
        {
            get => base.Position;

            set
            {
                base.Position = value;
                CollisionShape.DrawingArea = new asd.RectF(value - Texture.Size.To2DF() / 2.0f, CollisionShape.DrawingArea.Size);
            }
        }

        /// <summary>
        /// 中心座標を取得
        /// </summary>
        public new asd.Vector2DF CenterPosition
        {
            get => base.CenterPosition;
            private set => base.CenterPosition = value;
        }

        /// <summary>
        /// リソースへのパス
        /// </summary>
        public string ResourcePath { get; private set; }

        /// <summary>
        /// 遷移先のマップ名
        /// </summary>
        public string MoveToMap { get; }

        /// <summary>
        /// 遷移先の指定にDoor IDを使用するか
        /// </summary>
        public bool IsUseMoveToID { get; }

        /// <summary>
        /// 遷移先のDoor ID
        /// </summary>
        public int MoveToID { get; }

        /// <summary>
        /// 遷移先の座標
        /// </summary>
        public asd.Vector2DF MoveToPosition { get; }

        /// <summary>
        /// 解放条件スクリプト
        /// </summary>
        public string KeyScriptPath { get; private set; }

        public asd.RectangleShape CollisionShape { get; }

        bool isLeave;
        bool isCome;

        ScriptRunner<bool> keyScriptRunner;

        public bool AcceptLeave()
        {
            if (State == "open" && isLeave)
            {
                isLeave = false;
                RefPlayer.IsDrawn = false;
                return true;
            }
            return false;
        }

        public void AcceptCome()
        {
            State = "open";
            State = "opening";
            IsOneLoop = true;
            isCome = true;
        }

        public Door()
        {
            CameraGroup = 1;
            CollisionShape = new asd.RectangleShape();
            MoveToID = 0;
            ID = 0;
            IsUseMoveToID = true;
            isLeave = false;
            isCome = false;
            MoveToMap = "hoge";
            DrawingPriority = 1;
        }

        protected override void OnUpdate()
        {

            if (RefPlayer.CollisionShape.GetIsCollidedWith(CollisionShape) && Input.GetInputState(Inputs.A) == 1 && !isCome
                && RefPlayer.IsCollidedWithGround && MessageLayer2D.Count == 0)
            {
                bool temp = true;
                if (KeyScriptPath != "")
                {
                    var thread = keyScriptRunner(this);
                    thread.Wait();
                    temp = thread.Result;
                }
                if (temp)
                {
                    State = "open";
                    State = "opening";
                    IsOneLoop = true;
                    isLeave = true;
                    RefPlayer.IsUpdated = false;
                }
            }

            if (State == "open" && isLeave)
            {
                RefPlayer.IsDrawn = false;
            }

            if (State == "open" && isCome)
            {
                RefPlayer.IsDrawn = true;
                RefPlayer.IsUpdated = true;
                RefPlayer.IsDrawn = true;
                isCome = false;
                State = "close";
                State = "closing";
                IsOneLoop = true;
            }
            base.OnUpdate();
        }

        public static async Task<Door> CreateDoorAsync(DoorIO doorIO)
        {
            var door = new Door();

            door.ResourcePath = doorIO.ResourcePath;
            await door.LoadAnimationScriptAsync(door.ResourcePath);
            door.State = "close";
            door.CollisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), door.Texture.Size.To2DF());
            door.CenterPosition = door.Texture.Size.To2DF() / 2.0f;
            door.KeyScriptPath = doorIO.KeyScriptPath;
            if (door.KeyScriptPath != "")
            {
                try
                {
                    var stream = await IO.GetStreamAsync(door.KeyScriptPath);
                    using (stream)
                    {
                        Script<bool> keyScript = CSharpScript.Create<bool>(stream, options: options, globalsType: door.GetType());
                        await Task.Run(() => door.keyScriptRunner = keyScript.CreateDelegate());
                    }
                }
                catch (Exception e)
                {
                    ErrorIO.AddError(e);
                }
            }

            return door;
        }
    }
}
