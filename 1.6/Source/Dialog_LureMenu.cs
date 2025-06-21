using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace RimLures
{
    [StaticConstructorOnStartup]

    public class Dialog_LureMenu : Window
    {
        private readonly Texture2D lureIcon = ContentFinder<Texture2D>.Get("Things/Item/Manufactured/SPFluidCanister/SPFluidCanister_a");
        private static List<BiomeDef> allBiomes = DefDatabase<BiomeDef>.AllDefs.ToList();

        private Vector2 scrollPositionLeft;
        private Vector2 scrollPositionRight;

        private float scrollRectAnimalSelectionHeight = 2000f;
        private float scrollRectSelectedAnimalsHeight = 2000f;

        protected float OffsetHeaderY = 72f;

        private Building_Lure building;
        private LurePreset preset;
        int maxX = 1;
        private int x = 0;
        private int y = 0;
        string searchFilter = "";
        private float selectedEntryHeight = 50f;
        const float boxHeight = 75f;
        const float boxWidth = 90f;
        public override Vector2 InitialSize => new Vector2(Mathf.Min(Screen.width - 50, 1300), 720f);

        public Dialog_LureMenu()
        {
            doCloseX = true;
            doCloseButton = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            preset = new LurePreset();
            preset.localBiomes.Add(BiomeDefOf.TemperateForest);
            soundAppear = SoundDefOf.CommsWindow_Open;
            soundClose = SoundDefOf.CommsWindow_Close;
            draggable = true;
            drawShadow = true;
            onlyOneOfTypeAllowed = true;
            resizeable = true;

        }
        public Dialog_LureMenu(Building_Lure _building)
        {
            doCloseX = true;
            doCloseButton = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            building = _building;
            preset = new LurePreset();
            preset.selectedAnimals = new Dictionary<PawnKindDef, int>(building.preset.selectedAnimals);
            preset.localBiomes = new List<BiomeDef>(building.preset.localBiomes);
            preset.ingredients = new List<IngredientCount>(building.preset.ingredients);
            soundAppear = SoundDefOf.CommsWindow_Open;
            soundClose = SoundDefOf.CommsWindow_Close;
            draggable = true;
            drawShadow = true;
            onlyOneOfTypeAllowed = true;
            resizeable = true;
        }
        public override void PostOpen()
        {
            base.PostOpen();
        }
        public override void PreOpen()
        {
            base.PreOpen();

        }
        public override void DoWindowContents(Rect inRect)
        {
            // Left window (Animals)
            Rect AnimalsSearchRect = inRect;
            AnimalsSearchRect.height = 25f;
            AnimalsSearchRect.y += 5f;
            AnimalsSearchRect.x +=14;
            AnimalsSearchRect.width *= 0.15f;
            searchFilter = Widgets.TextArea(AnimalsSearchRect, searchFilter);
            AnimalsSearchRect.x = AnimalsSearchRect.xMax;
            AnimalsSearchRect.width = 26f;
            if (Widgets.ButtonImageFitted(AnimalsSearchRect.ContractedBy(4f), Widgets.CheckboxOffTex))
            {
                searchFilter = "";
            }

            Rect animalsSelectionRect = inRect;
            animalsSelectionRect.y += 25f;
            animalsSelectionRect.height -= 25f;
            animalsSelectionRect.width /= 2f;
            animalsSelectionRect = animalsSelectionRect.ContractedBy(10f);

            Widgets.DrawMenuSection(animalsSelectionRect);
            animalsSelectionRect = animalsSelectionRect.ContractedBy(4f);
            maxX = ((int)animalsSelectionRect.width / 90) - 1;
            /*            Log.Message(LureHelper.animalPrices.Keys.Count + allBiomes.Count);
                        Log.Message(maxX);
                        scrollRectAnimalSelectionHeight = Mathf.Ceil((allAnimalsForReading.Count + allBiomes.Count) / maxX) * 75f;
                        Log.Message(scrollRectAnimalSelectionHeight);*/
            Rect scrollRectAnimalSelection = new Rect(0f, 0f, animalsSelectionRect.width - 16f, scrollRectAnimalSelectionHeight);
            Widgets.BeginScrollView(animalsSelectionRect, ref scrollPositionLeft, scrollRectAnimalSelection);
            GUI.BeginGroup(scrollRectAnimalSelection);
            x = 0;
            y = 0;
            if (!string.IsNullOrEmpty(searchFilter))
            {
                Regex rgx = new Regex(searchFilter, RegexOptions.IgnoreCase);
                List<PawnKindDef> animalsFiltered = LureHelper.animalPrices.Keys.Where(c => rgx.IsMatch(c.defName) || rgx.IsMatch(c.label)).ToList();
                if (animalsFiltered.Any())
                {
                    List<PawnKindDef> localAnimals = preset.localBiomes.SelectMany(c => c.AllWildAnimals).ToList().Where(animalsFiltered.Contains).ToList();
                    if (localAnimals.Any())
                    {
                        Rect localBiomesRect = new Rect(animalsSelectionRect.x, (boxHeight) * (float)y, animalsSelectionRect.width, boxHeight);
                        y++;
                        using (new TextBlock(GameFont.Medium))
                        {
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(localBiomesRect, "RimLureLocalAnimals".Translate());
                            Text.Anchor = TextAnchor.UpperLeft;

                        }
                        DoEntries(localAnimals, animalsSelectionRect, true);
                    }
                    //Log.Message("Filtered animals not in local biomes");
                    //Log.Message(animalsFiltered.Except(localAnimals).Count());
                    if (animalsFiltered.Except(localAnimals).Except(LureHelper.biomelessAnimals.Where(c => RimLure_Settings.whiteListedAnimals.Contains(c.defName))).Any())
                    {
                        Rect exoticBiomesRect = new Rect(animalsSelectionRect.x, (boxHeight) * (float)y, animalsSelectionRect.width, boxHeight);
                        y++;
                        using (new TextBlock(GameFont.Medium))
                        {
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(exoticBiomesRect, "RimLureExoticAnimals".Translate());
                            Text.Anchor = TextAnchor.UpperLeft;

                        }
                        DoEntries(animalsFiltered.Except(localAnimals).Except(LureHelper.biomelessAnimals.Where(c=>RimLure_Settings.whiteListedAnimals.Contains(c.defName))).ToList(), animalsSelectionRect, false);
                    }
                    if (LureHelper.biomelessAnimals.Where(c => RimLure_Settings.whiteListedAnimals.Contains(c.defName) && animalsFiltered.Contains(c)).Any())
                    {
                        Rect exoticBiomesRect = new Rect(animalsSelectionRect.x, (boxHeight) * (float)y, animalsSelectionRect.width, boxHeight);
                        y++;
                        using (new TextBlock(GameFont.Medium))
                        {
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(exoticBiomesRect, "RimLureBiomelessAnimals".Translate());
                            Text.Anchor = TextAnchor.UpperLeft;

                        }
                        DoEntries(LureHelper.biomelessAnimals.Where(c => RimLure_Settings.whiteListedAnimals.Contains(c.defName) && animalsFiltered.Contains(c)).ToList(), animalsSelectionRect, false);
                    }
                }
                else
                {
                    using (new TextBlock(GameFont.Medium))
                    {
                        Rect noAnimalsRect = new Rect(animalsSelectionRect.x, boxHeight * (float)y, animalsSelectionRect.width, boxHeight);
                        y++;
                        using (new TextBlock(GameFont.Medium))
                        {
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(noAnimalsRect, "RimLureNoAnimals".Translate());
                            Text.Anchor = TextAnchor.UpperLeft;

                        }

                    }
                }



            }
            else
            {
                if (preset.localBiomes.Any())
                {
                    Rect localBiomesRect = new Rect(animalsSelectionRect.x, boxHeight * (float)y, animalsSelectionRect.width, boxHeight);
                    y++;
                    using (new TextBlock(GameFont.Medium))
                    {
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(localBiomesRect, "RimLureLocalBiomes".Translate());
                        Text.Anchor = TextAnchor.UpperLeft;

                    }
                    foreach (BiomeDef biome in preset.localBiomes.OrderBy(c => c.defName))
                    {
                        /*                        Log.Message(biome.LabelCap);
                                                Log.Message(biome.AllWildAnimals.Count());*/
                        if (biome.AllWildAnimals.Any())
                        {
                            Rect labelRect = new Rect(animalsSelectionRect.x, boxHeight * (float)y, animalsSelectionRect.width, boxHeight);
                            y++;
                            using (new TextBlock(GameFont.Medium))
                            {
                                Text.Anchor = TextAnchor.MiddleLeft;
                                Widgets.Label(labelRect, biome.LabelCap);
                                Text.Anchor = TextAnchor.UpperLeft;

                            }
                            DoEntries(biome.AllWildAnimals.ToList(), animalsSelectionRect, true);


                        }

                    }
                }

                Rect exoticBiomesRect = new Rect(animalsSelectionRect.x, boxHeight * (float)y, animalsSelectionRect.width, boxHeight);
                y++;
                using (new TextBlock(GameFont.Medium))
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(exoticBiomesRect, "RimLureExoticBiomes".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;

                }
                foreach (BiomeDef biome in allBiomes.Except(preset.localBiomes).OrderBy(c => c.defName))
                {
                    /*                    Log.Message(biome.LabelCap);
                                        Log.Message(biome.AllWildAnimals.Count());*/
                    if (biome.AllWildAnimals.Any())
                    {
                        Rect labelRect = new Rect(animalsSelectionRect.x, boxHeight * (float)y, animalsSelectionRect.width, boxHeight);
                        y++;
                        using (new TextBlock(GameFont.Medium))
                        {
                            Text.Anchor = TextAnchor.MiddleLeft;
                            Widgets.Label(labelRect, biome.LabelCap);
                            Text.Anchor = TextAnchor.UpperLeft;

                        }

                        DoEntries(biome.AllWildAnimals.ToList(), animalsSelectionRect, false);


                    }

                }
                if (LureHelper.biomelessAnimals.Where(c => RimLure_Settings.whiteListedAnimals.Contains(c.defName)).Any())
                {
                    Rect labelRect = new Rect(animalsSelectionRect.x, boxHeight * (float)y, animalsSelectionRect.width, boxHeight);
                    y++;
                    using (new TextBlock(GameFont.Medium))
                    {
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Widgets.Label(labelRect, "RimLureBiomeless".Translate());
                        Text.Anchor = TextAnchor.UpperLeft;

                    }

                    DoEntries(LureHelper.biomelessAnimals.Where(c => RimLure_Settings.whiteListedAnimals.Contains(c.defName)).ToList(), animalsSelectionRect, false);
                }
            }
            scrollRectAnimalSelectionHeight = boxHeight * (float)y;
            GUI.EndGroup();
            Widgets.EndScrollView();

            // Right window (Selected animals and price)
            Rect selectedAnimalWindow = inRect;
            selectedAnimalWindow.x += animalsSelectionRect.xMax + 14f;
            selectedAnimalWindow.y += 25f;
            selectedAnimalWindow.height *= 0.75f;
            selectedAnimalWindow.width /= 2f;
            selectedAnimalWindow = selectedAnimalWindow.ContractedBy(10f);

            Widgets.DrawMenuSection(selectedAnimalWindow);
            selectedAnimalWindow = selectedAnimalWindow.ContractedBy(4f);

            if (preset.selectedAnimals.Count == 0)
            {
                using (new TextBlock(GameFont.Small))
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(selectedAnimalWindow, "RimLureNoAnimalsSelected".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
            else
            {
                Rect scrollRectSelectedAnimals = new Rect(0f, 0f, selectedAnimalWindow.width - 16f, scrollRectSelectedAnimalsHeight);
                Widgets.BeginScrollView(selectedAnimalWindow, ref scrollPositionRight, scrollRectSelectedAnimals);
                GUI.BeginGroup(scrollRectSelectedAnimals);
                DoSelectedAnimals(selectedAnimalWindow);
                scrollRectSelectedAnimalsHeight = selectedEntryHeight * preset.selectedAnimals.Count;

                GUI.EndGroup();
                Widgets.EndScrollView();


            }
            //Bottom right Window (display lure costs)
            Rect lureCostWindow = inRect;
            lureCostWindow.x += selectedAnimalWindow.xMin - 14;
            lureCostWindow.y += selectedAnimalWindow.yMax;
            lureCostWindow.height *= 0.25f;
            lureCostWindow.height -= 14f;
            lureCostWindow.width /= 2f;
            lureCostWindow = lureCostWindow.ContractedBy(10f);

            Widgets.DrawMenuSection(lureCostWindow);
            lureCostWindow = lureCostWindow.ContractedBy(4f);

            Rect lureCostLabel = lureCostWindow;
            lureCostLabel.height /= 2.5f;
            lureCostLabel = lureCostLabel.ContractedBy(15,0);
            using (new TextBlock(GameFont.Medium))
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(lureCostLabel, "RimLureLuresRequired".Translate() + ": " + preset.GetCost());
                Text.Anchor = TextAnchor.UpperLeft;
            }
            Rect acceptRect = lureCostWindow;
            acceptRect.y = lureCostLabel.yMax;
            acceptRect.height /= 1.5f;
            acceptRect.width /= 2;
            acceptRect = acceptRect.ContractedBy(15);
            if(Widgets.ButtonText(acceptRect, "Accept".Translate()))
            {
                SaveSelection();
            }
            Rect cancelRect = lureCostWindow;
            cancelRect.y = lureCostLabel.yMax;
            cancelRect.height /= 1.5f;
            cancelRect.width /= 2;
            cancelRect.x = lureCostWindow.xMax - cancelRect.width;
            cancelRect = cancelRect.ContractedBy(15);
            if (Widgets.ButtonText(cancelRect, "Reset".Translate()))
            {
                CancelSelection();
            }


        }
        private void DoSelectedAnimals(Rect inRect)
        {
            Dictionary<PawnKindDef, int> animals = new Dictionary<PawnKindDef, int>(preset.selectedAnimals);
            if (preset.selectedAnimals.Any())
            {
                int k = 0;
                foreach (KeyValuePair<PawnKindDef, int > animal in animals)
                {
                    Rect animalRect = new Rect(0f, k * selectedEntryHeight, inRect.width, selectedEntryHeight);
                    animalRect.ContractedBy(5f);
                    Rect deleteRect = animalRect;
                    deleteRect.width *= 0.1f;
                    if(Widgets.ButtonImage(deleteRect, TexButton.Delete))
                    {
                        ClearAnimal(animal.Key);
                        break;
                    }
                    Rect IconRect = animalRect;
                    IconRect.width *= 0.1f;
                    IconRect.x = deleteRect.xMax;
                    Widgets.DefIcon(IconRect, animal.Key);
                    Rect labelRect = animalRect;
                    labelRect.width *= 0.45f;
                    labelRect.x = IconRect.xMax;
                    labelRect.ContractedBy(5f, 0);
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(labelRect, animal.Key.LabelCap);
                    Text.Anchor = TextAnchor.UpperLeft;
                    Rect RemoveTen = animalRect;
                    RemoveTen.width *= 0.07f;
                    RemoveTen.x = labelRect.xMax;
                    if(Widgets.ButtonText(RemoveTen,"-10", true))
                    {
                        AddAnimalCount(animal.Key, -10);
                    }
                    Rect RemoveOne = animalRect;
                    RemoveOne.width *= 0.03f;
                    RemoveOne.x = RemoveTen.xMax;
                    RemoveOne.ContractedBy(10, 0);
                    if (Widgets.ButtonText(RemoveOne, "-1", true))
                    {
                        AddAnimalCount(animal.Key, -1);
                    }
                    Rect countRect = animalRect;
                    countRect.width *= 0.08f;
                    countRect.x = RemoveOne.xMax;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(countRect, animal.Value.ToString());
                    Text.Anchor = TextAnchor.UpperLeft;
                    Rect AddOne = animalRect;
                    AddOne.width *= 0.03f;
                    AddOne.x = countRect.xMax;
                    AddOne.ContractedBy(10, 0);
                    if (Widgets.ButtonText(AddOne, "+1", true))
                    {
                        AddAnimalCount(animal.Key, 1);
                    }
                    Rect AddTen = animalRect;
                    AddTen.width *= 0.07f;
                    AddTen.x = AddOne.xMax;
                    if (Widgets.ButtonText(AddTen, "+10", true))
                    {
                        AddAnimalCount(animal.Key, 10);
                    }


                    k++;
                }
            }
            else
            {
                using (new TextBlock(GameFont.Medium))
                {
                    Text.Anchor = TextAnchor.MiddleLeft;
                    Widgets.Label(inRect, "RimLureNoAnimalsSelected".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
        }
        private void DoEntries(List<PawnKindDef> filteredAnimals, Rect inRect, bool localAnimal)
        {
            bool first = true;
            //Log.Message("Checking if animals is not empty");
            if (filteredAnimals.Any())
            {
                foreach (PawnKindDef animal in filteredAnimals.OrderBy(c => c.defName))
                {
                    //Log.Message(animal.defName);
                    if (x > maxX)
                    {
                        x = 0;
                        y++;
                    }
                    //Log.Message("Checked if should be on new line");
                    Rect boxRect = new Rect(boxWidth * (float)x, (boxHeight * (float)y), boxWidth, boxHeight);
                    if (!first)
                    {
                        boxRect.y += 10;
                    }
                    Rect innerBoxRect = boxRect.ContractedBy(10f);
                    //innerBoxRect.y -= 5f;

                    TooltipHandler.TipRegion(boxRect, animal.race.description);
                     //Widgets.DefIcon(innerBoxRect, animal);
                    if (Widgets.ButtonImageFitted(innerBoxRect, Widgets.GetIconFor(animal.race), animal.race.uiIconColor))
                    {
                        AddAnimal(animal);
                    }
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperCenter;
                    Widgets.Label(boxRect, animal.LabelCap);
                    Text.Anchor = TextAnchor.MiddleCenter;

                    Rect priceIconRect = boxRect;
                    priceIconRect.y = boxRect.yMax - 15f;
                    priceIconRect.x += 20f;
                    priceIconRect.height = 17f;
                    priceIconRect.width = 17f;
                    Rect priceRect = boxRect;
                    priceRect.y = boxRect.yMax - 15f;
                    priceRect.height = 17f;
                    priceRect.x = priceIconRect.xMax - 20f;
                    priceRect.width -= 17f;
                    if (localAnimal)
                    {

                        Widgets.DrawTextureFitted(priceIconRect, lureIcon, 1f);
                        Widgets.Label(priceRect, LureHelper.animalPrices[animal].localPrice.ToString());
                    }
                    else
                    {
                        Widgets.DrawTextureFitted(priceIconRect, lureIcon, 1f);
                        Widgets.Label(priceRect, LureHelper.animalPrices[animal].exoticPrice.ToString());
                    }

                    Text.Anchor = TextAnchor.UpperLeft;
                    x++;
                }
                y++;
                x = 0;
                if (y != 0)
                {
                    GUI.color = Color.grey;
                    Widgets.DrawLineHorizontal(boxWidth * (float)x, boxHeight * (float)y + 10f, inRect.width);
                    GUI.color = Color.white;
                }
                first = false;

            }
        }
        public void AddAnimal(PawnKindDef animal)
        {
            preset.AddAnimal(animal);
        }

        public void ClearAnimal(PawnKindDef animal)
        {
            preset.RemoveAnimal(animal);
            
        }
        
        public void AddAnimalCount(PawnKindDef animal, int count)
        {
            preset.AddAnimalCount(animal, count);
        }
        private void SaveSelection()
        {
            if(building != null)
            {
                building.preset = preset;
            }
            Close();
        }
        private void CancelSelection()
        {
            if (building != null)
            {
                preset = building.preset;
            }
            Close();
        }
    }
}