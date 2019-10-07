using System.Text;

using UnityEngine;
using Harmony;
using Common;

namespace GravTrapImproved
{
	// Treader spawn chunk probability
	[HarmonyPatch(typeof(SeaTreaderSounds), "SpawnChunks")]
	static class SeaTreaderSounds_SpawnChunks_Patch
	{
		static bool Prefix(SeaTreaderSounds __instance, Transform legTr)
		{
			return UnityEngine.Random.value <= Main.config.treaderSpawnChunkProbability;
		}
	}

	// Gravtrap patches
	[HarmonyPatch(typeof(Gravsphere), "Start")]
	static class Gravsphere_Start_Patch
	{
		static void Postfix(Gravsphere __instance)
		{
			__instance.gameObject.addComponentIfNeeded<GravTrapObjectsType>();
		}
	}


	[HarmonyPatch(typeof(Gravsphere), "AddAttractable")]
	static class Gravsphere_AddAttractable_Patch
	{
		static void Postfix(Gravsphere __instance, Rigidbody r)
		{
			__instance.GetComponent<GravTrapObjectsType>().handleAttracted(r);
		}
	}

	[HarmonyPatch(typeof(Gravsphere), "IsValidTarget")]
	static class Gravsphere_IsValidTarget_Patch
	{
		static bool Prefix(Gravsphere __instance, GameObject obj, ref bool __result)
		{
			__result = __instance.GetComponent<GravTrapObjectsType>().isValidTarget(obj);
			return false;
		}
	}

	
	// GUI patches
	[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
	static class TooltipFactory_ItemCommons_Patch
	{
		static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
		{
			if (techType == TechType.Gravsphere)
				TooltipFactory.WriteDescription(sb, "Objects type: " + GravTrapObjectsType.getFrom(obj).ObjType);
		}
	}

	[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
	static class TooltipFactory_ItemActions_Patch
	{
		static void Postfix(StringBuilder sb, InventoryItem item)
		{
			if (item.item.GetTechType() == TechType.Gravsphere)
				TooltipFactory.WriteAction(sb, "MMB", "switch objects type");
		}
	}

	[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
	static class uGUI_InventoryTab_OnPointerClick_Patch
	{
		static void Postfix(InventoryItem item, int button)
		{
			if (item.item.GetTechType() == TechType.Gravsphere && button == 2)
				GravTrapObjectsType.getFrom(item.item.gameObject).ObjType += 1;
		}
	}
}