using UnityEngine.UI;
using Common;

namespace UITweaks
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		public void ui_test_tooltip()
		{
			var tooltip = uGUI_Tooltip.main.gameObject;

			var text = Instantiate(tooltip.getChild("Text"), tooltip.transform);
			text.name = "TextBottom";

			text.GetComponent<Text>().text = "Test text";
			tooltip.dump("tooltip");
		}

		public void ui_tooltipclear() => uGUI_Tooltip.Clear();

		public void ui_pdadump()
		{
			uGUI_PDA.main?.gameObject.dump();
		}
	}
}