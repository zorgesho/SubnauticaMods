using Common;
using Common.Crafting;
using Common.Configuration;

namespace MiscPrototypes
{
	public static partial class Main
	{
		internal static readonly ModConfig config = Config.tryLoad<ModConfig>();

		public static void patch()
		{
			HarmonyHelper.patchAll(true);
			CraftHelper.patchAll();
		}
	}
}