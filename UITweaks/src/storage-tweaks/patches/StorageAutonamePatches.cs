using System.Linq;

using HarmonyLib;

using Common;
using Common.Harmony;

namespace UITweaks.StorageTweaks
{
	partial class StorageAutoname
	{
		[OptionalPatch, PatchClass]
		static class Patches
		{
			static bool prepare()
			{
				Serializer.init();
				return tweakEnabled;
			}

			static string prevLabel = null;

			[HarmonyPrefix, HarmonyPatch(typeof(uGUI_SignInput), "OnSelect")]
			static void uGUISignInput_OnSelect_Prefix(uGUI_SignInput __instance)
			{
				prevLabel = __instance.text;
			}

			// don't autoname storage with manually changed label
			// autoname storage again if label changed to empty string
			[HarmonyPostfix, HarmonyPatch(typeof(uGUI_SignInput), "OnDeselect")]
			static void uGUISignInput_OnDeselect_Postfix(uGUI_SignInput __instance)
			{
				if (prevLabel == __instance.text)
					return;

				using var _ = Debug.profiler("uGUISignInput_OnDeselect_Postfix");
				var storage = StorageLabels.allLabels.FirstOrDefault(l => l.label.signInput == __instance)?.owner;

				if (!storage)
					return;

				bool shouldManage = __instance.text == "";
				string storageID = storage.storageRoot.Id;

				manageStorage(storageID, shouldManage);

				if (shouldManage)
					storage.GetComponent<StorageAutoname>()?.onContentsChanged();
			}
		}
	}
}