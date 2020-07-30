using Harmony;
using UnityEngine;

namespace UITweaks
{
	//[HarmonyPatch(typeof(Inventory), "ExecuteItemAction")]
	//static class TooltipFactory_Recipe_Patchsdfsdf
	//{
	//	static void Prefix(ItemAction action)
	//	{
	//		$"{action}".log();
	//		Debug.logStack();

	//	}
	//}

	//[HarmonyPatch(typeof(QuickSlots), "Assign")]
	//static class TooltipFactory_Recipe_Patchssdfsdfdfsdf
	//{
	//	static void Prefix()
	//	{
	//	//	$"{action}".log();
	//		Debug.logStack();

	//	}
	//}

	//[HarmonyPatch(typeof(QuickSlots), "Bind")]
	//static class TooltipFactory_Recipe_Patcsdfhssdfsdfdfsdf
	//{
	//	static void Prefix()
	//	{
	//	//	$"{action}".log();
	//		Debug.logStack();

	//	}
	//}

	[HarmonyPatch(typeof(Player), "Update")]
	static class TooltipFactorysdf_Recipe_Patcsdfhssdfsdfdfsdf
	{
		static void Postfix()
		{
			TabKeys.update();
		}
	}


	static class TabKeys
	{
		static bool checkPDA()
		{
			Player main = Player.main;
			if (main.GetPDA().isInUse)
			{
				InventoryItem hoveredItem = ItemDragManager.hoveredItem;
				if (hoveredItem != null && Inventory.main.GetCanBindItem(hoveredItem))
					return true;
			}

			return false;
		}

		public static void update()
		{
			if (uGUI_BuilderMenu.singleton.state)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1))
					uGUI_BuilderMenu.singleton.SetCurrentTab(0);
				if (Input.GetKeyDown(KeyCode.Alpha2))
					uGUI_BuilderMenu.singleton.SetCurrentTab(1);
				if (Input.GetKeyDown(KeyCode.Alpha3))
					uGUI_BuilderMenu.singleton.SetCurrentTab(2);
				if (Input.GetKeyDown(KeyCode.Alpha4))
					uGUI_BuilderMenu.singleton.SetCurrentTab(3);
				if (Input.GetKeyDown(KeyCode.Alpha5))
					uGUI_BuilderMenu.singleton.SetCurrentTab(4);
			}

			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				if (!checkPDA())
				{
					Player main = Player.main;
					if (main.GetPDA().isInUse)
					{
						main.GetPDA().ui.OpenTab(PDATab.Inventory);
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				if (!checkPDA())
				{
					Player main = Player.main;
					if (main.GetPDA().isInUse)
					{
						main.GetPDA().ui.OpenTab(PDATab.Journal);
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				if (!checkPDA())
				{
					Player main = Player.main;
					if (main.GetPDA().isInUse)
					{
						main.GetPDA().ui.OpenTab(PDATab.Ping);
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				if (!checkPDA())
				{
					Player main = Player.main;
					if (main.GetPDA().isInUse)
					{
						main.GetPDA().ui.OpenTab(PDATab.Gallery);
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				if (!checkPDA())
				{
					Player main = Player.main;
					if (main.GetPDA().isInUse)
					{
						main.GetPDA().ui.OpenTab(PDATab.Log);
					}
				}
			}
			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				if (!checkPDA())
				{
					Player main = Player.main;
					if (main.GetPDA().isInUse)
					{
						main.GetPDA().ui.OpenTab(PDATab.Encyclopedia);
					}
				}
			}
		}
	}
}