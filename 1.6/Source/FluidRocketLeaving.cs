using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimLures
{
    public class FluidRocketLeaving : FlyShipLeaving
    {
        private Effecter flightEffecter;
        protected override void Tick()
        {
            if (flightEffecter == null)
            {
                flightEffecter = DefOfs.SPFRocketSmoke.Spawn();
                flightEffecter.Trigger(this, TargetInfo.Invalid);
            }
            else
            {
                flightEffecter?.EffectTick(this, TargetInfo.Invalid);
            }
            base.Tick();
        }
    }
}
