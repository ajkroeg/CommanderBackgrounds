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
using System.CodeDom;

namespace CommanderBackgrounds_backup
{

    public static class Mod
    {
        public static bool isCharacterCreation = false;
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
        [HarmonyPatch(typeof(SimGameState), "OnCareerModeStart")]
        public static class SimGameState_OnCareerModeStart_Patch
        {
            public static void Postfix(SimGameState __instance)
            {
                isCharacterCreation = true;
            }
        }

        [HarmonyPatch(typeof(SGCharacterCreationBackgroundSelectionPanel), "Done")]
        public static class SGCharacterCreationBackgroundSelectionPanel_Done_Patch
        {
            public static void Postfix(SGCharacterCreationBackgroundSelectionPanel __instance)
            {
                isCharacterCreation = false;
            }
        }

        [HarmonyPatch(typeof(SimGameState), "ApplySimGameResult")]
        public static class SimGameState_ApplySimGameResult_Patch
        {
            public static bool Prefix(SimGameState __instance, SimGameEventResult result, StatCollection stats, TagSet tags)
            {
                if (isCharacterCreation == false)
                {
                    return true;
                }
                else
                {
                    SimGameState.ApplySimGameEventResult(result, )
                    return false;
                }
            }
        }

    }
}
}
