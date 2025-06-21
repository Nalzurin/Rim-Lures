using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;

namespace RimLures
{
    public class JobDriver_LaunchPayload : JobDriver
    {
        public const int LaunchDelay = 60;

        private Building_Lure Building => (Building_Lure)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => Building.State != LureState.Active || Building.interacter == null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_General.WaitWith(TargetIndex.A, LaunchDelay, useProgressBar: true);
            yield return Toils_General.Do(delegate
            {
                Building.LaunchPayload();
            });
        }
    }
}
