using System;
using Common;

namespace CustomHotkeys
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		#region dev tools
		void OnConsoleCommand_debuggui_toggleterrain(NotificationCenter.Notification _)
		{
			FindObjectsOfType<TerrainDebugGUI>().forEach(gui => gui.enabled = !gui.enabled);
		}

		void OnConsoleCommand_debuggui_togglegraphics(NotificationCenter.Notification _)
		{
			FindObjectsOfType<GraphicsDebugGUI>().forEach(gui => gui.enabled = !gui.enabled);
		}

		void OnConsoleCommand_debuggui_hidephase(NotificationCenter.Notification n)
		{
			if (!GUIController.main)
				return;

			int phase = n.getArgCount() > 0? n.getArg<int>(0): (int)GUIController.main.hidePhase + 1;
			phase %= (int)GUIController.HidePhase.All + 1;
			GUIController.SetHidePhase(GUIController.main.hidePhase = (GUIController.HidePhase)phase);
		}
		#endregion

		void OnConsoleCommand_setwindowpos(NotificationCenter.Notification n)
		{
			WinApi.setWindowPos("Subnautica", n.getArg<int>(0), n.getArg<int>(1));
		}

		void OnConsoleCommand_setresolution(NotificationCenter.Notification n)
		{
			if (n.getArgCount() > 1)
				DisplayManager.SetResolution(Math.Max(640, n.getArg<int>(0)), Math.Max(480, n.getArg<int>(1)), !n.getArg<bool>(2));
		}

		[CommandData(caseSensitive = true, combineArgs = true)]
		void OnConsoleCommand_msg(NotificationCenter.Notification n)
		{
			n.getArg(0)?.onScreen();
		}

		void OnConsoleCommand_wait(NotificationCenter.Notification n)
		{
			HotkeyHelper.wait(n.getArg<float>(0));
		}

#if DEBUG
		void OnConsoleCommand_logassoc(NotificationCenter.Notification n)
		{
			if (n.getArgCount() == 1)
				WinApi.logAccos(n.getArg(0));
		}
#endif
	}
}