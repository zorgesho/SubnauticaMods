using System;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

using HarmonyLib;
using SMLHelper.V2.Handlers;

namespace Common.Crafting
{
	using Harmony;

	static class UnlockTechHelper
	{
		// key - original fragment tech type, value - substitute fragment tech type
		static readonly Dictionary<TechType, TechType> fragments = new();

		// key - tech for unlocking, value - tech for unlockPopup sprite (can be tech type or fragment type)
		static readonly Dictionary<TechType, TechType> unlockPopups = new();

		// techs that require multiple techs to unlock
		record CompoundTech(TechType techType, List<TechType> dependencies);
		static readonly List<CompoundTech> compoundTechs = new();

		public static void setFragmentTypeToUnlock(TechType unlockTechType, TechType origFragTechType, TechType substFragTechType, int fragCount, float scanTime)
		{
			FragmentUnlockPatches.patcher.patch();

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
			unlockPopups[unlockTechType] = origFragTechType;

			KnownTechHandler.SetAnalysisTechEntry(unlockTechType, new TechType[0]);
		}

		public static void setAllTechTypesForUnlock(TechType unlockTech, params TechType[] dependTechs)
		{
			CompoundTechUnlockPatch.patcher.patch();
			compoundTechs.Add(new (unlockTech, new (dependTechs)));
		}

		#region patches
		static class FragmentUnlockPatches
		{
			public static readonly HarmonyHelper.LazyPatcher patcher = new();

			[HarmonyTranspiler, HarmonyPatch(typeof(PDAScanner), "Scan")]
			static IEnumerable<CodeInstruction> scannerPatch(IEnumerable<CodeInstruction> cins)
			{
				static TechType _substTechType(TechType scanTechType) // substitute fragment tech type if it already known
				{
					if (!fragments.TryGetValue(scanTechType, out TechType substTechType))
						return scanTechType;

					return PDAScanner.complete.Contains(scanTechType)? substTechType: scanTechType;
				}

				return CIHelper.ciInsert(cins, ci => ci.isOp(OpCodes.Stloc_0), +1, 1,
					OpCodes.Ldloc_0,
					CIHelper.emitCall<Func<TechType, TechType>>(_substTechType),
					OpCodes.Stloc_0);
			}

			[HarmonyPrefix, HarmonyPatch(typeof(PDAScanner), "ContainsCompleteEntry")] // for loot spawning
			static bool fragmentCheckOverride(TechType techType, ref bool __result)
			{
				if (!fragments.TryGetValue(techType, out TechType substTechType))
					return true;

				__result = PDAScanner.complete.Contains(substTechType);
				return false;
			}

			[HarmonyPriority(Priority.Low)]
			[HarmonyPostfix, HarmonyHelper.Patch(typeof(KnownTech), "Initialize")]
			static void unlockPopupsUpdate()
			{
				KnownTech.AnalysisTech _getEntry(TechType techType) =>
					KnownTech.analysisTech.FirstOrDefault(tech => tech.techType == techType);

				foreach (var popup in unlockPopups)
				{
					if (_getEntry(popup.Key) is not KnownTech.AnalysisTech tech)
						continue;

					var sprite = _getEntry(popup.Value)?.unlockPopup;

					if (sprite == null && PDAScanner.GetEntryData(popup.Value) is PDAScanner.EntryData fragData)
						sprite = _getEntry(fragData.blueprint)?.unlockPopup; // try popup.Value as fragment tech type

					if (sprite != null)
						tech.unlockPopup = sprite;
				}
			}
		}

		static class CompoundTechUnlockPatch
		{
			public static readonly HarmonyHelper.LazyPatcher patcher = new();

			[HarmonyPostfix, HarmonyPatch(typeof(KnownTech), "Add")]
			static void KnownTech_Add_Postfix(TechType techType)
			{
				foreach (var tech in compoundTechs)
				{
					if (!tech.dependencies.Contains(techType) || KnownTech.Contains(tech.techType))
						continue;

					if (tech.dependencies.All(KnownTech.Contains))
						KnownTech.Add(tech.techType, true);
				}
			}
		}
		#endregion
	}
}