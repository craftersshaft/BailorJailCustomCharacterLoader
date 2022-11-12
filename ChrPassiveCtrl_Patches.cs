using FreeStyle.Unity.Obakeidoro;
using HarmonyLib;

namespace BailCustomChars.Patches
{
    [HarmonyPatch(typeof(ChrPassiveCtrl), "Initialize")]
    class ChrPassiveCtrl_Initialize
    {
        static void Postfix(ref ChrPassiveCtrl __instance)
        {
           // if (__instance.FindPassive((PASSIVE_TYPE)202020) == null && __instance.m_owner.isMine)
           // {
           //     
           //     PassiveSkill_PropHunt thisNode = new PassiveSkill_PropHunt();
           //     thisNode.Initialize(__instance.m_owner, PassiveSkillAsset.instance.GetItem(202020));
           //     __instance.m_passiveList.Add(thisNode.Cast<PassiveSkill_PropHunt>());
           //     BailPropPlugin.Instance.Log.LogInfo("tried to add Prop Hunt Passive Skill");
           // }
        }
    }
}