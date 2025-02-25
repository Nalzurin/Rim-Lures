using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimLures
{
    [DefOf]
    public static class DefOfs
    {
        public static ThingDef SPFluidCanister;
        public static ThingDef SPLauncher;
        public static JobDef LaunchSPFPayload;
        public static HediffDef SPFHediff;
        public static IncidentDef PheromoneAnimalsWalkIn;
        public static EffecterDef SPFRocketSmoke;
        public static ThingDef SPFRocketLeaving;
    }

    public struct AnimalPriceInLures
    {
        public int localPrice;
        public int exoticPrice;
        public AnimalPriceInLures(int _localPrice, int _ExoticPrice)
        {
            localPrice = _localPrice;
            exoticPrice = _ExoticPrice;
        }
    }
    [StaticConstructorOnStartup]
    public static class LureHelper
    {

        public static Dictionary<PawnKindDef, AnimalPriceInLures> animalPrices = [];
        static LureHelper()
        {
            CalculatePrices();
        }
        static void CalculatePrices()
        {
            List<PawnKindDef> allAnimals = DefDatabase<BiomeDef>.AllDefs.Where(c => c.AllWildAnimals != null).SelectMany(c => c.AllWildAnimals).Distinct().ToList();

            foreach (PawnKindDef def in allAnimals)
            {
                if (!animalPrices.ContainsKey(def))
                {
                    if (def.race == null)
                    {
                        Log.Message(def);
                    }

                    float animalPrice = Mathf.Max(def.race.BaseMarketValue / 4/*DefOfs.AnimalLure.BaseMarketValue*/, 1f);
                    animalPrices[def] = new AnimalPriceInLures((int)(animalPrice * RimLure_Settings.costModifierLocalAnimals), (int)(animalPrice * RimLure_Settings.costModifierExoticAnimals));
                }
            }

        }

        public static List<BiomeDef> GetLocalBiomesInRange(Building_Lure lure)
        {
            int startTileID = lure.Map.Tile;
            List<BiomeDef> result = [];
            int radius = RimLure_Settings.searchRadius;
            WorldGrid grid = Find.World.grid;

            HashSet<int> visited = new HashSet<int>();
            HashSet<BiomeDef> uniqueBiomes = new HashSet<BiomeDef>();
            Queue<(int TileID, int Distance)> queue = new Queue<(int TileID, int Distance)>();
            queue.Enqueue((startTileID, 0));
            visited.Add(startTileID);

            while (queue.Count > 0)
            {
                var (currentTileID, distance) = queue.Dequeue();

                // Get the current tile
                Tile currentTile = grid.tiles[currentTileID];
                if (currentTile != null)
                {
                    // Add the BiomeDef if it's not already in the set
                    if (uniqueBiomes.Add(currentTile.biome))
                    {
                        result.Add(currentTile.biome);
                    }
                }

                // If we've reached the max radius, stop processing this branch
                if (distance >= radius) continue;

                // Get the neighbors of the current tile
                List<int> neighbors = [];
                grid.GetTileNeighbors(currentTileID, neighbors);

                foreach (var neighborID in neighbors)
                {
                    if (!visited.Contains(neighborID))
                    {
                        visited.Add(neighborID);
                        queue.Enqueue((neighborID, distance + 1));
                    }
                }
            }
            foreach(BiomeDef def in result)
            {
                Log.Message(def.defName);
            }
            return result;
        }

    }
}
