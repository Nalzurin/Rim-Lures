using RimLures;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RimLures
{
    [StaticConstructorOnStartup]
    public class RimLure_Mod : Mod
    {

        public static RimLure_Settings settings;
        public RimLure_Mod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<RimLure_Settings>();
        }


        public override string SettingsCategory()
        {
            return "Smart Animal Lures";
        }


        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoWindowContents(inRect);
        }


    }
}
