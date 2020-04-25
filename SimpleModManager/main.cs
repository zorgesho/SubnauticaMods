using Common;

namespace SimpleModManager
{
	public static class Main
	{
		internal static readonly ModConfig config = Mod.init<ModConfig>();

		public static void patch()
		{
			ModManager.init();
		}
	}
}