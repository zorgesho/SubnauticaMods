using Common;
using Common.Configuration;

namespace Fatigue
{
	public static class Main
	{
		internal static ModConfig config = Config.tryLoad<ModConfig>();
		
		public static void patch()
		{
			HarmonyHelper.patchAll();

			UnityEngine.GameObject hotkeys = new UnityEngine.GameObject("Hotkeys", typeof(HHH));

			UnityEngine.Object.DontDestroyOnLoad(hotkeys);
		}
	}
}