using Dungeonator;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReskinSwitcherMod
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(tk2dBaseSprite), nameof(tk2dBaseSprite.Collection), MethodType.Setter)]
        [HarmonyPrefix]
        public static void ProcessNewCollectionSet(tk2dSpriteCollectionData value)
        {
            ProcessedCollections.ProcessNewCollection(value);
        }

        [HarmonyPatch(typeof(tk2dBaseSprite), nameof(tk2dBaseSprite.Awake))]
        [HarmonyPrefix]
        public static void ProcessNewCollectionAwake(tk2dBaseSprite __instance)
        {
            ProcessedCollections.ProcessNewCollection(__instance.Collection);
        }

        [HarmonyPatch(typeof(tk2dBaseSprite), nameof(tk2dBaseSprite.SetSprite), typeof(tk2dSpriteCollectionData), typeof(int))]
        [HarmonyPatch(typeof(tk2dBaseSprite), nameof(tk2dBaseSprite.SetSprite), typeof(tk2dSpriteCollectionData), typeof(string))]
        [HarmonyPrefix]
        public static void ProcessNewCollectionSS(tk2dSpriteCollectionData newCollection)
        {
            ProcessedCollections.ProcessNewCollection(newCollection);
        }

        [HarmonyPatch(typeof(tk2dTileMap), nameof(tk2dTileMap.Awake))]
        [HarmonyPrefix]
        public static void ProcessNewCollectionTilemap(tk2dTileMap __instance)
        {
            ProcessedCollections.ProcessNewCollection(__instance.spriteCollection);
        }

        [HarmonyPatch(typeof(tk2dTileMap), nameof(tk2dTileMap.Editor__SpriteCollection), MethodType.Setter)]
        [HarmonyPrefix]
        public static void ProcessNewCollectionTilemapCollection(tk2dSpriteCollectionData value)
        {
            ProcessedCollections.ProcessNewCollection(value);
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.ClearActiveGameData))]
        [HarmonyPrefix]
        public static void StartReskinRun()
        {
            if (ReskinLoader.groups == null)
                return;

            foreach(var gr in ReskinLoader.groups.Values)
            {
                if(gr == null)
                    continue;

                if (gr.TryGetReskin(gr.currentResprite, out var reskin))
                    reskin.OnNewRunStarted();
            }
        }
    }
}
