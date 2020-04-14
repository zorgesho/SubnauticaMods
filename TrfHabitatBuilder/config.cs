using System.Collections.Generic;
using Common.Configuration;

namespace TrfHabitatBuilder
{
	class ModConfig: Config
	{
		public readonly bool bigInInventory = false;
		public readonly bool removeVanillaBuilder = false;
		public readonly bool slowLoopAnim = false;

		public readonly float powerConsumption = 0.5f;

		[AddToConsole("trf_hb")]
		[Field.Action(typeof(UpdateBuilderPanel))]
		public readonly bool limitBlueprints = false;

		public class LockedTabs
		{
			public List<int> trfBuilder = new List<int>() { 3, 4 };
			public List<int> vanillaBuilder = new List<int>() { 0 };

			public List<int> get(TechType builderType) => builderType == TechType.Builder? vanillaBuilder: trfBuilder;
		}
		public LockedTabs lockedTabs = new LockedTabs();

		public class LockedBlueprints
		{
			public List<TechType> trfBuilder = new List<TechType>();
			public List<TechType> vanillaBuilder = new List<TechType>()
			{
				TechType.ThermalPlant,
				TechType.BaseFiltrationMachine,
				TechType.BaseBioReactor,
				TechType.BaseNuclearReactor,
				TechType.BaseWaterPark
			};

			public List<TechType> get(TechType builderType) => builderType == TechType.Builder? vanillaBuilder: trfBuilder;
		}
		public LockedBlueprints lockedBlueprints = new LockedBlueprints();

#if DEBUG
		[AddToConsole]
		public readonly float forcedBuildTime = 1.0f;
#endif
	}
}