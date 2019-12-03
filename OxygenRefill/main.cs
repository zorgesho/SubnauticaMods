using Common;

namespace OxygenRefill
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.patchAll();

			new OxygenRefillStation().Patch();
		}
	}
}