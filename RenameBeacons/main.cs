using Common;

namespace RenameBeacons
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.patchAll(false);
		}
	}
}