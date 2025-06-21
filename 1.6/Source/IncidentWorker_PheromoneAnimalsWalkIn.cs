using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimLures
{
    public class IncidentWorker_PheromoneAnimalsWalkIn : IncidentWorker
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            if (RCellFinder.TryFindRandomPawnEntryCell(out var _, map, CellFinder.EdgeRoadChance_Animal))
            {
                return (parms.pawnKind != null && parms.pawnCount > 0);
            }
            return false;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!RCellFinder.TryFindRandomPawnEntryCell(out var result, map, CellFinder.EdgeRoadChance_Animal))
            {
                return false;
            }

            int num = parms.pawnCount;
            if (num >= 2)
            {
                SpawnAnimal(result, map, parms.pawnKind, Gender.Female);
                SpawnAnimal(result, map, parms.pawnKind, Gender.Male);
                num -= 2;
            }
            for (int i = 0; i < num; i++)
            {
                SpawnAnimal(result, map, parms.pawnKind);
            }
            if(parms.pawnCount == 1)
            {
                SendStandardLetter("RimLurePheromoneAnimalWalkInLabel".Translate(parms.pawnKind.LabelCap), "RimLurePheromoneAnimalWalkIn".Translate(parms.pawnKind.LabelCap), LetterDefOf.PositiveEvent, parms, new TargetInfo(result, map));

            }
            else
            {
                SendStandardLetter("RimLurePheromoneAnimalsWalkInLabel".Translate(parms.pawnKind.GetLabelPlural().CapitalizeFirst()), "RimLurePheromoneAnimalsWalkIn".Translate(parms.pawnKind.GetLabelPlural()), LetterDefOf.PositiveEvent, parms, new TargetInfo(result, map));

            }
            return true;
        }

        private void SpawnAnimal(IntVec3 location, Map map, PawnKindDef pawnKind, Gender? gender = null)
        {
            IntVec3 loc = CellFinder.RandomClosewalkCellNear(location, map, 12);
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, gender));
            pawn.health.AddHediff(DefOfs.SPFHediff);
            GenSpawn.Spawn(pawn, loc, map, Rot4.Random);
        }

    }
}
