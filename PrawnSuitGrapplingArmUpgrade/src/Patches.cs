using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using Harmony;

using Common;

namespace PrawnSuitGrapplingArmUpgrade
{
	class UpgradedGrapplingArm: MonoBehaviour
	{
	}

	
	[HarmonyPatch(typeof(ExosuitGrapplingArm), "IExosuitArm.OnUseDown")]
	static class ExosuitGrapplingArm_OnUseDown_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode.Equals(OpCodes.Ldc_R4) && instruction.operand.Equals(2.0f))
				{
					"injected".onScreen().log();
					yield return new CodeInstruction(OpCodes.Ldc_R4, 0.1f);
					continue;
				}

				yield return instruction;
			}
		}
	}



	[HarmonyPatch(typeof(Exosuit), "GetArmPrefab")]
	static class Exosuit_GetArmPrefab_Patch
	{
		static GameObject prefab = null;
		
		static bool Prefix(Exosuit __instance, TechType techType, ref GameObject __result)
		{
			if (techType == GrapArm.TechTypeID)
			{
				if (prefab == null)
				{
					prefab = UnityEngine.Object.Instantiate(__instance.GetArmPrefab(TechType.ExosuitGrapplingArmModule));
					prefab.AddComponent<UpgradedGrapplingArm>();
				}

				__result = prefab;
				
				//GameObject arm = __result.transform.Find("exosuit_01_armRight/ArmRig/exosuit_grapplingHook_geo")?.gameObject;

				return false;
			}

			return true;
		}
	}


	[HarmonyPatch(typeof(Exosuit), "OnUpgradeModuleChange")]
	static class Exosuit_OnUpgradeModuleChange_Patch
	{
		static bool Prefix(Exosuit __instance, int slotID, TechType techType, bool added)
		{
			if (techType == GrapArm.TechTypeID)
			{
				__instance.MarkArmsDirty();
				return false;
			}
			
			return true;
		}
	}


	//[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
	static class ExosuitGrapplingArm_FixedUpdate_Patch
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il0)
		{
			foreach (var instruction in instructions)
			{
				if (instruction.opcode.Equals(OpCodes.Ldc_R4) && instruction.operand.Equals(35f))
				{
					Label l1 = il0.DefineLabel();
					Label l2 = il0.DefineLabel();

					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Callvirt,
						AccessTools.Method(typeof(Component), "GetComponent").MakeGenericMethod(typeof(UpgradedGrapplingArm)));

					yield return new CodeInstruction(OpCodes.Ldnull);
					yield return new CodeInstruction(OpCodes.Call,
						AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality"));

					yield return new CodeInstruction(OpCodes.Brtrue_S, l1);
					yield return new CodeInstruction(OpCodes.Ldc_R4, 35f);
					yield return new CodeInstruction(OpCodes.Br_S, l2);
					
					var c1 = new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(Main), "config"));
					c1.labels.Add(l1);
					yield return c1;
					
					yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ModConfig), "armLength"));

					//var c1 = new CodeInstruction(OpCodes.Ldc_R4, 100f);
					//c1.labels.Add(l1);
					//yield return c1;

					var c2 = new CodeInstruction(OpCodes.Nop);
					c2.labels.Add(l2);
					yield return c2;

					continue;
				}

				yield return instruction;
			}
		}
	}


	[HarmonyPatch(typeof(ExosuitGrapplingArm), "FixedUpdate")]
	static class HintSwimToSurface_OnLanguageChanged_Patch
	{
		static bool Prefix(ExosuitGrapplingArm __instance)
		{
			//bool upgraded = __instance.GetComponent<UpgradedGrapplingArm>() != null;

			float ttt = (__instance.GetComponent<UpgradedGrapplingArm>() != null) ? 100f : 35f;
			//float ttt1 = ((__instance.GetComponent<UpgradedGrapplingArm>() != null) ? Main.config.armLength : 35f);

			if (__instance.hook.attached)
			{
				__instance.grapplingLoopSound.Play();
				Exosuit componentInParent = __instance.GetComponentInParent<Exosuit>();
				Vector3 value = __instance.hook.transform.position - __instance.front.position;
				Vector3 a = Vector3.Normalize(value);
				float magnitude = value.magnitude;
				if (magnitude > 1f)
				{
					if (!componentInParent.IsUnderwater() && componentInParent.transform.position.y + 0.2f >= __instance.grapplingStartPos.y)
					{
						a.y = Mathf.Min(a.y, 0f);
					}
					componentInParent.GetComponent<Rigidbody>().AddForce(a * Main.config.field15, ForceMode.Acceleration);
					__instance.hook.GetComponent<Rigidbody>().AddForce(-a * Main.config.field400, ForceMode.Force);
				}
				__instance.rope.SetIsHooked();
			}
			else if (__instance.hook.flying)
			{
				//$"{__instance.hook.rb.velocity} {__instance.hook.rb.velocity.magnitude}".logDbg();

				if ((__instance.hook.transform.position - __instance.front.position).magnitude > ttt)
				{
					__instance.ResetHook();
				}
				__instance.grapplingLoopSound.Play();
			}
			else
			{
				__instance.grapplingLoopSound.Stop();
			}

			return false;
		}
	}


	//[HarmonyPatch(typeof(ExosuitGrapplingArm), "OnHit")] 
	//static class HintSwimToSurface_OnLanguageChanged_sdfPatch
	//{
	//	static bool Prefix(ExosuitGrapplingArm __instance)
	//	{
	//		__instance.hook.transform.parent = null;
	//		__instance.hook.transform.position = __instance.front.transform.position;
	//		__instance.hook.SetFlying(true);
	//		Exosuit componentInParent = __instance.GetComponentInParent<Exosuit>();
	//		GameObject x = null;
	//		Vector3 a = default(Vector3);
	//		UWE.Utils.TraceFPSTargetPosition(componentInParent.gameObject, 100f, ref x, ref a, false);
	//		if (x == null || x == __instance.hook.gameObject)
	//		{
	//			a = MainCamera.camera.transform.position + MainCamera.camera.transform.forward * 25f;
	//		}
	//		Vector3 a2 = Vector3.Normalize(a - __instance.hook.transform.position);
	//		__instance.hook.rb.velocity = a2 * Main.config.field25;
	//		global::Utils.PlayFMODAsset(__instance.shootSound, __instance.front, 15f);
	//		__instance.grapplingStartPos = componentInParent.transform.position;

	//		return false;
	//	}
	//}


	//static class HHH
	//{
	//	public static bool PPP(ExosuitGrapplingArm __instance, out float cooldownDuration, ref bool __result)
	//	{
	//		"usedown".onScreen().logDbg();
	//		__instance.animator.SetBool("use_tool", true);
	//		if (!__instance.rope.isLaunching)
	//		{
	//			__instance.rope.LaunchHook(35f);
	//		}
	//		cooldownDuration = 0.1f;
	//		__result = true;

	//		return false;
	//	}
	//}

	//[HarmonyPatch(typeof(ExosuitGrapplingArm), "IExosuitArm.OnUseDown")]//, new Type[] { typeof(float) })]
	//static class HintSwimToSurface_OnLanguageChanged_sdfPatchsdfsdf
	//{
	//	static bool Prefix(ExosuitGrapplingArm __instance, out float cooldownDuration, bool __result)
	//	{
	//		"usedown".onScreen().logDbg();
	//		__instance.animator.SetBool("use_tool", true);
	//		if (!__instance.rope.isLaunching)
	//		{
	//			__instance.rope.LaunchHook(35f);
	//		}
	//		cooldownDuration = 0.5f;
	//		__result = true;

	//		return false;
	//	}
	//}



			//foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			//{
			//	if (!assembly.FullName.Contains("Console"))
			//		continue;
			//	$"---------------------{assembly.FullName}".log();
				
			//	Type[] types = assembly.GetExportedTypes();

			//	foreach (var t in types)
			//		t.FullName.log();

			//}

			//var exosuitArmF = typeof(ExosuitGrapplingArm).GetMethod("IExosuitArm.OnUseDown", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			//var exosuitArmPatch = typeof(HHH).GetMethod("PPP", BindingFlags.Public | BindingFlags.Static);


			////$"{exosuitArmF != null}   {exosuitArmPatch != null}".logDbg();
			//HarmonyHelper.harmonyInstance.Patch(exosuitArmF, new HarmonyMethod(exosuitArmPatch));



}
