using KTrie;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RimLures
{
    public class LurePreset : IExposable
    {
        public List<BiomeDef> localBiomes;
        public Dictionary<PawnKindDef, int> selectedAnimals;
        public List<IngredientCount> ingredients;
        public int lureCost => GetCost();

        public LurePreset()
        {
            localBiomes = new List<BiomeDef>();
            selectedAnimals = new Dictionary<PawnKindDef, int>();
            ingredients = new List<IngredientCount>();
        }
        public void AddAnimalCount(PawnKindDef animal, int count)
        {
            if (selectedAnimals.ContainsKey(animal))
            {
                selectedAnimals[animal] = Math.Max(selectedAnimals[animal] + count, 1);
            }
            UpdateIngredients();
        }
        public void RemoveAnimal(PawnKindDef animal)
        {
            if (selectedAnimals.ContainsKey(animal))
            {
                selectedAnimals.Remove(animal);
            }
            UpdateIngredients();
        }

        public void AddAnimal(PawnKindDef animal)
        {
            if (selectedAnimals.ContainsKey(animal))
            {
                selectedAnimals[animal] = selectedAnimals[animal] + 1;
            }
            else
            {
                selectedAnimals[animal] = 1;
            }
            UpdateIngredients();
        }
        public int GetCost()
        {
            int result = 0;
            foreach (PawnKindDef animal in selectedAnimals.Keys)
            {
                int count = selectedAnimals[animal];
                int price = 0;
                if(localBiomes.SelectMany(c=>c.AllWildAnimals).Distinct().ToList().Contains(animal))
                {
                    price = LureHelper.animalPrices[animal].localPrice;
                }
                else
                {
                    price = LureHelper.animalPrices[animal].exoticPrice;
                }
                result += count * price; 
            }
            return result;
        }
        public void UpdateIngredients()
        {
            
            ingredients = [new ThingDefCountClass(ThingDefOf.ComponentIndustrial, RimLure_Settings.componentCost).ToIngredientCount(), new ThingDefCountClass(ThingDefOf.Steel, RimLure_Settings.steelCost).ToIngredientCount(), new ThingDefCountClass(DefOfs.SPFluidCanister, lureCost).ToIngredientCount()];
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref localBiomes, "localBiomes", LookMode.Def);
            Scribe_Collections.Look(ref selectedAnimals, "selectedAnimals", LookMode.Def, LookMode.Value);

            Scribe_Collections.Look(ref ingredients, "ingredients", LookMode.Deep);
        }
    }
}
