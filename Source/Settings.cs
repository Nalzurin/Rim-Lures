using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace RimLures
{
    public class RimLure_Settings : ModSettings
    {
        public static float costModifierLocalAnimals = 0.75f;
        public static float costModifierExoticAnimals = 1.75f;
        public static int searchRadius = 8;

        public static int steelCost = 50;
        public static int componentCost = 2;
        public static int chemCost = 25;

        public static bool doCooldownBetweenLaunches = false;
        public static float cooldownDaysBetweenLaunches = 0.5f;

        public static float minDaysForAnimalsToArrive = 1;
        public static float maxDaysForAnimalsToArrive = 5;

        private string _steelCost;
        private string _componentCost;
        private string _chemCost;
        private string _minDaysForAnimalsToArrive;
        private string _maxDaysForAnimalsToArrive;
        private string _cooldownDaysBetweenLaunches;
        private Vector2 _scrollPosition;
        public static List<string> whiteListedAnimals = [];
        public override void ExposeData()
        {

            Scribe_Values.Look(ref costModifierLocalAnimals, "costModifierLocalAnimals", defaultValue: 0.75f, forceSave: true);
            Scribe_Values.Look(ref costModifierExoticAnimals, "costModifierExoticAnimals", defaultValue: 1.75f, forceSave: true);
            Scribe_Values.Look(ref searchRadius, "searchRadius", defaultValue: 8, forceSave: true);

            Scribe_Values.Look(ref steelCost, "steelCost", defaultValue: 50, forceSave: true);
            Scribe_Values.Look(ref componentCost, "componentCost", defaultValue:2, forceSave: true);
            Scribe_Values.Look(ref chemCost, "chemCost", defaultValue: 25, forceSave: true);

            Scribe_Values.Look(ref doCooldownBetweenLaunches, "doCooldownBetweenLaunches", defaultValue: false, forceSave: true);
            Scribe_Values.Look(ref cooldownDaysBetweenLaunches, "cooldownDaysBetweenLaunches", defaultValue: 0.5f, forceSave: true);

            Scribe_Values.Look(ref minDaysForAnimalsToArrive, "minDaysForAnimalsToArrive", defaultValue: 1, forceSave: true);
            Scribe_Values.Look(ref maxDaysForAnimalsToArrive, "maxDaysForAnimalsToArrive", defaultValue: 5, forceSave: true);
            Scribe_Collections.Look(ref whiteListedAnimals, "whiteListedAnimals", LookMode.Value);
            base.ExposeData();
        }

        public void DoWindowContents(Rect inRect)
        {
            Rect rect2 = new Rect(inRect);
            rect2.height = 750f;
            Rect rect3 = rect2;
            Widgets.AdjustRectsForScrollView(inRect,ref rect2, ref rect3);
            Widgets.BeginScrollView(inRect, ref _scrollPosition, rect3, showScrollbars: false);
           
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(rect3);
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("RimLureSettingsSearch".Translate());

            }
            listing_Standard.Gap();

            listing_Standard.Label("SearchRadius".Translate() + ": " + searchRadius.ToString() + " " + "SearchRadiusTiles".Translate() + " "+ "SearchRadiusParenthesis".Translate());
            searchRadius = (int)listing_Standard.Slider(searchRadius,0, 20);
            listing_Standard.Gap(5f);
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("RimLureSettingsCosts".Translate());

            }
            listing_Standard.Gap();
            listing_Standard.Label("CostModifierLocalAnimals".Translate() + ": " + (costModifierLocalAnimals * 100f).ToString() + "%");
            costModifierLocalAnimals = (float)Math.Round((double)listing_Standard.Slider(costModifierLocalAnimals, 0.01f, 5f), 2);
            listing_Standard.Label("CostModifierExoticAnimals".Translate() + ": " + (costModifierExoticAnimals * 100f).ToString() + "%");
            costModifierExoticAnimals = (float)Math.Round((double)listing_Standard.Slider(costModifierExoticAnimals, 0.01f, 5f), 2);

            listing_Standard.Label("SteelCost".Translate());
            listing_Standard.TextFieldNumeric(ref steelCost, ref _steelCost, 0, 250);

            listing_Standard.Label("ComponentCost".Translate());
            listing_Standard.TextFieldNumeric(ref componentCost, ref _componentCost, 0, 250);

            listing_Standard.Label("ChemCost".Translate());
            listing_Standard.TextFieldNumeric(ref chemCost, ref _chemCost, 0, 100);

            listing_Standard.Gap(5f);
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("RimLureSettingsAnimalArrival".Translate());

            }
            listing_Standard.Gap();

            listing_Standard.Label("MinDaysForAnimalsToArrive".Translate());
            listing_Standard.TextFieldNumeric(ref minDaysForAnimalsToArrive, ref _minDaysForAnimalsToArrive, 0, 10f);

            listing_Standard.Label("MaxDaysForAnimalsToArrive".Translate());
            listing_Standard.TextFieldNumeric(ref maxDaysForAnimalsToArrive, ref _maxDaysForAnimalsToArrive, 0, 10f);

            listing_Standard.Gap(5f);
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("RimLureSettingsCooldown".Translate());

            }
            listing_Standard.Gap();

            listing_Standard.CheckboxLabeled("RimLureDoCoolDownBetweenLaunches".Translate(), ref doCooldownBetweenLaunches);
            if (doCooldownBetweenLaunches)
            {
                listing_Standard.Label("CooldownDaysBetweenLaunches".Translate());
                listing_Standard.TextFieldNumeric(ref cooldownDaysBetweenLaunches, ref _cooldownDaysBetweenLaunches, 0, 10);

            }
            listing_Standard.Gap(5f);
            using (new TextBlock(GameFont.Medium))
            {
                listing_Standard.Label("RimLureSettingsBiomelessWhitelist".Translate());

            }
            listing_Standard.Gap();
            if (listing_Standard.ButtonText("RimLureSettingsBiomelessWhitelistManage".Translate()))
            {
                Find.WindowStack.Add(new Dialog_ManageWhitelist());
            }

            listing_Standard.End();
            Widgets.EndScrollView();
        }

    }
}
