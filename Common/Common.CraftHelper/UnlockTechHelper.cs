using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;
using SMLHelper.V2.Handlers;

namespace Common.Crafting
{
	using Harmony;
	using Reflection;

	static class UnlockTechHelper
	{
		// key - original fragment tech type, value - substitute fragment tech type
		static readonly Dictionary<TechType, TechType> fragments = new Dictionary<TechType, TechType>();

		// key - tech for unlocking, value - tech for unlockPopup sprite (can be tech type or fragment type)
		static readonly Dictionary<TechType, TechType> unlockPopups = new Dictionary<TechType, TechType>();

		static bool inited = false;
		static void init()
		{
			if (inited || !(inited = true))
				return;

			HarmonyHelper.patch();

			// patching separately because of HarmonyPriority
			HarmonyHelper.patch(typeof(KnownTech).method("Initialize"), postfix: typeof(UnlockTechHelper).method(nameof(unlockPopupsUpdate)));
		}

		public static void setFragmentTypeToUnlock(TechType unlockTechType, TechType origFragTechType, TechType substFragTechType, int fragCount, float scanTime)
		{
			init();

			PDAHandler.AddCustomScannerEntry(new PDAScanner.EntryData()
			{
				key = substFragTechType,
				blueprint = unlockTechType,
				totalFragments = fragCount,
				scanTime = scanTime,
				destroyAfterScan = true,
				locked = true,
				isFragment = true
			});

			fragments[origFragTechType] = substFragTechType;
			setUnlockNotification(unlockTechType, origFragTechType);
		}

		public static void setUnlockNotification(TechType techType, UnityEngine.Sprite unlockPopup)
		{
			KnownTechHandler.SetAnalysisTechEntry(techType, new TechType[0], UnlockSprite: unlockPopup);
		}

		public static void setUnlockNotification(TechType techType, TechType unlockPopupTechType)
		{
			setUnlockNotification(techType, null);
			unlockPopups[techType] = unlockPopupTechType;
		}


		#region patches
		// substitute fragment tech type if it already known (for use in transpiler)
		static TechType substituteTechType(TechType scanTechType)
		{
			if (!fragments.TryGetValue(scanTechType, out TechType substTechType))
				return scanTechType;

			return PDAScanner.complete.Contains(scanTechType)? substTechType: scanTechType;
		}

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(PDAScanner), "Scan")]
		static IEnumerable<CodeInstruction> scannerPatch(IEnumerable<CodeInstruction> cins) =>
			CIHelper.ciInsert(cins,
				cin => cin.isOp(OpCodes.Stloc_0), +1, 1,
					OpCodes.Ldloc_0,
					new CodeInstruction(OpCodes.Call, typeof(UnlockTechHelper).method(nameof(UnlockTechHelper.substituteTechType))),
					OpCodes.Stloc_0);

		[HarmonyPrefix]
		[HarmonyPatch(typeof(PDAScanner), "ContainsCompleteEntry")] // for loot spawning
		static bool fragmentCheckOverride(TechType techType, ref bool __result)
		{
			if (!fragments.TryGetValue(techType, out TechType substTechType))
				return true;

			__result = PDAScanner.complete.Contains(substTechType);
			return false;
		}

		[HarmonyPriority(Priority.Low)]
		static void unlockPopupsUpdate()
		{
			KnownTech.AnalysisTech _getEntry(TechType techType) =>
				KnownTech.analysisTech.FirstOrDefault(tech => tech.techType == techType);

			foreach (var popup in unlockPopups)
			{
				var tech   = _getEntry(popup.Key);
				var sprite = _getEntry(popup.Value)?.unlockPopup;

				if (sprite == null && PDAScanner.GetEntryData(popup.Value) is PDAScanner.EntryData fragData)
					sprite = _getEntry(fragData.blueprint)?.unlockPopup; // try popup.Value as fragment tech type

				if (sprite != null)
					tech.unlockPopup = sprite;
			}
		}
		#endregion
	}
}