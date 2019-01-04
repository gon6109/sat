using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer
{
    public interface IDamageControler
    {
        int HP { get; set; }
        bool IsReceiveDamage { get; set; }
        Queue<DamageRect> DamageRequests { get; }
        Queue<DirectDamage> DirectDamageRequests { get; }
        DamageRect.OwnerType OwnerType { get; }
        asd.Shape CollisionShape { get; }
    }

    public class DamageRect : asd.RectangleShape
    {
        public OwnerType Owner { get; private set; }
        public int Frame { get; set; }
        public bool Sastainable { get; private set; }
        public int Damage { get; private set; }
        public int KnockBack { get; private set; }

        public DamageRect(OwnerType owner, asd.RectF rect, int damage ,int frame, bool sastainable,int knockBack)
        {
            Damage = damage;
            Owner = owner;
            DrawingArea = rect;
            Frame = frame;
            Sastainable = sastainable;
            KnockBack = knockBack;
        }

        public enum OwnerType
        {
            Player,
            Enemy
        }
    }

    public class DirectDamage
    {
        public IDamageControler RecieveTo { get; private set; }
        public int Damage { get; private set; }
        public int KnockBack { get; private set; }

        public DirectDamage(IDamageControler recieveTo, int damage,int knockBack)
        {
            RecieveTo = recieveTo;
            Damage = damage;
            KnockBack = knockBack;
        }
    }
}
