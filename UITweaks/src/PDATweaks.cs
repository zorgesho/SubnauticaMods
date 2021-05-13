using System.Linq;
using System.Collections;

using Harmony;
using UnityEngine;

using Common;
using Common.Harmony;

namespace UITweaks
{
	using NMGroup = NotificationManager.Group;

	static class PDATweaks
	{
		const int tooltipTextSize = 25;
		const int tooltipSmallTextSize = 20;
		const int tooltipSmallTextSizeSpacing = 5;

		static NMGroup getGroupByTabIndex(int index)
		{
			return index switch
			{
				0 => NMGroup.Inventory,
				1 => NMGroup.Blueprints,
				3 => NMGroup.Gallery,
				4 => NMGroup.Log,
				5 => NMGroup.Encyclopedia,
				_ => NMGroup.Undefined
			};
		}

		static void removeNotifications(NMGroup group)
		{
			if (group == NMGroup.Undefined)
				return;

			foreach (var n in NotificationManager.main.notifications.Keys.Where(n => n.group == group).ToList())
			{
				NotificationManager.main.notifications.Remove(n);
				NotificationManager.main.NotifyRemove(n);
			}
		}

		static string modifyTooltip(int tabIndex, string tooltip)
		{
			NMGroup group = getGroupByTabIndex(tabIndex);

			if (Main.config.pdaTweaks.tabHotkeysEnabled && Main.config.showToolbarHotkeys)
			{
				KeyCode keycode = Main.config.pdaTweaks.tabHotkeys[(PDATab)(tabIndex + 1)];
				string key = SMLHelper.V2.Utility.KeyCodeUtils.KeyCodeToString(keycode);
				tooltip = $"<size={tooltipTextSize}><color=#ADF8FFFF>{key}</color> - </size>{tooltip}";
			}

			if (Main.config.pdaTweaks.showItemCount)
			{
				int itemCount = 0;

				if (group == NMGroup.Encyclopedia)
					itemCount = PDAEncyclopedia.entries.Count;
				else if (group == NMGroup.Blueprints)
					itemCount = _blueprintCount();
				else if (tabIndex == 2) // beacons
					itemCount = (uGUI_PDA.main?.tabPing as uGUI_PingTab)?.entries.Count ?? 0;
				else
					itemCount = NotificationManager.main.targets.Keys.Where(n => n.group == group).Count();

				tooltip += $"<size={tooltipTextSize}> ({itemCount})</size>";

				static int _blueprintCount() =>
					(uGUI_PDA.main?.tabJournal as uGUI_BlueprintsTab)?.entries.
						SelectMany(entry => entry.Value.entries).
						Where(entry => entry.Value._progress == null || entry.Value._progress.total == -1).
						Count() ?? 0;
			}

			if (Main.config.pdaTweaks.allowClearNotifications)
			{
				if (group != NMGroup.Undefined && NotificationManager.main.GetCount(group) > 0)
				{
					string nextline = $"\n<size={tooltipSmallTextSizeSpacing}>\n</size>";
					string hint = $"{Strings.Mouse.rightButton} - <color=#00ffffff>{L10n.str(L10n.ids_PDAClearNotifications)}</color>";
					tooltip += $"{nextline}<size={tooltipSmallTextSize}>{hint}</size>";
				}
			}

			return tooltip;
		}

		static IEnumerator tabHotkeys(bool ignoreFirstKey)
		{
			bool ignoreKey = ignoreFirstKey;

			while (uGUI_PDA.main?.tabOpen != PDATab.None)
			{
				foreach (var key in Main.config.pdaTweaks.tabHotkeys)
				{
					if (key.Value >= KeyCode.Alpha1 && key.Value <= KeyCode.Alpha9 && _isHoveredItemBindable()) // ignore alpha keys if we binding slots
						continue;

					if (!FPSInputModule.current.lockMovement && Input.GetKeyDown(key.Value))
					{
						if (!ignoreKey || (ignoreKey = false))
							uGUI_PDA.main.OpenTab(key.Key);
					}
				}

				yield return null;
			}

			static bool _isHoveredItemBindable() =>
				uGUI_PDA.main?.tabOpen == PDATab.Inventory && Inventory.main.GetCanBindItem(ItemDragManager.hoveredItem);
		}

		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare() => Main.config.pdaTweaks.enabled;

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_PDA), "OnToolbarClick")]
			static void toolbarClick(int index, int button)
			{
				if (Main.config.pdaTweaks.allowClearNotifications && button == 1)
					removeNotifications(getGroupByTabIndex(index));
			}

			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_PDA), "GetToolbarTooltip")]
#if GAME_SN
			static void getTooltip(int index, ref string tooltipText)
			{
				tooltipText = modifyTooltip(index, tooltipText);
			}
#elif GAME_BZ
			static void getTooltip(int index, TooltipData data)
			{
				string tooltip = data.prefix.ToString();
				data.prefix.Clear();
				data.prefix.Append(modifyTooltip(index, tooltip));
			}
#endif
			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_PDA), "OnOpenPDA")]
			static void openPDA()
			{
				// if tab hotkey is pressed before PDA is open it's probably to open storage slot, so we'll ignore first pressed key
				if (Main.config.pdaTweaks.tabHotkeysEnabled)
					UWE.CoroutineHost.StartCoroutine(tabHotkeys(Main.config.pdaTweaks.tabHotkeys.Values.Any(key => Input.GetKeyDown(key))));
			}

			const string idSlotExtender = "SlotExtender" + (Mod.Consts.isGameBZ? "Zero": "");

			// compatibility patch for SlotExtender mod
			// don't close PDA when tab hotkey is pressed while inside a vehicle
			[HarmonyHelper.Patch(HarmonyHelper.PatchOptions.CanBeAbsent)]
			[HarmonyPrefix, HarmonyHelper.Patch(idSlotExtender + "." + idSlotExtender + ", " + idSlotExtender, "TryUseSlotItem")]
			static bool keepPDAOpen() => !(Main.config.pdaTweaks.tabHotkeysEnabled && Player.main?.GetPDA()?.isOpen == true);
		}
	}
}