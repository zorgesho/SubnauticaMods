using System;
using System.Runtime.InteropServices;

using UnityEngine;

using Common;

using Object = UnityEngine.Object;

namespace CustomHotkeys
{
	static class HotkeyHelper
	{
		static GameObject gameObject = null;
		
		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		private static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);
		
		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindow(string className, string windowName);

		static void setPos()
		{
			IntPtr p = FindWindow(null, "Subnautica");
			if (p != null)
				p.ToString().onScreen();
			
			SetWindowPos(FindWindow(null, "Subnautica"), 0, 10, 500, 0, 0, 0x0001);
		}

		class Hotkeys: MonoBehaviour
		{
			void Update()
			{
				if (Input.GetKeyDown(KeyCode.F1))
				{
					DisplayManager.SetResolution(1280, 720, false);
					setPos();
				}
				
				if (Input.GetKeyDown(KeyCode.F2))
					DisplayManager.SetResolution(2560, 1440, true);
			}
		}


		static public void init()
		{
			if (gameObject == null)
			{
				gameObject = new GameObject("CustomHotkeys", typeof(Hotkeys), typeof(SceneCleanerPreserve));
				Object.DontDestroyOnLoad(gameObject);
			}
		}
	}
}
