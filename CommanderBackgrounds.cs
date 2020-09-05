using System;
using Harmony;
using BattleTech;
using Unity;
using HBS.Collections;
using UnityEngine;
using BattleTech.UI;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Events;

namespace CommanderBackgrounds
{
    public static class Mod
    {
        internal static Settings modSettings;

        public static void Init(string modDir, string settings)
        {

            // read settings
            try
            {
                modSettings = JsonConvert.DeserializeObject<Settings>(settings);
            }
            catch (Exception)
            {
                modSettings = new Settings();
            }
            //            Helpers.PopulateAbilities();
            var harmony = HarmonyInstance.Create("us.wulfbone.CommanderBackgrounds");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
        public class Settings
        { }


        [HarmonyPatch(typeof(SGCharacterCreationCareerBackgroundSelectionPanel), "Done")]
        [HarmonyBefore(new string [] { "de.morphyum.InnerSphereMap" })]
        public static class SGCharacterCreationCareerBackgroundSelectionPanel_Done_Patch
        {
            public static void Prefix(SGCharacterCreationCareerBackgroundSelectionPanel __instance)
            {
                var traverse = Traverse.Create(__instance);
                var results = new List<SimGameEventResult>();
                
                var playerBackground = traverse.Property("playerBackground").GetValue<List<BackgroundDef>>();
                
                foreach (BackgroundDef backgroundDef in playerBackground)
                    {
                        results.Add(backgroundDef.Results);
                    }

                SimGameState.ApplySimGameEventResult(results);
                return;
            }
        }

    }
}