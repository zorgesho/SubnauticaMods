using Common;
using Common.Configuration;

namespace HabitatPlatform
{
#if DEBUG
	[Field.BindConsole("hbpl")]
#endif
	class ModConfig: Config
	{
		public readonly float stepMove = 0.1f;
		public readonly float stepRotate = 0.5f;
		public readonly float defPosY = -0.0465f;

		public readonly bool chargeCameras = Mod.Consts.isDevBuild;
		public readonly bool ignoreEnginesColliders = true;
		public readonly bool tryFixCollisionBug = true;
#if DEBUG
		public readonly bool dbgVisibleFoundations = false;
		public readonly bool dbgPrintColliders = false;
		public readonly bool dbgFastPlatformBuild = true;
		public readonly bool dbgPatches = true;
#endif
	}

	class L10n: LanguageHelper
	{
		public const string ids_HPName = "Habitat Platform";
		public const string ids_HPDesc = "Floating platform that can be used for building habitats.";
		public const string ids_ChargeCamera = "Charge camera drone";
		public const string ids_ChargeCameraDesc = "Charge and repair camera drone.";

		public static readonly string ids_platformsNode = "Platforms";
	}
}