using System;
using System.Runtime.InteropServices;

using Common;

namespace CustomHotkeys
{
	class ConsoleCommands: PersistentConsoleCommands
	{
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

		void OnConsoleCommand_msg(NotificationCenter.Notification n)
		{
			$"{n.getArg(0)}".onScreen();
		}


		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		static extern IntPtr FindWindow(string className, string windowName);
	}
}