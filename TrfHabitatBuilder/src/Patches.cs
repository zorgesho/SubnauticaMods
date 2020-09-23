using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Harmony;
using Common.Configuration;

namespace TrfHabitatBuilder
{
	[PatchClass]
	static class BuilderToolPatches
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(BuilderTool), "Start")]
		[HarmonyPatch(typeof(BuilderTool), "OnDisable")]
		static bool BuilderTool_Prefix(BuilderTool __instance) => !__instance.gameObject.GetComponent<TrfBuilderControl>();

		[HarmonyPrefix, HarmonyPatch(typeof(BuilderTool), "LateUpdate")]
		static bool BuilderTool_LateUpdate_Prefix(BuilderTool __instance)
		{
			TrfBuilderControl tbc = __instance.gameObject.GetComponent<TrfBuilderControl>();
			if (tbc == null)
				return true;

			tbc.updateBeams();
			return false;
		}

		static readonly string trfBuilderToolName = nameof(TrfBuilder).ToLower();

		[HarmonyPrefix, HarmonyPatch(typeof(QuickSlots), "SetAnimationState")]
		static bool QuickSlots_SetAnimationState_Prefix(QuickSlots __instance, string toolName)
		{
			if (toolName != trfBuilderToolName)
				return true;

			__instance.SetAnimationState("terraformer");
			return false;
		}
	}

	#region GUI patches
	class UpdateBuilderPanel: Config.Field.IAction
	{
		public void action()
		{
			OptionalPatches.update(); // we need to update patches before the code below, so we using single action

			if (!Main.config.limitBlueprints)
				uGUIBuilderMenu_Show_Patch.enableAllTabs();

			uGUI_BuilderMenu.singleton.UpdateItems();
		}
	}

	[OptionalPatch, PatchClass]
	static class LockedTabsPatches
	{
		static bool prepare() => Main.config.limitBlueprints && Main.config.tabLockFix;

		// locking tabs for hotkeys
		[HarmonyPrefix, HarmonyPatch(typeof(uGUI_BuilderMenu), "SetCurrentTab")]
		static bool uGUI_BuilderMenu_SetCurrentTab_Prefix(int index)
		{
			return !Main.config.lockedTabs.get(GameUtils.getHeldToolType()).Contains(index);
		}

		// locking tabs for gamepad
		[HarmonyPrefix, HarmonyPatch(typeof(uGUI_BuilderMenu), "OnButtonDown")]
		static bool uGUI_BuilderMenu_OnButtonDown_Prefix(uGUI_BuilderMenu __instance, GameInput.Button button)
		{
			int dir = button switch
			{
				GameInput.Button.UIPrevTab => -1,
				GameInput.Button.UINextTab => +1,
				_ => 0
			};

			if (dir == 0)
				return true;

			int nextTab = __instance.TabOpen;
			var list = Main.config.lockedTabs.get(GameUtils.getHeldToolType());

			do
			{
				nextTab = (__instance.TabCount + nextTab + dir) % __instance.TabCount;
			}
			while (list.Contains(nextTab));

			__instance.SetCurrentTab(nextTab);
			return false;
		}
	}

	// patch for locking tabs in the builder menu based on helded builder
	[OptionalPatch, HarmonyPatch(typeof(uGUI_BuilderMenu), "Show")]
	static class uGUIBuilderMenu_Show_Patch
	{
		static bool Prepare() => Main.config.limitBlueprints;

		static uGUI_Toolbar toolbar;
		static uGUI_Toolbar getToolbar() =>
			toolbar? toolbar: toolbar = uGUI_BuilderMenu.singleton.gameObject.getChild("Content/Toolbar").GetComponent<uGUI_Toolbar>();

		static void setLocked(this uGUI_ItemIcon icon, bool locked)
		{
			if (locked)
			{
				icon.OnLock();
				icon.manager = null;
			}
			else
			{
				icon.OnUnlock();
				icon.manager = getToolbar();
			}
		}

		public static void enableAllTabs() => getToolbar().icons.ForEach(icon => icon.setLocked(false));

		static void Prefix()
		{
			var list = Main.config.lockedTabs.get(GameUtils.getHeldToolType());
			var icons = getToolbar().icons;

			int firstUnlocked = -1;
			for (int i = 0; i < icons.Count; i++)
			{
				bool locked = list.Contains(i);
				icons[i].setLocked(locked);

				if (!locked && firstUnlocked == -1)
					firstUnlocked = i;
			}

			if (list.Contains(uGUI_BuilderMenu.singleton.TabOpen))
				uGUI_BuilderMenu.singleton.SetCurrentTab(firstUnlocked);
			else
				uGUI_BuilderMenu.singleton.UpdateItems();
		}
	}

	// patch for locking blueprints based on helded builder
	[OptionalPatch, HarmonyPatch(typeof(uGUI_BuilderMenu), "UpdateItems")]
	static class uGUIBuilderMenu_UpdateItems_Patch
	{
		static bool Prepare() => Main.config.limitBlueprints;

		static List<TechType> lockedBlueprints;

		static void init() => lockedBlueprints = Main.config.lockedBlueprints.get(GameUtils.getHeldToolType());

		static void updateUnlockState(TechType techType, ref TechUnlockState unlockState)
		{
			if (unlockState == TechUnlockState.Available && lockedBlueprints.Contains(techType))
				unlockState = TechUnlockState.Locked;
		}
		delegate void _updateUnlockState(TechType techType, ref TechUnlockState unlockState);

		static void lockIcon(TechUnlockState unlockState, string stringForInt)
		{
			if (unlockState == TechUnlockState.Locked)
				uGUI_BuilderMenu.singleton.iconGrid.GetIcon(stringForInt).manager = null;
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();

			list.Insert(0, CIHelper.emitCall<Action>(init));

			// insert after "TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType);"
			CIHelper.ciInsert(list, cin => cin.isOpLoc(OpCodes.Stloc_S, 4), +1, 1,
				OpCodes.Ldloc_3,
				OpCodes.Ldloca_S, 4,
				CIHelper.emitCall<_updateUnlockState>(updateUnlockState));

			// insert after "this.iconGrid.AddItem"
			CIHelper.ciInsert(list, cin => cin.isOp(OpCodes.Ceq), +4, 1,
				OpCodes.Ldloc_S, 4,
				OpCodes.Ldloc_S, 5,
				CIHelper.emitCall<Action<TechUnlockState, string>>(lockIcon));

			return list;
		}
	}
	#endregion

	#region debug patches
#if DEBUG
	// for debugging
	[HarmonyPatch(typeof(Constructable), "GetConstructInterval")]
	static class Constructable_GetConstructInterval_Patch
	{
		static bool Prefix(ref float __result)
		{
			__result = Main.config.forcedBuildTime;
			return false;
		}
	}
#endif
	#endregion
}