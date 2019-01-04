using BaseComponent;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SatCore.MotionEditor
{
    public class Player : Character, SatPlayer.IEffectManeger
    {
        public string WalkLeftEffect => "walk_l";
        public string WalkRightEffect => "walk_r";
        public string DashLeftEffect => "dash_l";
        public string DashRightEffect => "dash_r";
        public string JumpLeftEffect => "jump_l";
        public string JumpRightEffect => "jump_r";
        public string HitLeftEffect => "hit_l";
        public string HitRightEffect => "hit_r";
        public string ChargeLeftEffect => "charge_l";
        public string ChargeRightEffect => "charge_r";
        Dictionary<string, asd.Vector2DF> effectPositions;
        asd.Vector2DF GetEffectPosition(string name)
        {
            if (effectPositions.ContainsKey(name)) return effectPositions[name];
            else return new asd.Vector2DF();
        }

        [TextInput("名前")]
        public string Name { get; set; }

        [NumberInput("歩行速度")]
        public new int WalkSpeed { get => base.WalkSpeed; set => base.WalkSpeed = value; }

        [NumberInput("走行速度")]
        public new int DashSpeed { get => base.DashSpeed; set => base.DashSpeed = value; }

        [NumberInput("跳躍力")]
        public new int JumpPower { get => base.JumpPower; set => base.JumpPower = value; }

        [VectorInput("当たり判定サイズ")]
        public new asd.Vector2DF CollisionSize { get => base.CollisionSize; set => base.CollisionSize = value; }

        [Group("左歩き")]
        public new Motion WalkLeftMotion { get => base.WalkLeftMotion; set => base.WalkLeftMotion = value; }
        [Group("右歩き")]
        public new Motion WalkRightMotion { get => base.WalkRightMotion; set => base.WalkRightMotion = value; }
        [Group("左走り")]
        public new Motion DashLeftMotion { get => base.DashLeftMotion; set => base.DashLeftMotion = value; }
        [Group("右走り")]
        public new Motion DashRightMotion { get => base.DashRightMotion; set => base.DashRightMotion = value; }
        [Group("左上昇")]
        public new Motion UpLeftMotion { get => base.UpLeftMotion; set => base.UpLeftMotion = value; }
        [Group("右上昇")]
        public new Motion UpRightMotion { get => base.UpRightMotion; set => base.UpRightMotion = value; }
        [Group("左下降")]
        public new Motion DownLeftMotion { get => base.DownLeftMotion; set => base.DownLeftMotion = value; }
        [Group("右下降")]
        public new Motion DownRightMotion { get => base.DownRightMotion; set => base.DownRightMotion = value; }
        [Group("左上")]
        public new Motion UpperLeftMotion { get => base.UpperLeftMotion; set => base.UpperLeftMotion = value; }
        [Group("右上")]
        public new Motion UpperRightMotion { get => base.UpperRightMotion; set => base.UpperRightMotion = value; }
        [Group("左下")]
        public new Motion LowerLeftMotion { get => base.LowerLeftMotion; set => base.LowerLeftMotion = value; }
        [Group("右下")]
        public new Motion LowerRightMotion { get => base.LowerRightMotion; set => base.LowerRightMotion = value; }
        [Group("左上走り")]
        public new Motion DashUpperLeftMotion { get => base.DashUpperLeftMotion; set => base.DashUpperLeftMotion = value; }
        [Group("右上走り")]
        public new Motion DashUpperRightMotion { get => base.DashUpperRightMotion; set => base.DashUpperRightMotion = value; }
        [Group("左下走り")]
        public new Motion DashLowerLeftMotion { get => base.DashLowerLeftMotion; set => base.DashLowerLeftMotion = value; }
        [Group("右下走り")]
        public new Motion DashLowerRightMotion { get => base.DashLowerRightMotion; set => base.DashLowerRightMotion = value; }
        [Group("左直立")]
        public new Motion UprightLeftMotion { get => base.UprightLeftMotion; set => base.UprightLeftMotion = value; }
        [Group("右直立")]
        public new Motion UprightRightMotion { get => base.UprightRightMotion; set => base.UprightRightMotion = value; }
        [Group("左ジャンプ")]
        public new Motion JumpLeftMotion { get => base.JumpLeftMotion; set => base.JumpLeftMotion = value; }
        [Group("右ジャンプ")]
        public new Motion JumpRightMotion { get => base.JumpRightMotion; set => base.JumpRightMotion = value; }

        [Group("左歩きエフェクト")]
        public Effect WalkLeftEffectData
        {
            get => _walkLeftEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _walkLeftEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("右歩きエフェクト")]
        public Effect WalkRightEffectData
        {
            get => _walkRightEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _walkRightEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("左走りエフェクト")]
        public Effect DashLeftEffectData
        {
            get => _dashLeftEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _dashLeftEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("右走りエフェクト")]
        public Effect DashRightEffectData
        {
            get => _dashRightEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _dashRightEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("左ジャンプエフェクト")]
        public Effect JumpLeftEffectData
        {
            get => _jumpLeftEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _jumpLeftEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("右ジャンプエフェクト")]
        public Effect JumpRightEffectData
        {
            get => _jumpRightEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _jumpRightEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("左被弾エフェクト")]
        public Effect HitLeftEffectData
        {
            get => _hitLeftEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _hitLeftEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("右被弾エフェクト")]
        public Effect HitRightEffectData
        {
            get => _hitRightEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _hitRightEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("左タメエフェクト")]
        public Effect ChargeLeftEffectData
        {
            get => _chargeLeftEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _chargeLeftEffectData = value;
                OnPropertyChanged();
            }
        }
        [Group("右タメエフェクト")]
        public Effect ChargeRightEffectData
        {
            get => _chargeRightEffectData;
            set
            {
                UndoRedoManager.ChangeProperty(this, value);
                _chargeRightEffectData = value;
                OnPropertyChanged();
            }
        }

        public string HeavyLeftAttackState => "heavy_attack_l";
        public string HeavyRightAttackState => "heavy_attack_r";
        public string LightCommboLeftState => "light_attack_l";
        public string LightCommboRightState => "light_attack_r";

        [Group("左強攻撃")]
        public Attack HeavyLeftAttack { get; set; }
        [Group("右強攻撃")]
        public Attack HeavyRightAttack { get; set; }

        [ListInput("左コンボ", additionButtonEventMethodName: "AddLightAttackLeft")]
        public UndoRedoCollection<Attack> LightCommboLeft { get; set; }
        public void AddLightAttackLeft()
        {
            LightCommboLeft.Add(new Attack("commbo_l_" + (LightCommboLeft.Count + 1).ToString(), this));
        }

        [ListInput("右コンボ", additionButtonEventMethodName: "AddLightAttackRight")]
        public UndoRedoCollection<Attack> LightCommboRight { get; set; }
        public void AddLightAttackRight()
        {
            LightCommboRight.Add(new Attack("commbo_r_" + (LightCommboRight.Count + 1).ToString(), this));
        }

        int actionCounter;
        private Effect _walkLeftEffectData;
        private Effect _walkRightEffectData;
        private Effect _dashLeftEffectData;
        private Effect _dashRightEffectData;
        private Effect _jumpLeftEffectData;
        private Effect _jumpRightEffectData;
        private Effect _hitLeftEffectData;
        private Effect _hitRightEffectData;
        private Effect _chargeLeftEffectData;
        private Effect _chargeRightEffectData;

        public int ActionCounter
        {
            get
            {
                if (Input.GetInputState(Inputs.A) == -1) return actionCounter;
                return 0;
            }
        }

        public Dictionary<string, SatPlayer.Effect> Effects { get; private set; }

        public Player(string playerDataPath, PhysicAltseed.PhysicalWorld world) : base(world)
        {
            LightCommboLeft = new UndoRedoCollection<Attack>();
            LightCommboRight = new UndoRedoCollection<Attack>();
            effectPositions = new Dictionary<string, asd.Vector2DF>();
            Effects = new Dictionary<string, SatPlayer.Effect>();
            LoadPlayerData(SatIO.PlayerIO.GetPlayerIO(playerDataPath));
            actionCounter = 0;
        }

        public Player(PhysicAltseed.PhysicalWorld world) : base(world)
        {
            LightCommboLeft = new UndoRedoCollection<Attack>();
            LightCommboRight = new UndoRedoCollection<Attack>();
            effectPositions = new Dictionary<string, asd.Vector2DF>();
            Effects = new Dictionary<string, SatPlayer.Effect>();
            LoadPlayerData(new SatIO.PlayerIO());
            actionCounter = 0;
        }

        protected override void InputPlayer()
        {
            if (!State.Contains("jump") && IsColligedWithGround)
            {
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) < 1) State = WalkLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) < 1) State = WalkRightState;
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) > 0) State = DashLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) > 0) State = DashRightState;
                if (Input.GetInputState(Inputs.Up) == 1)
                {
                    State = State.Contains("_l") ? JumpLeftState : JumpRightState;
                    SetEffect(State.Contains("_l") ? JumpLeftEffect : JumpRightEffect, GetEffectPosition(State.Contains("_l") ? JumpLeftEffect : JumpRightEffect));
                    return;
                }
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.Left) > 0) State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                if (Input.GetInputState(Inputs.Right) < 1 && Input.GetInputState(Inputs.Left) < 1) State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                if (Input.GetInputState(Inputs.A) > 20) State = "charge" + (State.Contains("_l") ? "_l" : "_r");
            }
            else
            {
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) < 1) State = CollisionShape.Velocity.Y < 0 ? UpperLeftState : LowerLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) < 1) State = CollisionShape.Velocity.Y < 0 ? UpperRightState : LowerRightState;
                if (Input.GetInputState(Inputs.Left) > 0 && Input.GetInputState(Inputs.B) > 0) State = CollisionShape.Velocity.Y < 0 ? DashUpperLeftState : DashLowerLeftState;
                if (Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.B) > 0) State = CollisionShape.Velocity.Y < 0 ? DashUpperRightState : DashLowerRightState;
                if ((Input.GetInputState(Inputs.Right) > 0 && Input.GetInputState(Inputs.Left) > 0)
                    || (Input.GetInputState(Inputs.Right) < 1 && Input.GetInputState(Inputs.Left) < 1))
                {
                    if (CollisionShape.Velocity.Y < 0) State = State.Contains("_l") ? UpLeftState : UpRightState;
                    else State = State.Contains("_l") ? DownLeftState : DownRightState;
                }
            }

            if (Input.GetInputState(Inputs.A) != -1) actionCounter = Input.GetInputState(Inputs.A);
        }

        protected override void Action()
        {
            if (ActionCounter > 20)
            {
                State = State.Contains("_l") ? UprightLeftState : UprightRightState;
                State = State.Contains("_l") ? HeavyLeftAttackState : HeavyRightAttackState;
                IsOneLoop = true;
                SetEffect(State.Contains("_l") ? HeavyLeftAttackState : HeavyRightAttackState, GetEffectPosition(State.Contains("_l") ? HeavyLeftAttackState : HeavyRightAttackState));
                CollisionShape.Velocity = new asd.Vector2DF(0, CollisionShape.Velocity.Y);
            }
            else if (ActionCounter > 0)
            {
                //TODO: コンボ
                CollisionShape.Velocity = new asd.Vector2DF(0, 0);
            }
            else if (State == WalkLeftState || State == UpperLeftState || State == LowerLeftState) CollisionShape.Velocity = new asd.Vector2DF(-WalkSpeed, CollisionShape.Velocity.Y);
            else if (State == WalkRightState || State == UpperRightState || State == LowerRightState) CollisionShape.Velocity = new asd.Vector2DF(WalkSpeed, CollisionShape.Velocity.Y);
            else if (State == DashLeftState || State == DashUpperLeftState || State == DashLowerLeftState) CollisionShape.Velocity = new asd.Vector2DF(-DashSpeed, CollisionShape.Velocity.Y);
            else if (State == DashRightState || State == DashUpperRightState || State == DashLowerRightState) CollisionShape.Velocity = new asd.Vector2DF(DashSpeed, CollisionShape.Velocity.Y);
            else if (State.Contains("jump"))
            {
                CollisionShape.SetImpulse(new asd.Vector2DF(0.0f, -JumpPower), CollisionShape.CenterPosition);
            }
            else CollisionShape.Velocity = new asd.Vector2DF(0, CollisionShape.Velocity.Y);

            if (State == WalkLeftState) SetEffect(WalkLeftEffect, GetEffectPosition(WalkLeftEffect));
            if (State == WalkRightState) SetEffect(WalkRightEffect, GetEffectPosition(WalkRightEffect));
            if (State == DashLeftState) SetEffect(DashLeftEffect, GetEffectPosition(DashLeftEffect));
            if (State == DashRightState) SetEffect(DashRightEffect, GetEffectPosition(DashRightEffect));
        }

        public void LoadPlayerData(SatIO.PlayerIO player)
        {
            LoadMotion(player);

            Name = player.Name;
            WalkLeftEffectData = Effect.GetEffect(player.WalkLeftEffect, WalkLeftEffect, this);
            WalkRightEffectData = Effect.GetEffect(player.WalkRightEffect, WalkRightEffect, this);
            DashLeftEffectData = Effect.GetEffect(player.DashLeftEffect, DashLeftEffect, this);
            DashRightEffectData = Effect.GetEffect(player.DashRightEffect, DashRightEffect, this);
            JumpLeftEffectData = Effect.GetEffect(player.JumpLeftEffect, JumpLeftEffect, this);
            JumpRightEffectData = Effect.GetEffect(player.JumpRightEffect, JumpRightEffect, this);
            HitLeftEffectData = Effect.GetEffect(player.HitLeftEffect, HitLeftEffect, this);
            HitRightEffectData = Effect.GetEffect(player.HitRightEffect, HitRightEffect, this);
            ChargeLeftEffectData = Effect.GetEffect(player.ChargeLeftEffect, ChargeLeftEffect, this);
            ChargeRightEffectData = Effect.GetEffect(player.ChargeRightEffect, ChargeRightEffect, this);
            HeavyLeftAttack = Attack.GetAttack(player.HeavyLeftAttack, HeavyLeftAttackState, this);
            HeavyRightAttack = Attack.GetAttack(player.HeavyRightAttack, HeavyRightAttackState, this);
            foreach (var item in player.LightCommboLeft)
            {
                LightCommboLeft.Add(Attack.GetAttack(item, LightCommboLeftState + "_" + LightCommboLeft.Count, this));
            }
            foreach (var item in player.LightCommboRight)
            {
                LightCommboRight.Add(Attack.GetAttack(item, LightCommboRightState + "_" + LightCommboRight.Count, this));
            }
        }

        public SatIO.PlayerIO ToPlayerIO()
        {
            SatIO.PlayerIO playerIO = new SatIO.PlayerIO();
            playerIO.Name = Name;
            playerIO.WalkSpeed = WalkSpeed;
            playerIO.DashSpeed = DashSpeed;
            playerIO.JumpPower = JumpPower;
            playerIO.WalkLeftMotion = (SatIO.MotionIO.AnimationIO)WalkLeftMotion;
            playerIO.WalkRightMotion = (SatIO.MotionIO.AnimationIO)WalkRightMotion;
            playerIO.DashLeftMotion = (SatIO.MotionIO.AnimationIO)DashLeftMotion;
            playerIO.DashRightMotion = (SatIO.MotionIO.AnimationIO)DashRightMotion;
            playerIO.JumpLeftMotion = (SatIO.MotionIO.AnimationIO)JumpLeftMotion;
            playerIO.JumpRightMotion = (SatIO.MotionIO.AnimationIO)JumpRightMotion;
            playerIO.UpLeftMotion = (SatIO.MotionIO.AnimationIO)UpLeftMotion;
            playerIO.UpRightMotion = (SatIO.MotionIO.AnimationIO)UpRightMotion;
            playerIO.DownLeftMotion = (SatIO.MotionIO.AnimationIO)DownLeftMotion;
            playerIO.DownRightMotion = (SatIO.MotionIO.AnimationIO)DownRightMotion;
            playerIO.UpperLeftMotion = (SatIO.MotionIO.AnimationIO)UpperLeftMotion;
            playerIO.UpperRightMotion = (SatIO.MotionIO.AnimationIO)UpperRightMotion;
            playerIO.LowerLeftMotion = (SatIO.MotionIO.AnimationIO)LowerLeftMotion;
            playerIO.LowerRightMotion = (SatIO.MotionIO.AnimationIO)LowerRightMotion;
            playerIO.DashUpperLeftMotion = (SatIO.MotionIO.AnimationIO)DashUpperLeftMotion;
            playerIO.DashUpperRightMotion = (SatIO.MotionIO.AnimationIO)DashUpperRightMotion;
            playerIO.DashLowerLeftMotion = (SatIO.MotionIO.AnimationIO)DashLowerLeftMotion;
            playerIO.DashLowerRightMotion = (SatIO.MotionIO.AnimationIO)DashLowerRightMotion;
            playerIO.UprightLeftMotion = (SatIO.MotionIO.AnimationIO)UprightLeftMotion;
            playerIO.UprightRightMotion = (SatIO.MotionIO.AnimationIO)UprightRightMotion;

            foreach (var item in LightCommboLeft)
            {
                playerIO.LightCommboLeft.Add((SatIO.PlayerIO.AttackIO)item);
            }
            foreach (var item in LightCommboRight)
            {
                playerIO.LightCommboRight.Add((SatIO.PlayerIO.AttackIO)item);
            }
            playerIO.HeavyLeftAttack = (SatIO.PlayerIO.AttackIO)HeavyLeftAttack;
            playerIO.HeavyRightAttack = (SatIO.PlayerIO.AttackIO)HeavyRightAttack;

            playerIO.WalkLeftEffect = (SatIO.PlayerIO.EffectIO)WalkLeftEffectData;
            playerIO.WalkRightEffect = (SatIO.PlayerIO.EffectIO)WalkRightEffectData;
            playerIO.DashLeftEffect = (SatIO.PlayerIO.EffectIO)DashLeftEffectData;
            playerIO.DashRightEffect = (SatIO.PlayerIO.EffectIO)DashRightEffectData;
            playerIO.JumpLeftEffect = (SatIO.PlayerIO.EffectIO)JumpLeftEffectData;
            playerIO.JumpRightEffect = (SatIO.PlayerIO.EffectIO)JumpRightEffectData;
            playerIO.HitLeftEffect = (SatIO.PlayerIO.EffectIO)HitLeftEffectData;
            playerIO.HitRightEffect = (SatIO.PlayerIO.EffectIO)HitRightEffectData;
            playerIO.ChargeLeftEffect = (SatIO.PlayerIO.EffectIO)ChargeLeftEffectData;
            playerIO.ChargeRightEffect = (SatIO.PlayerIO.EffectIO)ChargeRightEffectData;

            return playerIO;
        }

        public void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval, asd.Vector2DF position)
        {
            LoadEffect(animationGroup, extension, sheets, name, interval);
            effectPositions[name] = position;
        }

        public void LoadEffect(string animationGroup, string extension, int sheets, string name, int interval)
        {
            SatPlayer.Effect effect = new SatPlayer.Effect();
            effect.LoadEffect(animationGroup, extension, sheets, interval);
            Effects[name] = effect;
        }

        public void SetEffect(string name, asd.Vector2DF position)
        {
            if (!Effects.ContainsKey(name)) return;
            SatPlayer.Effect effect = (SatPlayer.Effect)Effects[name].Clone();
            effect.Position = Position + position;
            Layer.AddObject(effect);
        }

        public class Effect : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) =>
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            public string Name { get; private set; }
            Player refPlayer;
            private string _animationGroup;
            private int _sheets;
            private int _interval;
            private asd.Vector2DF _position;

            public Effect(string animationGroup, int sheets, string name, int interval, asd.Vector2DF position, Player player)
            {
                Name = name;
                _position = position;
                _animationGroup = animationGroup;
                _sheets = sheets;
                _interval = interval;
                refPlayer = player;
                refPlayer.LoadEffect(AnimationGroup, "png", Sheets, Name, Interval, Position);
            }

            [FileInput("ファイル", "AnimationFile|*0.png")]
            public string AnimationGroup
            {
                get => _animationGroup;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value.Replace("0.png", ""));
                    _animationGroup = value.Replace("0.png", "");
                    refPlayer.LoadEffect(AnimationGroup, "png", Sheets, Name, Interval, Position);
                    OnPropertyChanged();
                }
            }

            [NumberInput("枚数")]
            public int Sheets
            {
                get => _sheets;
                set
                {
                    if (value > 0)
                    {
                        UndoRedoManager.ChangeProperty(this, value);
                        _sheets = value;
                        refPlayer.LoadEffect(AnimationGroup, "png", Sheets, Name, Interval, Position);
                        OnPropertyChanged();
                    }
                }
            }

            [NumberInput("フレーム数/コマ")]
            public int Interval
            {
                get => _interval;
                set
                {
                    if (value > 0)
                    {
                        UndoRedoManager.ChangeProperty(this, value);
                        _interval = value;
                        refPlayer.LoadEffect(AnimationGroup, "png", Sheets, Name, Interval, Position);
                        OnPropertyChanged();
                    }
                }
            }

            [VectorInput("座標")]
            public asd.Vector2DF Position
            {
                get => _position;
                set
                {
                    UndoRedoManager.ChangeProperty(this, value);
                    _position = value;
                    refPlayer.LoadEffect(AnimationGroup, "png", Sheets, Name, Interval, Position);
                    OnPropertyChanged();
                }
            }

            public static Effect GetEffect(SatIO.PlayerIO.EffectIO effect, string name, Player player)
            {
                return new Effect(effect.AnimationGroup, effect.Sheets, name, effect.Interval, effect.Position, player);
            }

            public static explicit operator SatIO.PlayerIO.EffectIO(Effect effect)
            {
                return new SatIO.PlayerIO.EffectIO(effect.AnimationGroup, effect.Sheets, effect.Interval, effect.Position);
            }
        }

        public class Attack : IListInput
        {
            public Attack(string name, Player player)
            {
                DamageRectPosition = new asd.Vector2DF();
                DamegeRectSize = new asd.Vector2DF();
                Effect = new Effect("", 1, name, 1, new asd.Vector2DF(), player);
                Animation = new Motion("", 1, name, 1, player);
            }

            public Attack(asd.Vector2DF damageRectPosition, asd.Vector2DF damegeRectSize, Effect effect, Motion animation)
            {
                DamageRectPosition = damageRectPosition;
                DamegeRectSize = damegeRectSize;
                Effect = effect;
                Animation = animation;
            }

            [VectorInput("ダメージ領域左上")]
            public asd.Vector2DF DamageRectPosition { get; set; }
            [VectorInput("ダメージ領域サイズ")]
            public asd.Vector2DF DamegeRectSize { get; set; }
            [Group("エフェクト")]
            public Effect Effect { get; set; }
            [Group("モーション")]
            public Character.Motion Animation { get; set; }

            public string Name => Animation.Name;

            public static Attack GetAttack(SatIO.PlayerIO.AttackIO attack, string name, Player player)
            {
                return new Attack(attack.DamageRectPosition, attack.DamegeRectSize, Effect.GetEffect(attack.Effect, name, player), Character.Motion.GetMotion(attack.Animation, name, player));
            }

            public static explicit operator SatIO.PlayerIO.AttackIO(Attack attack)
            {
                return new SatIO.PlayerIO.AttackIO(attack.DamageRectPosition, attack.DamegeRectSize, (SatIO.PlayerIO.EffectIO)attack.Effect, (SatIO.MotionIO.AnimationIO)attack.Animation, attack.Animation.Name);
            }
        }

    }
}
