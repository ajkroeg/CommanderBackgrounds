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
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

namespace CommanderBackgrounds
{
    public class CustomStartingResult
    {
        [JsonIgnore]
        public List<SimGameEventResult> simResult = new List<SimGameEventResult>();
        public List<JObject> simResultJOs = new List<JObject>();
    }
    public static class Mod
    {
        public static List<SimGameEventResult> StartingResults = new List<SimGameEventResult>();
        internal static Settings modSettings;
        internal static Logger modLog;
        public static void Init(string modDir, string settings)
        {
            modLog = new Logger(modDir, "backgrounds", true);
            try
            { 
                modSettings = JsonConvert.DeserializeObject<Settings>(settings);
            }
            catch (Exception)
            {
                modSettings = new Settings();
            }
            var harmony = HarmonyInstance.Create("us.wulfbone.CommanderBackgrounds");
            Mod.modLog.LogMessage($"Initializing Commander Backgrounds - Version {typeof(Settings).Assembly.GetName().Version}");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Initialize();
        }

        public class Settings
        {
            public bool enableLogging = true;
            public CustomStartingResult CustomStartingResults = new CustomStartingResult();
        }

        public static void Initialize()
        {

            foreach (var jObject in Mod.modSettings.CustomStartingResults.simResultJOs)
            {
                var simResult = new SimGameEventResult();
                simResult.Scope = jObject["Scope"].ToObject<EventScope>();
                simResult.Requirements = jObject["Requirements"].ToObject<RequirementDef>();
                simResult.AddedTags = new TagSet();
                simResult.AddedTags.FromJSON(jObject["AddedTags"].ToString());
                simResult.RemovedTags = new TagSet();
                simResult.RemovedTags.FromJSON(jObject["RemovedTags"].ToString());
                simResult.Stats = jObject["Stats"].ToObject<SimGameStat[]>();
                simResult.Actions = jObject["Actions"].ToObject<SimGameResultAction[]>();
                simResult.ForceEvents = jObject["ForceEvents"].ToObject<SimGameForcedEvent[]>();
                simResult.TemporaryResult = jObject["TemporaryResult"].ToObject<bool>();
                simResult.ResultDuration = jObject["ResultDuration"].ToObject<int>();
                Mod.modSettings.CustomStartingResults.simResult.Add(simResult);
                Mod.modLog.LogMessage($"Processed a starting simgame result.");
            }
        }

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

                foreach (var result in Mod.modSettings.CustomStartingResults.simResult)
                {
                    results.Add(result);
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
