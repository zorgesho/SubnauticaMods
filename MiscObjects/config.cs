using Common;
using Common.Configuration;

#if DEBUG
using Common.Configuration.Actions;
#endif

namespace MiscObjects
{
	class ModConfig: Config
	{
		public readonly bool removeVanillaCounter = true;

#if DEBUG
		[Field.BindConsole]
		[Field.Action(typeof(UpdateOptionalPatches))]
#endif
		public readonly bool swimSpeedPatch = true;
	}

	class L10n: LanguageHelper
	{
		public static string ids_BoxInv	 = "BOX";
		public static string ids_OpenBox = "Open box";
	}
}