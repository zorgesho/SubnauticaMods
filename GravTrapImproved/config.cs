using System.Collections.Generic;

using Oculus.Newtonsoft.Json;
using Oculus.Newtonsoft.Json.Converters;

using Common.Configuration;

namespace GravTrapImproved
{
	class ModConfig: Config
	{
		const bool rangesLimited =
#if DEBUG
			false;
#else
			true;
#endif
		public readonly bool useWheelScroll = true;
		public readonly bool useWheelClick = false;

		public readonly float treaderChunkSpawnFactor = 1f;

		public readonly bool mk2Enabled = true;

		[Field.Range(0, 30)]
		public readonly int mk2FragmentCountToUnlock = 4; // unlock with vanilla gravtrap if zero

		[Field.Range(12, rangesLimited? 24: 1000)]
		[AddToConsole("gt_mk2")]
		public readonly int mk2MaxObjectCount = 18; // default: 12

		[Field.Range(15, rangesLimited? 100: 1000)]
		[AddToConsole("gt_mk2")]
		public readonly float mk2MaxForce = 25f; // default: 15f

		[Field.Range(17, rangesLimited? 50: 1000)]
		[AddToConsole("gt_mk2")]
		[Field.Action(typeof(GravTrapMK2Patches.UpdateRadiuses))]
		public readonly float mk2MaxRadius = 25f; // default: 17f

		public readonly LargeWorldEntity.CellLevel _changeTrapCellLevel = LargeWorldEntity.CellLevel.Near;
	}


	class TypesConfig: Config
	{
		public class TechTypeList
		{
			[JsonArray(ItemConverterType = typeof(StringEnumConverter))]
			class TechTypes: HashSet<TechType>
			{
				public TechTypes(IEnumerable<TechType> enumerable): base(enumerable) {}
			}

			public readonly string name;
			readonly TechTypes techTypes;

			public void add(TechTypeList list) => add(list.techTypes);
			public void add(IEnumerable<TechType> list) => techTypes.AddRange(list);

			public bool contains(TechType techType) => techTypes.Contains(techType);

			public TechTypeList(string name, params TechType[] techTypes)
			{
				this.name = name;
				this.techTypes = new TechTypes(techTypes);
			}
		}

		public readonly List<string> noJoin = new List<string>()
		{
#if DEBUG
			"Test"
#endif
		};

		public readonly List<TechTypeList> techTypeLists = new List<TechTypeList>() // from GravSphere.allowedTechTypes[]
		{
			new TechTypeList
			(
				"Creatures",

				TechType.Biter,
				TechType.Bladderfish,
				TechType.Bleeder,
				TechType.Boomerang,
				TechType.CaveCrawler,
				TechType.CrabSquid,
				TechType.Crash, // added
				TechType.Eyeye,
				TechType.GarryFish,
				TechType.GhostRayBlue,
				TechType.GhostRayRed,
				TechType.HoleFish,
				TechType.Hoopfish,
				TechType.Hoverfish,
				TechType.Jellyray,
				TechType.Jumper,
				TechType.LavaLarva,
				TechType.LavaLizard,
				TechType.Mesmer,
				TechType.Oculus,
				TechType.Peeper,
				TechType.PrecursorDroid,
				TechType.RabbitRay,
				TechType.Reginald,
				TechType.Skyray,
				TechType.Spadefish,
				TechType.Stalker
			),
			new TechTypeList
			(
				"Resources",

				TechType.AcidOld,
				TechType.Aerogel,
				TechType.AluminumOxide,
				TechType.AminoAcids,
				TechType.AramidFibers,
				TechType.BasaltChunk,
				TechType.BatteryAcidOld,
				TechType.Benzene,
				TechType.Bleach,
				TechType.CalciumChunk,
				TechType.CombustibleOld,
				TechType.ComputerChip,
				TechType.Copper,
				//TechType.CrashPowder,
				TechType.DepletedReactorRod,
				TechType.Diamond,
				TechType.Enamel,
				TechType.EnameledGlass,
				TechType.Fiber,
				TechType.FiberMesh,
				TechType.GasPod,
				TechType.Glass,
				TechType.Gold,
				TechType.Graphene,
				TechType.HydrochloricAcid,
				TechType.Kyanite,
				TechType.Lead,
				TechType.LimestoneChunk,
				TechType.Lithium,
				TechType.Lodestone,
				TechType.Lubricant,
				TechType.Magnesium,
				TechType.Magnetite,
				TechType.MercuryOre,
				TechType.Nanowires,
				TechType.Nickel,
				TechType.ObsidianChunk,
				TechType.OpalGem,
				TechType.PlasteelIngot,
				TechType.Polyaniline,
				TechType.PowerCell,
				TechType.PrecursorIonCrystal,
				TechType.Quartz,
				TechType.ReactorRod,
				TechType.Salt,
				TechType.SandstoneChunk,
				TechType.ScrapMetal,
				TechType.ShaleChunk,
				TechType.Silicone,
				TechType.Silver,
				TechType.Sulphur,
				TechType.Titanium,
				TechType.TitaniumIngot,
				TechType.UraniniteCrystal,
				TechType.Uranium,
				TechType.VesselOld
			),
			new TechTypeList
			(
				"Eggs",

				TechType.GrandReefsEgg,
				TechType.GrassyPlateausEgg,
				TechType.KelpForestEgg,
				TechType.KooshZoneEgg,
				TechType.LavaZoneEgg,
				TechType.MushroomForestEgg,
				TechType.TwistyBridgesEgg,
				TechType.BonesharkEgg,
				TechType.BonesharkEggUndiscovered,
				TechType.CrabsnakeEgg,
				TechType.CrabsnakeEggUndiscovered,
				TechType.CrabsquidEgg,
				TechType.CrabsquidEggUndiscovered,
				TechType.CrashEgg,
				TechType.CrashEggUndiscovered,
				TechType.CutefishEgg,
				TechType.CutefishEggUndiscovered,
				TechType.GasopodEgg,
				TechType.GasopodEggUndiscovered,
				TechType.JellyrayEgg,
				TechType.JellyrayEggUndiscovered,
				TechType.JumperEgg,
				TechType.JumperEggUndiscovered,
				TechType.LavaLizardEgg,
				TechType.LavaLizardEggUndiscovered,
				TechType.MesmerEgg,
				TechType.MesmerEggUndiscovered,
				TechType.RabbitrayEgg,
				TechType.RabbitrayEggUndiscovered,
				TechType.ReefbackEgg,
				TechType.ReefbackEggUndiscovered,
				TechType.SandsharkEgg,
				TechType.SandsharkEggUndiscovered,
				TechType.ShockerEgg,
				TechType.ShockerEggUndiscovered,
				TechType.SpadefishEgg,
				TechType.SpadefishEggUndiscovered,
				TechType.StalkerEgg,
				TechType.StalkerEggUndiscovered,
				TechType.GenericEgg
			),
#if DEBUG
			new TechTypeList
			(
				"Test",

				TechType.StalkerTooth,
				TechType.Flare,
				TechType.Beacon,
				TechType.TimeCapsule
			)
#endif
		};
	}
}