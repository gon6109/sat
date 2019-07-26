using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatPlayer.Game
{
    /// <summary>
    /// Effekseer用エフェクト
    /// </summary>
    public class EffekseerEffectObject2D : asd.EffectObject2D
    {
        protected override void OnAdded()
        {
            DrawingPriority = 3;
            base.OnAdded();
            Play();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (!IsPlaying)
                Dispose();
        }
    }
}
