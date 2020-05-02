using UnityEngine;
using UnityEngine.Events;
using Harmony;

using Common;

#pragma warning disable

namespace Fatigue
{
	[HarmonyPatch(typeof(Bed), "Start")] // HACK TODO: 
	static class Bed_Start_Patch
	{
		static void Prefix(Bed __instance)
		{
			__instance.kSleepInterval = 0;
		}
	}
	
	
	[HarmonyPatch(typeof(Bed), "EnterInUseMode")]
	static class Bed_EnterInUseMode_Patch
	{
		static bool Prefix(Bed __instance, Player player)
		{
			"EnterInUseMode called".log();

			if (__instance.inUseMode == Bed.InUseMode.None)
			{
				__instance.SwitchBedUsingEffects(player, true);
		
				if (GameOptions.GetVrAnimationMode())
					__instance.ForcedSetAnimParams(true, false, player.playerAnimator);
		
				player.cinematicModeActive = true;
				MainCameraControl.main.viewModel.localRotation = Quaternion.identity;
				__instance.inUseMode = Bed.InUseMode.Sleeping;

				SleepGUI.main.start();

				UnityAction<SleepGUI.State> sleepGUIEventAction = null;
				sleepGUIEventAction = new UnityAction<SleepGUI.State>((state) =>
				{
					if (state == SleepGUI.State.FadeOut)
					{
						__instance.ExitInUseMode(player);
						SleepGUI.main.stateChangeEvent.RemoveListener(sleepGUIEventAction);
					}
				});

				SleepGUI.main.stateChangeEvent.AddListener(sleepGUIEventAction);
			}

			return false;
		}
	}
	
	
	[HarmonyPatch(typeof(Bed), "ExitInUseMode")]
	static class Bed_ExitInUseMode_Patch
	{
		static bool Prefix(Bed __instance, Player player, bool skipCinematics = false)
		{
			if (__instance.inUseMode == Bed.InUseMode.Sleeping)
			{
				__instance.SwitchBedUsingEffects(player, false);
			
				if (player == __instance.currentPlayer)
				{

					if (!skipCinematics)
					{
						__instance.ResetAnimParams(player.playerAnimator);
						__instance.currentStandUpCinematicController.StartCinematicMode(player);
					}

					if (GameOptions.GetVrAnimationMode() || skipCinematics)
						__instance.ForcedSetAnimParams(false, true, player.playerAnimator);
				}

				MainCameraControl.main.lookAroundMode = false;
				__instance.inUseMode = Bed.InUseMode.None;
			}

			return false;
		}
	}
	
	
	[HarmonyPatch(typeof(Bed), "Update")]
	static class Bed_Update_Patch
	{
		static bool Prefix(Bed __instance)
		{
			if (__instance.inUseMode == Bed.InUseMode.None && __instance.currentPlayer != null)
			{
				__instance.Subscribe(__instance.currentPlayer, false);
				__instance.currentPlayer = null;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(Bed), "OnPlayerDeath")] // TODO: try transpilers
	static class Bed_OnPlayerDeath_Patch
	{
		static bool Prefix(Bed __instance, Player player)
		{
			if (__instance.currentPlayer == player)
			{
				__instance.animator.Rebind();

				player.cinematicModeActive = false;
			}

			return false;
		}
	}

	[HarmonyPatch(typeof(Bed), "CheckIfUnderwater")] // TODO: try transpilers
	static class Bed_CheckIfUnderwater_Patch
	{
		static bool Prefix(Bed __instance, global::Utils.MonitoredValue<bool> isUnderwater)
		{
			return false;
		}
	}


	[HarmonyPatch(typeof(Bed), "GetSide")] // hack for bed in the lifepod, using only right side
	static class Bed_GetSide_Patch
	{
		static bool Prefix(Bed __instance, Player player, ref Bed.BedSide __result)
		{
			if (__instance.gameObject.transform.parent != EscapePod.main.gameObject.transform)
				return true;

			__result = Bed.BedSide.Right;
			return false;
		}
	}
}