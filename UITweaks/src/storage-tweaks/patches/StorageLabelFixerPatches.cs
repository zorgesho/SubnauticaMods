#if GAME_SN
using HarmonyLib;
using Common.Harmony;

namespace UITweaks.StorageTweaks
{
	static partial class StorageLabelFixers
	{
		[OptionalPatch, HarmonyPatch(typeof(ColoredLabel), "OnProtoDeserialize")]
		static class ColoredLabel_OnProtoDeserialize_Patch
		{
			static bool Prepare() => tweakEnabled;

			static void Prefix(ColoredLabel __instance)
			{
				var input = __instance.signInput.inputField;

				if (input.characterLimit < __instance.text?.Length)
					input.characterLimit = __instance.text.Length;
			}
		}
	}
}
#endif // GAME_SN