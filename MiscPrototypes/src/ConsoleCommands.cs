using System;
using System.Collections;
using System.Text;

using Harmony;
using UnityEngine;

using Common;
using Common.Reflection;

namespace MiscPrototypes
{
	[HarmonyPatch(typeof(GameInput), "Initialize")] // just to create console commands object
	static class GameInput_Awake_Patch_ConsoleCommands
	{
#pragma warning disable IDE0052
		static GameObject go = null;
#pragma warning restore
		static void Postfix() => go ??= PersistentConsoleCommands.createGameObject<TestConsoleCommands>();
	}


	class TestConsoleCommands: PersistentConsoleCommands
	{
		void OnConsoleCommand_debug_gameinput(NotificationCenter.Notification n)
		{
			StopAllCoroutines();

			if (n.getArg<bool>(0))
				StartCoroutine(_dbg());

			static IEnumerator _dbg()
			{
				while (true)
				{
					var sb = new StringBuilder();
					sb.AppendLine();
					sb.AppendLine($"clearInput: {GameInput.clearInput}; scanningInput: {GameInput.scanningInput}");
					sb.AppendLine($"movedir: {GameInput.GetMoveDirection()}");
					sb.AppendLine($"playercontroller: {Player.main?.playerController.inputEnabled}");
					sb.AppendLine($"fpsmod lock: {FPSInputModule.current.lockMovement}");

					foreach (var mod in InputHelper.KeyWithModifier.modifiers)
					{
						sb.AppendLine($"{mod}: down: {Input.GetKeyDown(mod)} up: {Input.GetKeyUp(mod)} held: {Input.GetKey(mod)}");
					}

					sb.ToString().onScreen("gameinput");
					yield return null;
				}
			}
		}


		void OnConsoleCommand_debug_print_gameinput(NotificationCenter.Notification n)
		{
			var sb = new StringBuilder();
			sb.AppendLine();
			for (int i = 0; i < GameInput.inputs.Count; i++)
			{
				var input = GameInput.inputs[i];
				sb.AppendLine($"{input.name} {(int)input.keyCode} {input.axis} {input.axisPositive}");
			}

			sb.ToString().saveToFile("gameinput");
		}


		void OnConsoleCommand_equip1(NotificationCenter.Notification n)
		{
			//this.DeselectInternal();
			if (n.getArgCount() == 0)
				return;

			TechType techType = Enum.Parse(typeof(TechType), n.getArg(0), true).cast<TechType>();

			if (techType == TechType.None)
			{
				"NONE".onScreen();
				return;
			}

			InventoryItem inventoryItem = Inventory.main.container.GetItems(techType)[0];
			if (inventoryItem != null)
			{
				Pickupable item = inventoryItem.item;
				PlayerTool component = item.GetComponent<PlayerTool>();
				if (component != null)
				{
					Inventory.main.quickSlots.DrawAsTool(component);
				}
				else
				{
					Inventory.main.quickSlots.DrawAsItem(inventoryItem);
				}
				Inventory.main.quickSlots._heldItem = inventoryItem;
				//Inventory.main.quickSlots.NotifyToggle(slotID, true);
				//this.NotifySelect(slotID);
				//Equipment.SendEquipmentEvent(item, 0, this.owner, QuickSlots.slotNames[slotID]);
			}
		}

		//new Hotkey { command = "item flashlight; item lasercutter", label = null /*"Toggle Fast Start"*/, key = KeyCode.R },
		//new Hotkey { command = "bindslot 0 flashlight; equipslot 0", label = null /*"Toggle Fast Start"*/, key = KeyCode.F },
		//new Hotkey { command = "bindslot 0 lasercutter; equipslot 0", label = null /*"Toggle Fast Start"*/, key = KeyCode.L },

		void OnConsoleCommand_bindslot(NotificationCenter.Notification n)
		{
			if (n.getArgCount() < 2)
				return;

			TechType techType = Enum.Parse(typeof(TechType), n.getArg(1), true).cast<TechType>(); // TODO try convert

			if (techType == TechType.None)
			{
				"NONE".onScreen();
				return;
			}

			InventoryItem inventoryItem = Inventory.main.container.GetItems(techType)[0];
			Inventory.main.quickSlots.Bind(n.getArg<int>(0), inventoryItem);
		}

		void OnConsoleCommand_equipslot(NotificationCenter.Notification n)
		{
			if (n.getArgCount() == 1)
				Inventory.main.quickSlots.SlotKeyDown(n.getArg<int>(0));
		}
	}
}