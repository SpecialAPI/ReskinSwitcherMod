using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using System.IO;
using Gunfiguration;
using System.Reflection;

namespace ReskinSwitcherMod
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "spapi.etg.reskinswitcher";
        public const string NAME = "Reskin Switcher";
        public const string VERSION = "1.1.0";

        public void Awake()
        {
            ReskinLoader.LoadReskins();
            ReskinConfig.Init();

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), GUID);
        }
    }
}
