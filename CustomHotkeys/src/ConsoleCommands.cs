using System;
using System.Runtime.InteropServices;

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
			SetWindowPos(FindWindow(null, "Subnautica"), 0, n.getArg<int>(0), n.getArg<int>(1), 0, 0, 0x0001);
		}

		void OnConsoleCommand_setresolution(NotificationCenter.Notification n)
		{
			if (n.getArgCount() < 2)
				return;

			DisplayManager.SetResolution(n.getArg<int>(0), n.getArg<int>(1), !n.getArg<bool>(2));
		}

		[CommandData(caseSensitive = true, combineArgs = true)]
		void OnConsoleCommand_msg(NotificationCenter.Notification n)
		{
			n.getArg(0)?.onScreen();
		}

		void OnConsoleCommand_wait(NotificationCenter.Notification n)
		{
			HotkeyHelper.setWaitTime(n.getArg<float>(0));
		}

		#region windows API functions
		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		static extern IntPtr FindWindow(string className, string windowName);
		#endregion
	}
}