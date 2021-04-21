using System.Linq;
using UnityEngine.UI;
using Common;

namespace UITweaks
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		public void ui_setpingenabled(PingType pingType, int colorIndex, bool enabled)
		{
			PingToggles.setPingEnabled(pingType, colorIndex, enabled);
		}

#if DEBUG
		public void ui_log_known_blueprints()
		{
			var blueprintsTab = uGUI_PDA.main.tabJournal as uGUI_BlueprintsTab;

			int i = 1;
			foreach (var entry in blueprintsTab.entries)
				foreach (var tech in entry.Value.entries)
					$"{i++} {tech} {tech.Value._progress?.amount} {tech.Value._progress?.total} {tech.Value._progress?.unlocked}".logDbg();

			int blueprintCount = blueprintsTab?.entries.
									SelectMany(entry => entry.Value.entries).
									Where(entry => entry.Value?._progress.total != -1).
									Count() ?? 0;

			$"blueprintCount: {blueprintCount}".onScreen().logDbg();
		}

		public void ui_test_tooltip()
		{
			var tooltip = uGUI_Tooltip.main.gameObject;

			var text = tooltip.createChild(tooltip.getChild("Text"), "TextBottom");
			text.GetComponent<Text>().text = "Test text";

			tooltip.dump("tooltip");
		}

		public void ui_tooltipclear() => uGUI_Tooltip.Clear();

		public void ui_pdadump() => uGUI_PDA.main?.gameObject.dump();

		public void ui_log_notifications() => NotificationManager.main.ToString().logDbg();
#endif
	}
}