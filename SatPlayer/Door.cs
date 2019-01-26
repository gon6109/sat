using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using SatIO;
using BaseComponent;
using System.Collections.Concurrent;

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
        /// IDを設定・取得
        /// </summary>
        public int ID { get; set; }

        public new asd.Vector2DF Position
        {
            get
            {
                return base.Position;
            }

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
            get
            {
                return base.CenterPosition;
            }

            private set
            {
                base.CenterPosition = value;
            }
        }

        /// <summary>
        /// リソースへのパス
        /// </summary>
        public string TexturePath { get; set; }

        /// <summary>
        /// 遷移先のマップ名
        /// </summary>
        public string MoveToMap { get; set; }

        /// <summary>
        /// 遷移先の指定にDoor IDを使用するか
        /// </summary>
        public bool IsUseMoveToID { get; set; }

        /// <summary>
        /// 遷移先のDoor ID
        /// </summary>
        public int MoveToID { get; set; }

        /// <summary>
        /// 遷移先の座標
        /// </summary>
        public asd.Vector2DF MoveToPosition { get; set; }

        /// <summary>
        /// 解放条件スクリプト
        /// </summary>
        public string KeyScriptPath { get; set; }

        public asd.RectangleShape CollisionShape { get; set; }

        Player refPlayer;
        public Player RefPlayer { get => refPlayer; }

        bool isLeave;
        bool isCome;

        ScriptRunner<bool> keyScriptRunner;

        public bool AcceptLeave()
        {
            if (State == "open" && isLeave)
            {
                isLeave = false;
                refPlayer.IsDrawn = false;
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

        public Door(BlockingCollection<Action> subThreadQueue, string texturePath, string keyScriptPath, Player player)
        {
            CameraGroup = 1;
            refPlayer = player;
            CollisionShape = new asd.RectangleShape();
            TexturePath = texturePath;
            LoadAnimationScript(texturePath);
            State = "close";
            CollisionShape.DrawingArea = new asd.RectF(new asd.Vector2DF(), Texture.Size.To2DF());
            CenterPosition = Texture.Size.To2DF() / 2.0f;
            MoveToID = 0;
            ID = 0;
            IsUseMoveToID = true;
            isLeave = false;
            isCome = false;
            MoveToMap = "hoge";
            DrawingPriority = 1;
            KeyScriptPath = keyScriptPath;
            if (KeyScriptPath != "")
            {
                Script<bool> keyScript = CSharpScript.Create<bool>(IO.GetStream(KeyScriptPath), options: options, globalsType: this.GetType());
                subThreadQueue.TryAdd(() => keyScriptRunner = keyScript.CreateDelegate());
            }
        }

        protected override void OnUpdate()
        {

            if (refPlayer.CollisionShape.GetIsCollidedWith(CollisionShape) && Input.GetInputState(Inputs.A) == 1 && !isCome
                && refPlayer.IsCollidedWithGround && MessageLayer2D.Count == 0)
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
                    refPlayer.IsUpdated = false;
                }
            }

            if (State == "open" && isLeave)
            {
                refPlayer.IsDrawn = false;
            }

            if (State == "open" && isCome)
            {
                refPlayer.IsDrawn = true;
                refPlayer.IsUpdated = true;
                refPlayer.IsDrawn = true;
                isCome = false;
                State = "close";
                State = "closing";
                IsOneLoop = true;
            }
            base.OnUpdate();
        }
    }
}
