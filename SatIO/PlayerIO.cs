using BaseComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using static SatIO.MotionIO;

namespace SatIO
{
    [Serializable()]
    public class PlayerIO : MotionIO
    {
        public string Name { get; set; }
        public List<AttackIO> LightCommboLeft { get; set; }
        public List<AttackIO> LightCommboRight { get; set; }
        public AttackIO HeavyLeftAttack { get; set; }
        public AttackIO HeavyRightAttack { get; set; }

        public EffectIO WalkLeftEffect { get; set; }
        public EffectIO WalkRightEffect { get; set; }
        public EffectIO DashLeftEffect { get; set; }
        public EffectIO DashRightEffect { get; set; }
        public EffectIO JumpLeftEffect { get; set; }
        public EffectIO JumpRightEffect { get; set; }
        public EffectIO HitLeftEffect { get; set; }
        public EffectIO HitRightEffect { get; set; }
        public EffectIO ChargeLeftEffect { get; set; }
        public EffectIO ChargeRightEffect { get; set; }

        public PlayerIO()
        {
            LightCommboLeft = new List<AttackIO>();
            LightCommboRight = new List<AttackIO>();
            HeavyLeftAttack = new AttackIO();
            HeavyRightAttack = new AttackIO();
            WalkLeftEffect = new EffectIO();
            WalkRightEffect = new EffectIO();
            DashLeftEffect = new EffectIO();
            DashRightEffect = new EffectIO();
            JumpLeftEffect = new EffectIO();
            JumpRightEffect = new EffectIO();
            HitLeftEffect = new EffectIO();
            HitRightEffect = new EffectIO();
            ChargeLeftEffect = new EffectIO();
            ChargeRightEffect = new EffectIO();
        }

        [Serializable()]
        public class EffectIO : AnimationIO
        {
            public VectorIO Position { get; set; }

            public EffectIO(string animationGroup, int sheets, int interval, VectorIO position) : base(animationGroup, sheets, interval)
            {
                Position = position;
            }

            public EffectIO()
            {
                Position = new VectorIO();
            }
        }

        [Serializable()]
        public class AttackIO
        {
            public AttackIO(VectorIO damageRectPosition, VectorIO damegeRectSize, EffectIO effect, AnimationIO animation, string name)
            {
                DamageRectPosition = damageRectPosition;
                DamegeRectSize = damegeRectSize;
                Effect = effect;
                Animation = animation;
                Name = name;
            }

            public AttackIO()
            {
                DamageRectPosition = new VectorIO();
                DamegeRectSize = new VectorIO();
                Effect = new EffectIO();
                Animation = new AnimationIO();
                Name = "";
            }

            public VectorIO DamageRectPosition { get; set; }
            public VectorIO DamegeRectSize { get; set; }
            public EffectIO Effect { get; set; }
            public AnimationIO Animation { get; set; }
            public string Name { get; set; }
        }

        public void SavePlayerIO(string path)
        {
            using (FileStream playerFile = new FileStream(path, FileMode.Create))
            {
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(playerFile, this);
            }
        }

        public static PlayerIO GetPlayerIO(string path)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            return (PlayerIO)serializer.Deserialize(IO.GetStream(path));
        }
    }
}
