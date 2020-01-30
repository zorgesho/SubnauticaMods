using Common;

namespace RenameBeacons
{
	public static class Main
	{
		public static void patch()
		{
			HarmonyHelper.patchAll();
			LanguageHelper.init();
		}
	}

	class L10n: LanguageHelper
	{
		public static readonly string ids_name = "Name";
		public static readonly string ids_rename = "rename";
	}
}