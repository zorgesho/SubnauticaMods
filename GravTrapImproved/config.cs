using System.Collections.Generic;

using Common;
using Common.Configuration;

namespace GravTrapImproved
{
	class ModConfig: Config
	{
		public readonly bool useWheelScroll = true;
		public readonly bool useWheelClick = false;

#if DEBUG
		[Field.BindConsole("gt_mk2")]
#endif
		public class MK2Props
		{
			public readonly bool enabled = true;

			public readonly float dmgMod = 0.5f;
			public readonly float heatDmgMod = 0.2f;
			public readonly float acidDmgMod = 0.5f;

			[Field.Range(0, 30)]
			public readonly int fragmentCountToUnlock = 4; // unlock with vanilla gravtrap if zero
		}
		public MK2Props mk2 = new MK2Props();

#if !DEBUG
		[Field.Range(12, 1000)]
#endif
		[Field.BindConsole("gt_mk2")]
		public readonly int mk2MaxObjects = 20; // default: 12

#if !DEBUG
		[Field.Range(15, 1000)]
#endif
		[Field.BindConsole("gt_mk2")]
		public readonly float mk2MaxForce = 20f; // default: 15f

#if !DEBUG
		[Field.Range(17, 1000)]
#endif
		[Field.BindConsole("gt_mk2")]
		[Field.Action(typeof(GravTrapMK2Patches.UpdateRanges))]
		public readonly float mk2Range = 30f; // default: 17f

		public readonly float treaderChunkSpawnFactor = 1f;
		public readonly bool raysVisible = true;
		public readonly bool extraGUIText = true;
		public readonly LargeWorldEntity.CellLevel _changeTrapCellLevel = LargeWorldEntity.CellLevel.Near;
	}


	class L10n: LanguageHelper
	{
		public const string ids_GravTrapMK2 = "Grav trap MK2";
		public const string ids_GravTrapMK2Description = "More powerful and durable model with increased range and ability to attract more objects simultaneously.";
		public const string ids_GravTrapMenu = "Grav Trap Upgrades";

		public static readonly string ids_objectsType = "Objects type: ";
		public static readonly string ids_switchObjectsType = "switch objects type";
		public static readonly string ids_or = " or ";

		public static readonly string ids_All = "All";
	}


	class TypesConfig: Config
	{
		public class TechTypeList
		{
			public readonly string name;
			readonly HashSet<TechType> techTypes;

			public void add(TechTypeList list) => add(list.techTypes);
			public void add(IEnumerable<TechType> list) => techTypes.AddRange(list);

			public bool contains(TechType techType) => techTypes.Contains(techType);

			public TechTypeList(string name, params TechType[] techTypes)
			{
				this.name = name;
				this.techTypes = new HashSet<TechType>(techTypes);
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
#if GAME_SN
				TechType.Biter,
				TechType.Bleeder,
				TechType.CaveCrawler,
				TechType.CrabSquid,
				TechType.Eyeye,
				TechType.GarryFish,
				TechType.GhostRayBlue,
				TechType.GhostRayRed,
				TechType.HoleFish,
				TechType.Hoverfish,
				TechType.Jellyray,
				TechType.Jumper,
				TechType.LavaLarva,
				TechType.LavaLizard,
				TechType.Mesmer,
				TechType.Oculus,
				TechType.Peeper,
				TechType.RabbitRay,
				TechType.Reginald,
				TechType.Spadefish,
				TechType.Stalker,
#elif GAME_BZ
				TechType.ArcticPeeper,
				TechType.ArrowRay,
				TechType.FeatherFish,
				TechType.FeatherFishRed,
				TechType.SeaMonkeyBaby,
				TechType.SpinnerFish,
				TechType.Symbiote,
				TechType.Triops,
				TechType.DiscusFish,
				TechType.Pinnacarid,
				TechType.ArcticRay,
				TechType.NootFish,
				TechType.SeaMonkey,
				TechType.Brinewing,
#endif
				TechType.Bladderfish,
				TechType.Boomerang,
				TechType.Crash,
				TechType.Hoopfish,
				TechType.PrecursorDroid,
				TechType.Skyray
			),
			new TechTypeList
			(
				"Resources",
#if GAME_SN
				TechType.Bleach,
				TechType.GasPod,
				TechType.SandstoneChunk,
				TechType.ShaleChunk,
#elif GAME_BZ
				TechType.BreakableGold,
				TechType.BreakableLead,
				TechType.BreakableSilver,
#endif
				TechType.Aerogel,
				TechType.AluminumOxide,
				TechType.AramidFibers,
				TechType.Benzene,
				TechType.ComputerChip,
				TechType.Copper,
				TechType.DepletedReactorRod,
				TechType.Diamond,
				TechType.EnameledGlass,
				TechType.FiberMesh,
				TechType.Glass,
				TechType.Gold,
				TechType.HydrochloricAcid,
				TechType.Kyanite,
				TechType.Lead,
				TechType.LimestoneChunk,
				TechType.Lithium,
				TechType.Lubricant,
				TechType.Magnetite,
				TechType.Nickel,
				TechType.PlasteelIngot,
				TechType.Polyaniline,
				TechType.PowerCell,
				TechType.PrecursorIonCrystal,
				TechType.Quartz,
				TechType.ReactorRod,
				TechType.Salt,
				TechType.ScrapMetal,
				TechType.Silicone,
				TechType.Silver,
				TechType.Sulphur,
				TechType.Titanium,
				TechType.TitaniumIngot,
				TechType.UraniniteCrystal
			),
			new TechTypeList
			(
				"Eggs",
#if GAME_SN
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
				TechType.SpadefishEgg,
				TechType.SpadefishEggUndiscovered,
				TechType.StalkerEgg,
				TechType.StalkerEggUndiscovered,
#elif GAME_BZ
				TechType.SeaMonkeyEgg,
				TechType.ArcticRayEgg,
				TechType.ArcticRayEggUndiscovered,
				TechType.BruteSharkEgg,
				TechType.BruteSharkEggUndiscovered,
				TechType.LilyPaddlerEgg,
				TechType.LilyPaddlerEggUndiscovered,
				TechType.PinnacaridEgg,
				TechType.PinnacaridEggUndiscovered,
				TechType.SquidSharkEgg,
				TechType.SquidSharkEggUndiscovered,
				TechType.TitanHolefishEgg,
				TechType.TitanHolefishEggUndiscovered,
				TechType.TrivalveBlueEgg,
				TechType.TrivalveBlueEggUndiscovered,
				TechType.TrivalveYellowEgg,
				TechType.TrivalveYellowEggUndiscovered,
				TechType.BrinewingEgg,
				TechType.BrinewingEggUndiscovered,
				TechType.CryptosuchusEgg,
				TechType.CryptosuchusEggUndiscovered,
				TechType.GlowWhaleEgg,
				TechType.GlowWhaleEggUndiscovered,
				TechType.JellyfishEgg,
				TechType.JellyfishEggUndiscovered,
				TechType.PenguinEgg,
				TechType.PenguinEggUndiscovered,
				TechType.RockPuncherEgg,
				TechType.RockPuncherEggUndiscovered,
#endif
				TechType.GenericEgg,
				TechType.ShockerEgg,
				TechType.ShockerEggUndiscovered
			),
#if DEBUG
			new TechTypeList
			(
				"Test",
#if GAME_SN
				TechType.StalkerTooth,
				TechType.TimeCapsule,
				TechType.GasPod,
#endif
				TechType.Flare,
				TechType.Beacon,
				TechType.Gravsphere
			)
#endif // DEBUG
		};
	}
}