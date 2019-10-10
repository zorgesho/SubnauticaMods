using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using Harmony;

using Common;

namespace GravTrapImproved
{
	#region Treader chunk spawning patch
	// Treader spawn chunk probability
	[HarmonyPatch(typeof(SeaTreaderSounds), "SpawnChunks")]
	static class SeaTreaderSounds_SpawnChunks_Patch
	{
		static bool Prefix(SeaTreaderSounds __instance, Transform legTr) => UnityEngine.Random.value <= Main.config.treaderSpawnChunkProbability;
	}
	#endregion

	#region Gravtrap patches
	[HarmonyPatch(typeof(Gravsphere), "Start")]
	static class Gravsphere_Start_Patch
	{
		static void Postfix(Gravsphere __instance)
		{
			__instance.gameObject.addComponentIfNeeded<GravTrapObjectsType>();

			// patching work radius of a gravsphere
			SphereCollider sphere = __instance.gameObject.GetComponents<SphereCollider>()?.FirstOrDefault((s) => s.isTrigger);
			if (sphere)
				sphere.radius = Main.config.maxRadius;
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

	// Patching max count of attracted objects
	[HarmonyPatch(typeof(Gravsphere), "OnTriggerEnter")]
	static class Gravsphere_OnTriggerEnter_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return HarmonyHelper.changeConstToConfigVar(instructions, (sbyte)12, nameof(Main.config.maxAttractedObjects));
		}
	}

	// Patching max force applied to attracted objects
	[HarmonyPatch(typeof(Gravsphere), "ApplyGravitation")]
	static class Gravsphere_OnTriggerEnterss_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return HarmonyHelper.changeConstToConfigVar(instructions, 15f, nameof(Main.config.maxForce));
		}
	}
	#endregion

	#region GUI patches
	[HarmonyPatch(typeof(TooltipFactory), "ItemCommons")]
	static class TooltipFactory_ItemCommons_Patch
	{
		static void Postfix(StringBuilder sb, TechType techType, GameObject obj)
		{
			if (techType == TechType.Gravsphere)
			{
				if (Main.config.useWheelScroll && InputHelper.getMouseWheelValue() != 0f) // not exactly right to do it here, but I don't find a better way
					GravTrapObjectsType.getFrom(obj).ObjType += Math.Sign(InputHelper.getMouseWheelValue());

				TooltipFactory.WriteDescription(sb, "Objects type: " + GravTrapObjectsType.getFrom(obj).ObjType);
			}
		}
	}

	[HarmonyPatch(typeof(TooltipFactory), "ItemActions")]
	static class TooltipFactory_ItemActions_Patch
	{
		static readonly string buttons = (Main.config.useWheelClick? Strings.Mouse.middleButton: "") +
										((Main.config.useWheelClick && Main.config.useWheelScroll)? " or ": "") +
										 (Main.config.useWheelScroll? (Strings.Mouse.scrollUp + "/" + Strings.Mouse.scrollDown): "");
		
		static void Postfix(StringBuilder sb, InventoryItem item)
		{
			if ((Main.config.useWheelClick || Main.config.useWheelScroll) && item.item.GetTechType() == TechType.Gravsphere)
				TooltipFactory.WriteAction(sb, buttons, "switch objects type");
		}
	}

	[HarmonyPatch(typeof(uGUI_InventoryTab), "OnPointerClick")]
	static class uGUI_InventoryTab_OnPointerClick_Patch
	{
		static void Postfix(InventoryItem item, int button)
		{
			if (Main.config.useWheelClick && item.item.GetTechType() == TechType.Gravsphere && button == 2)
				GravTrapObjectsType.getFrom(item.item.gameObject).ObjType += 1;
		}
	}
	#endregion
}