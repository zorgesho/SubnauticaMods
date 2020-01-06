using Common;
using Common.Configuration;

namespace MiscObjects
{
	class ModConfig: Config
	{
		public readonly bool removeVanillaCounter = true;
	}

	class L10n: LanguageHelper
	{
		public static string ids_BoxInv	 = "BOX";
		public static string ids_OpenBox = "Open box";
	}
}