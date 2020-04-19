using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

using Common;
using Common.Configuration;

namespace TrfHabitatBuilder
{
	#region builder patches
	[HarmonyPatch(typeof(BuilderTool), "Start")]
	static class BuilderTool_Start_Patch
	{
		static bool Prefix(BuilderTool __instance) => !__instance.gameObject.GetComponent<TrfBuilderControl>();
	}

	[HarmonyPatch(typeof(BuilderTool), "OnDisable")]
	static class BuilderTool_OnDisable_Patch
	{
		static bool Prefix(BuilderTool __instance) => !__instance.gameObject.GetComponent<TrfBuilderControl>();
	}

	[HarmonyPatch(typeof(BuilderTool), "LateUpdate")]
	static class BuilderTool_LateUpdate_Patch
	{
		static bool Prefix(BuilderTool __instance)
		{
			TrfBuilderControl tbc = __instance.gameObject.GetComponent<TrfBuilderControl>();
			if (tbc == null)
				return true;

			tbc.updateBeams();
			return false;
		}
	}

	[HarmonyPatch(typeof(QuickSlots), "SetAnimationState")]
	static class QuickSlots_SetAnimationState_Patch
	{
		static readonly string builderToolName = nameof(TrfBuilder).ToLower();

		static bool Prefix(QuickSlots __instance, string toolName)
		{
			if (toolName != builderToolName)
				return true;

			__instance.SetAnimationState("terraformer");
			return false;
		}
	}
	#endregion

	#region GUI patches
	class UpdateBuilderPanel: Config.Field.IAction
	{
		public void action()
		{
			HarmonyHelper.updateOptionalPatches();

			if (!Main.config.limitBlueprints)
				uGUIBuilderMenu_Show_Patch.enableAllTabs();

			uGUI_BuilderMenu.singleton.UpdateItems();
		}
	}

	// patch for locking tabs in the builder menu based on helded builder
	[HarmonyHelper.OptionalPatch]
	[HarmonyPatch(typeof(uGUI_BuilderMenu), "Show")]
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
	[HarmonyHelper.OptionalPatch]
	[HarmonyPatch(typeof(uGUI_BuilderMenu), "UpdateItems")]
	static class uGUIBuilderMenu_UpdateItems_Patch
	{
		static bool Prepare() => Main.config.limitBlueprints;


		static List<TechType> lockedBlueprints;
		public static void init() => lockedBlueprints = Main.config.lockedBlueprints.get(GameUtils.getHeldToolType());

		public static void updateUnlockState(TechType techType, ref TechUnlockState unlockState)
		{
			if (unlockState == TechUnlockState.Available && lockedBlueprints.Contains(techType))
				unlockState = TechUnlockState.Locked;
		}

		public static void lockIcon(TechUnlockState unlockState, string stringForInt)
		{
			if (unlockState == TechUnlockState.Locked)
				uGUI_BuilderMenu.singleton.iconGrid.GetIcon(stringForInt).manager = null;
		}


		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> cins)
		{
			var list = cins.ToList();

			list.Insert(0, new CodeInstruction(OpCodes.Call, typeof(uGUIBuilderMenu_UpdateItems_Patch).method(nameof(init))));

			// insert after "TechUnlockState techUnlockState = KnownTech.GetTechUnlockState(techType);"
			HarmonyHelper.ciInsert(list, cin => cin.isOpLoc(OpCodes.Stloc_S, 4), +1, 1,
				OpCodes.Ldloc_3,
				OpCodes.Ldloca_S, 4,
				OpCodes.Call, typeof(uGUIBuilderMenu_UpdateItems_Patch).method(nameof(updateUnlockState)));

			// insert after "this.iconGrid.AddItem"
			HarmonyHelper.ciInsert(list, cin => cin.isOp(OpCodes.Ceq), +4, 1,
				OpCodes.Ldloc_S, 4,
				OpCodes.Ldloc_S, 5,
				OpCodes.Call, typeof(uGUIBuilderMenu_UpdateItems_Patch).method(nameof(lockIcon)));

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