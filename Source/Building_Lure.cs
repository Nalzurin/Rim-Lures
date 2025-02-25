using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static UnityEngine.Random;

namespace RimLures
{

    public enum LureState
    {
        Disabled,
        Inactive,
        WaitingForIngredients,
        NeedFuel,
        Active,
        Cooldown
    }

    [StaticConstructorOnStartup]
    public class Building_Lure : Building, IThingHolder
    {
        private int coolDownTicksLeft = 0;
        private bool initLaunch;


        private bool debugDisableNeedForIngredients;


        public static readonly Texture2D CancelLoadingIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");
        public static readonly Texture2D ManagePayloadIcon = ContentFinder<Texture2D>.Get("UI/Designators/ManagePayload");
        public static readonly Texture2D PreparePayloadIcon = ContentFinder<Texture2D>.Get("UI/Designators/PreparePayload");
        public bool PowerOn => this.TryGetComp<CompPowerTrader>().PowerOn;


        public CompRefuelable fuelComp => this.TryGetComp<CompRefuelable>();


        public bool enoughFuel
        {
            get
            {
                return fuelComp.Fuel >= RimLure_Settings.chemCost;
            }
        }

        public LurePreset preset;

        public ThingOwner<Thing> innerContainer;

        public bool AllRequiredIngredientsLoaded
        {

            get
            {
                if (!debugDisableNeedForIngredients)
                {
                    for (int i = 0; i < preset.ingredients.Count(); i++)
                    {
                        if (GetRequiredCountOf(preset.ingredients[i].FixedIngredient) > 0)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

        public LureState State
        {
            get
            {
                if (RimLure_Settings.doCooldownBetweenLaunches && coolDownTicksLeft > 0)
                {
                    return LureState.Cooldown;
                }
                if (!PowerOn)
                {
                    return LureState.Disabled;
                }
                if (!initLaunch)
                {
                    return LureState.Inactive;
                }
                if (!AllRequiredIngredientsLoaded)
                {
                    return LureState.WaitingForIngredients;
                }
                if (!enoughFuel)
                {
                    return LureState.NeedFuel;
                }


                return LureState.Active;
            }
        }

        public Building_Lure()
        {
            innerContainer = new ThingOwner<Thing>(this);
            preset = new LurePreset();
        }
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (preset.ingredients != null)
            {
                for (int i = 0; i < preset.ingredients.Count(); i++)
                {
                    preset.ingredients[i].ResolveReferences();
                }
            }
            preset.localBiomes = LureHelper.GetLocalBiomesInRange(this);

        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
            base.DeSpawn(mode);
        }

        public int GetRequiredCountOf(ThingDef thingDef)
        {
            for (int i = 0; i < preset.ingredients.Count(); i++)
            {
                if (preset.ingredients[i].FixedIngredient == thingDef)
                {
                    int num = innerContainer.TotalStackCountOfDef(preset.ingredients[i].FixedIngredient);
                    return (int)preset.ingredients[i].GetBaseCount() - num;
                }
            }
            return 0;
        }

        public bool CanAcceptIngredient(Thing thing)
        {
            return GetRequiredCountOf(thing.def) > 0;
        }

        public void CancelProcess()
        {
            EjectContents();
        }
        public void EjectContents()
        {

            innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near);


        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }
            if (!selPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                yield return new FloatMenuOption("CannotUseReason".Translate(this) + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }
            if (State == LureState.Active)
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("RimLureLaunchLure".Translate(), delegate
                {
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(DefOfs.LaunchSPFPayload, this), JobTag.MiscWork);
                }), selPawn, this);
            }
        }

        public static bool WasLoadingCancelled(Thing thing)
        {
            if (thing is Building_Lure { initLaunch: false })
            {
                return true;
            }
            return false;
        }

