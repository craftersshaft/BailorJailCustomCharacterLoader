using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using System.Reflection;
using System.Runtime.InteropServices;
using System;
using FreeStyle.Unity.Obakeidoro;
using FreeStyle.Unity.Menu;
using AccountDefines;
using Il2CppSystem.IO;
using BepInEx.Configuration;
using UnityEngine;
using Il2CppSystem.Collections.Generic;
using FreeStyle.Unity.Common;
using System.Linq;
using UnhollowerBaseLib;
using Zentame2018_Unity;
using UnhollowerRuntimeLib;

namespace BailCustomChars
{

    [BepInPlugin("craftersshaft.bailorjailmods.characterloader", "Bail or Jail Custom Character Loader", "0.0.1")]
    public class BailCharPlugin : BasePlugin
    {
        internal static BailCharPlugin Instance;
        public static string rootCustomCharPath;
        public static List<AssetBundle> realAssetBundles = new List<AssetBundle>();

        // public static ConfigEntry<bool> configShouldReplaceAudio;
        // public static ConfigEntry<bool> configShouldLoadExternalAudio;
        //public static ConfigEntry<bool> configShouldReplaceTexturesFromBundles;

        public override void Load()
        {
            Instance = this;
            // Plugin startup logic
            Log.LogInfo($"Plugin BailOrJailPropSkill is loaded!");
            rootCustomCharPath = Path.Combine(Paths.BepInExRootPath, "CustomCharacters");
            rootCustomCharPath = rootCustomCharPath.Replace("\\", "/");
            Directory.CreateDirectory(rootCustomCharPath);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            //configShouldReplaceAudio = Config.Bind("AudioReplacement",      // The section under which the option is shown
            //                    "ShouldReplaceAudio",  // The key of the configuration option in the configuration file
            //                    true, // The default value
            //                    "If true, "); // Description of the option to show in the config file

        }
    }

}