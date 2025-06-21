﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;
using Verse;

namespace RimLures
{
    public class WorkGiver_HaulToPheromoneLauncher : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DefOfs.SPLauncher);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsForbidden(pawn))
            {
                return false;
            }
            if (!(t is Building_Lure { State: LureState.WaitingForIngredients } building_Lure))
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return FindIngredients(pawn, building_Lure).Thing != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_Lure { State: LureState.WaitingForIngredients } building_AutoImplanter))
            {
                return null;
            }
            ThingCount thingCount = FindIngredients(pawn, building_AutoImplanter);
            if (thingCount.Thing != null)
            {
                Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
                job.count = Mathf.Min(job.count, thingCount.Count);
                return job;
            }
            return null;
        }

        private ThingCount FindIngredients(Pawn pawn, Building_Lure lure)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
            if (thing == null)
            {
                return default(ThingCount);
            }
            int requiredCountOf = lure.GetRequiredCountOf(thing.def);
            return new ThingCount(thing, Mathf.Min(thing.stackCount, requiredCountOf));
            bool Validator(Thing x)
            {
                if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
                {
                    return false;
                }
                return lure.CanAcceptIngredient(x);
            }
        }
    }
}
