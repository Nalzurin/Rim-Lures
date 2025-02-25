using KTrie;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimLures
{
    public class Dialog_ManageWhitelist : Window
    {
        string searchFilter = "";
        private Vector2 scrollPosition;
        private float scrollRectAnimalSelectionHeight = 2000f;
        private float selectedEntryHeight = 75f;
        public override Vector2 InitialSize => new Vector2(Mathf.Min(Screen.width - 50, 720f), 720f);
        public Dialog_ManageWhitelist()
        {
            doCloseX = true;
            doCloseButton = false;
            forcePause = true;
            absorbInputAroundWindow = true;
            draggable = true;
            drawShadow = true;
            onlyOneOfTypeAllowed = true;
            resizeable = true;
            if(RimLure_Settings.whiteListedAnimals == null)
            {
                RimLure_Settings.whiteListedAnimals = new List<string>();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect AnimalsSearchRect = inRect;
            AnimalsSearchRect.height = 25f;
            AnimalsSearchRect.y += 5f;
            AnimalsSearchRect.x += 14;
            AnimalsSearchRect.width *= 0.5f;
            searchFilter = Widgets.TextArea(AnimalsSearchRect, searchFilter);

            Rect whitelistedRect = inRect;
            whitelistedRect.height = 25f;
            whitelistedRect.y += 5f;
            whitelistedRect.x += inRect.width * 0.70f;
            whitelistedRect.width *= 0.25f;
            using (new TextBlock(GameFont.Medium))
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(whitelistedRect, "RimLureWhitelisted".Translate());
                Text.Anchor = TextAnchor.UpperLeft;

            }
            AnimalsSearchRect.x = AnimalsSearchRect.xMax;
            AnimalsSearchRect.width = 26f;
            if (Widgets.ButtonImageFitted(AnimalsSearchRect.ContractedBy(4f), Widgets.CheckboxOffTex))
            {
                searchFilter = "";
            }
            Rect animalsSelectionRect = inRect;
            animalsSelectionRect.y += 25f;
            animalsSelectionRect.height -= 25f;
            animalsSelectionRect = animalsSelectionRect.ContractedBy(10f);

            Widgets.DrawMenuSection(animalsSelectionRect);
            animalsSelectionRect = animalsSelectionRect.ContractedBy(4f);
            Rect scrollRectAnimalSelection = new Rect(0f, 0f, animalsSelectionRect.width - 16f, scrollRectAnimalSelectionHeight);
            Widgets.BeginScrollView(animalsSelectionRect, ref scrollPosition, scrollRectAnimalSelection);
            GUI.BeginGroup(scrollRectAnimalSelection);

            if (!string.IsNullOrEmpty(searchFilter))
            {
                Regex rgx = new Regex(searchFilter, RegexOptions.IgnoreCase);
                List<PawnKindDef> animalsFiltered = LureHelper.biomelessAnimals.Where(c => rgx.IsMatch(c.defName) || rgx.IsMatch(c.label)).ToList();
                if (animalsFiltered.Any())
                {
                    DoEntries(animalsFiltered, animalsSelectionRect);
                }
                scrollRectAnimalSelectionHeight = selectedEntryHeight * animalsFiltered.Count;

            }
            else
            {
                DoEntries(LureHelper.biomelessAnimals, animalsSelectionRect);
                scrollRectAnimalSelectionHeight = selectedEntryHeight * LureHelper.biomelessAnimals.Count;

            }
            GUI.EndGroup();
            Widgets.EndScrollView();
        }
        public void DoEntries(List<PawnKindDef> defs, Rect inRect)
        {
            int k = 0;
            foreach (PawnKindDef def in defs)
            {
                Rect animalRect = new Rect(0f, k * selectedEntryHeight, inRect.width, selectedEntryHeight);
                animalRect.ContractedBy(20f);
                Rect IconRect = animalRect;
                IconRect.width *= 0.25f;
                Widgets.DefIcon(IconRect, def);
                Rect labelRect = animalRect;
                labelRect.width *= 0.5f;
                labelRect.x = IconRect.xMax;
                labelRect.ContractedBy(5f, 0);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(labelRect, def.LabelCap);
                Rect toggle = animalRect;
                toggle.width *= 0.20f;
                toggle.height *= 0.75f;
                toggle.x = labelRect.xMax;
                if (RimLure_Settings.whiteListedAnimals.Contains(def.defName))
                {
                    if (Widgets.ButtonText(toggle, "Yes".Translate(), true))
                    {
                        RimLure_Settings.whiteListedAnimals.Remove(def.defName);
                    }

                }
                else
                {
                    if (Widgets.ButtonText(toggle, "No".Translate(), true))
                    {
                        RimLure_Settings.whiteListedAnimals.Add(def.defName);

                    }

                }
                Text.Anchor = TextAnchor.UpperLeft;

                k++;
            }
        }

    }
}