        public override void Tick()
        {
            //Log.Message("Ticking");
            base.Tick();
            LureState state = State;
            Log.Message(AllRequiredIngredientsLoaded);
            if (state == LureState.Cooldown)
            {
                coolDownTicksLeft--;
                if (coolDownTicksLeft <= 0)
                {

                    Messages.Message("RimLureCooldownFinished".Translate(), this, MessageTypeDefOf.PositiveEvent);

                }
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            //Manage Preset
            Command_Action command_ManagePreset = new Command_Action();
            command_ManagePreset.defaultLabel = "RimLureManagePreset".Translate();
            command_ManagePreset.defaultDesc = "RimLureManagePresetDesc".Translate();
            command_ManagePreset.icon = ManagePayloadIcon;
            command_ManagePreset.action = delegate
            {
                Dialog_LureMenu dialog = new Dialog_LureMenu(this);
                Find.WindowStack.Add(dialog);
            };
            command_ManagePreset.activateSound = SoundDefOf.Tick_Tiny;
            if (initLaunch)
            {
                command_ManagePreset.Disable("RimLureLaunchStarted".Translate());
            }
            yield return command_ManagePreset;

            if (preset.selectedAnimals.Any())
            {
                if (!initLaunch)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = "RimLurePrepareLaunch".Translate();
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append("RimLureSelectedAnimals".Translate());
                    string text1 = "";
                    foreach (PawnKindDef animal in preset.selectedAnimals.Keys)
                    {

                        text1 += $"- {animal.LabelCap} x{preset.selectedAnimals[animal]} \n";
                    }
                    stringBuilder.Append("\n");
                    stringBuilder.Append(text1);
                    string text2 = preset.ingredients.Select((IngredientCount i) => i.Summary).ToCommaList(useAnd: true);
                    stringBuilder.Append("RimLurePrepareLaunchDesc".Translate(text2));
                    command_Action.defaultDesc = stringBuilder.ToString();
                    command_Action.icon = PreparePayloadIcon;
                    command_Action.action = delegate
                    {
                        initLaunch = true;
                    };
                    command_Action.activateSound = SoundDefOf.Tick_Tiny;
                    if (State == LureState.Cooldown)
                    {
                        command_Action.Disable("RimLureCooldownOngoing".Translate(coolDownTicksLeft.ToStringTicksToPeriod()));
                    }
                    yield return command_Action;
                }
            }
            if (initLaunch)
            {
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "CommandCancelLoad".Translate();
                command_Action3.defaultDesc = "CommandCancelLoadDesc".Translate();
                command_Action3.icon = CancelLoadingIcon;
                command_Action3.action = delegate
                {

                    EjectContents();
                    initLaunch = false;
                };
                command_Action3.activateSound = SoundDefOf.Designate_Cancel;
                yield return command_Action3;
            }

            if (!DebugSettings.ShowDevGizmos)
            {
                yield break;
            }
            Command_Action command_Action5 = new Command_Action();
            command_Action5.defaultLabel = (debugDisableNeedForIngredients ? "DEV: Enable Ingredients" : "DEV: Disable Ingredients");
            command_Action5.action = delegate
            {
                debugDisableNeedForIngredients = !debugDisableNeedForIngredients;
            };
            yield return command_Action5;
            if (State == LureState.Cooldown)
            {
                Command_Action command_Action6 = new Command_Action();
                command_Action6.defaultLabel = "DEV: Reset cooldown";
                command_Action6.action = delegate
                {
                    coolDownTicksLeft = 0;
                };
            }
        }



        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());
            switch (State)
            {
                case LureState.Inactive:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("RimLureInactive".Translate());
                    break;
                case LureState.Disabled:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("PowerNeeded".Translate());
                    break;
                case LureState.WaitingForIngredients:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("SubcoreScannerWaitingForIngredients".Translate());
                    AppendIngredientsList(stringBuilder);
                    break;
                case LureState.NeedFuel:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("RimLureNeedFuel".Translate());
                    break;
                case LureState.Cooldown:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("RimLureCooldownOngoing".Translate(coolDownTicksLeft.ToStringTicksToPeriod()));
                    break;
                case LureState.Active:
                    stringBuilder.AppendLineIfNotEmpty();
                    stringBuilder.Append("RimLureReadyToLaunch".Translate());
                    break;

            }
            return stringBuilder.ToString();
        }

        public void LaunchPayload()
        {
            foreach (PawnKindDef key in preset.selectedAnimals.Keys)
            {
                IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.Special, target: this.Map);
                incidentParms.pawnKind = key;
                incidentParms.pawnCount = preset.selectedAnimals[key];
                int delay = (int)(Rand.Range(RimLure_Settings.minDaysForAnimalsToArrive, RimLure_Settings.maxDaysForAnimalsToArrive) * 60000);
                Find.Storyteller.incidentQueue.Add(DefOfs.PheromoneAnimalsWalkIn, Find.TickManager.TicksGame + delay, incidentParms, 240000);
            }
            initLaunch = false;
            Messages.Message("RimLurePayLoadLaunched".Translate(), MessageTypeDefOf.PositiveEvent, false);
            innerContainer.ClearAndDestroyContents();
            preset.selectedAnimals.Clear();
            preset.UpdateIngredients();
            fuelComp.ConsumeFuel(RimLure_Settings.chemCost);
            ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
            activeDropPod.Contents = new ActiveDropPodInfo();
            FlyShipLeaving flyShipLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(DefOfs.SPFRocketLeaving, activeDropPod);
            flyShipLeaving.groupID = 0;
            flyShipLeaving.destinationTile = this.Map.Tile + 1;
            flyShipLeaving.worldObjectDef = WorldObjectDefOf.TravelingTransportPods;
            GenSpawn.Spawn(flyShipLeaving, Position, Map);


            if (RimLure_Settings.doCooldownBetweenLaunches)
            {
                coolDownTicksLeft = (int)(RimLure_Settings.cooldownDaysBetweenLaunches * 60000);
            }
        }
        private void AppendIngredientsList(StringBuilder sb)
        {
            for (int i = 0; i < preset.ingredients.Count; i++)
            {
                IngredientCount ingredientCount = preset.ingredients[i];
                int num = innerContainer.TotalStackCountOfDef(ingredientCount.FixedIngredient);
                int num2 = (int)ingredientCount.GetBaseCount();
                sb.AppendInNewLine($" - {ingredientCount.FixedIngredient.LabelCap} {num} / {num2}");
            }
        }

        public override void ExposeData()
        {

            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref initLaunch, "initLaunch", defaultValue: false);
            Scribe_Values.Look(ref coolDownTicksLeft, "coolDownTicksLeft", 0);
            Scribe_Deep.Look(ref preset, "preset");
        }

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }
    }
}


