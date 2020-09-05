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


        [HarmonyPatch(typeof(SGCharacterCreationWidget), "CreatePilot")]

        public static class SGCharacterCreationWidget_CreatePilot_Patch
        {
            public static bool Prefix(SGCharacterCreationWidget __instance, ref Pilot __result, ref Pilot ___createdPilot)
            {

                if (!Traverse.Create(__instance).Method("CanCreate").GetValue<bool>())
                    {
                    Debug.LogWarning("Can't create profile :(");
                    __result = null;
                    return false;
                    }

                StatCollection stats = new StatCollection();
                TagSet tagSet = new TagSet();
                var sim = UnityGameInstance.BattleTechGame.Simulation;
                var backgroundSelection = Traverse.Create(__instance).Property("backgroundSelection").GetValue<SGCharacterCreationBackgroundSelectionPanel>();

                foreach (BackgroundDef backgroundDef in backgroundSelection.playerBackground)
                {
                   // SimGameState.ApplySimGameResult(backgroundDef.Results, stats, tagSet);

                    if (backgroundDef.Results.Stats != null && backgroundDef.Results.Scope== EventScope.Commander)
                    {
                        for (int i = 0; i < backgroundDef.Results.Stats.Length; i++)
                        {
                            SimGameStat simGameStat = backgroundDef.Results.Stats[i];
                            float num = simGameStat.ToSingle();
                            if (stats.ContainsStatistic(simGameStat.name))
                            {
                                float num2 = stats.GetStatistic(simGameStat.name).Value<float>();
                                if (simGameStat.set)
                                {
                                    num2 = num;
                                }
                                else
                                {
                                    num2 += num;
                                }
                                stats.Set<float>(simGameStat.name, num2);
                            }
                            else
                            {
                                stats.AddStatistic<float>(simGameStat.name, num);
                            }
                        }
                    }
                }
                var descriptionId = Traverse.Create(__instance).Property("descriptionId").GetValue<string>();
                var call = Traverse.Create(__instance).Property("call").GetValue<string>();
                var first = Traverse.Create(__instance).Property("first").GetValue<string>();
                var last = Traverse.Create(__instance).Property("last").GetValue<string>();
                var g = Traverse.Create(__instance).Property("g").GetValue<Gender>();
                var newAge = Traverse.Create(__instance).Property("newAge").GetValue<int>();
                var details = Traverse.Create(__instance).Property("details").GetValue<string>();



                var GetPilotStat = Traverse.Create(__instance).Method("GetPilotStat", new Type[] { typeof(int) });

                var gunnery = GetPilotStat.GetValue<int>(new object[] { SimGameState.GetGunnerySkill(stats) });
                var piloting = GetPilotStat.GetValue<int>(new object[] { SimGameState.GetPilotingSkill(stats) });
                var guts = GetPilotStat.GetValue<int>(new object[] { SimGameState.GetGutsSkill(stats) });
                var tactics = GetPilotStat.GetValue<int>(new object[] { SimGameState.GetTacticsSkill(stats) });




                tagSet.Add("commander_player");
                PilotDef pilotDef = new PilotDef(new HumanDescriptionDef(
                    descriptionId,
                    call,
                    first,
                    last,
                    call,
                    g,
                    FactionEnumeration.GetNoFactionValue(),
                    newAge,
                    details,
                    ""),
                    gunnery,
                    piloting,
                    guts,
                    tactics,
                    0, 3, false, 0, "", SimGameState.GetAbilities(stats), AIPersonality.Undefined, 0, tagSet, 0, Mathf.FloorToInt(stats.GetValue<float>("ExperienceUnspent")));

                var nameAndAppearance = Traverse.Create(__instance).Property("nameAndAppearance").GetValue<SGCharacterCreationNameAndAppearanceScreen>();
                pilotDef.PortraitSettings = nameAndAppearance.portraitSettings;
                pilotDef.SetHiringHallStats(true, false, true, false);
                pilotDef.DataManager = sim.DataManager;
                pilotDef.ForceRefreshAbilityDefs();

                ___createdPilot = new Pilot(pilotDef, "commander", true);

                __result = ___createdPilot;
                return false;
            }
        }

    }
}