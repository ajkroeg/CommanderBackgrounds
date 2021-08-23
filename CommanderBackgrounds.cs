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
        public static class SGCharacterCreationCareerBackgroundSelectionPanel_Done_Patch
        {
            [HarmonyBefore(new string[] { "de.morphyum.InnerSphereMap" })]
            [HarmonyPriority(1000)]
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

        [HarmonyPatch(typeof(SimGameState), "DismissPilot", new Type[] {typeof(string)})]
        public static class SimGameState_DismissPilot_Patch
        {
            public static bool Prefix(string pilotID, SimGameState __instance)
            {
                if (pilotID == "*")
                {
                    foreach (Pilot pilot in __instance.PilotRoster.ToList())
                    {
                        if (!pilot.IsPlayerCharacter)
                        {
                            __instance.DismissPilot(pilot);
                        }
                    }

                    return false;
                }

                return true;
            }
        }
    }
}
