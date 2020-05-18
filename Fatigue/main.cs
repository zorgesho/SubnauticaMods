using Common;
using Common.Harmony;

namespace Fatigue
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll();

			UnityEngine.GameObject hotkeys = new UnityEngine.GameObject("Hotkeys", typeof(HHH));
			UnityEngine.Object.DontDestroyOnLoad(hotkeys);
		}
	}
}