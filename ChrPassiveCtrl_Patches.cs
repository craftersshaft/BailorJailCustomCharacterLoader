using FreeStyle.Unity.Obakeidoro;
using HarmonyLib;

namespace BailCustomChars.Patches
{
    [HarmonyPatch(typeof(PsdHomeMenuComponent), "_OnAwake")]
    class PsdHomeMenuComponent_OnAwake
    {
        public static bool hasDoneThisAlready = false;
        static void Postfix(ref PsdHomeMenuComponent __instance)
        {
            CharManager.LoadCharactersFromDirectory(BailCharPlugin.rootCustomCharPath);
            hasDoneThisAlready = true;
        }
    }
}