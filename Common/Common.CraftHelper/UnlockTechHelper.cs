using System.Collections.Generic;

using Harmony;

namespace Common.Crafting
{
	static class UnlockTechHelper
	{
		public enum UnlockType {Any, All};
		
		class CompoundTech
		{
			public TechType techType;
			public List<TechType> dependencies;
		}

		static readonly List<CompoundTech> compoundTech = new List<CompoundTech>();

		public static void setTechTypesForUnlock(UnlockType unlockType, TechType techForUnlock, TechType[] techToUnlock)
		{
			if (unlockType == UnlockType.All)
			{
				compoundTech.Add(new CompoundTech { techType = techForUnlock, dependencies = new List<TechType>(techToUnlock) });
			}
			else
			{
				// use smlhelper?
			}
		}

		[HarmonyPatch(typeof(KnownTech), "Add")] // todo optional patch
		static class KnownTech_Add_Patch
		{
			static void Postfix(TechType techType)
			{
				foreach (var c in compoundTech)
				{
					if (c.dependencies.Contains(techType) && !KnownTech.Contains(c.techType))
					{
						bool unlock = true;
						c.dependencies.ForEach(d => unlock &= KnownTech.Contains(d));

						if (unlock)
							KnownTech.Add(c.techType, true);
					}
				}
			}
		}
	}


	//[HarmonyPatch(typeof(KnownTech), "Analyze")]
	//static class Exosuit_SpawnArm_Patch11111
	//{
	//	static void Postfix(TechType techType)
	//	{
	//		$"-------- ANALYZED {techType}".log();
	//	}
	//}
	
	//[HarmonyPatch(typeof(PDAScanner), "Unlock")]
	//static class Exosuit_SpawnArm_Patch11111111
	//{
	//	static void Postfix(PDAScanner.EntryData entryData)
	//	{
	//		$"-------- SCANNED {entryData.key}".log();
	//	}
	//}

}