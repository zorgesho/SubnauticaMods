using System;

namespace Common
{
	static partial class Mod
	{
		[Obsolete]
		public const bool isDevBuild =
#if DEBUG
			true;
#else
			false;
#endif
		[Obsolete]
		public const bool isBranchStable =
#if BRANCH_STABLE
			true;
#else
			false;
#endif
		public static class Consts
		{
			public const bool isDevBuild =
#if DEBUG
				true;
#else
				false;
#endif

			public const bool isBranchStable =
#if BRANCH_STABLE
				true;
#else
				false;
#endif

			public const bool isGameSN =
#if GAME_SN
				true;
#else
				false;
#endif

			public const bool isGameBZ =
#if GAME_BZ
				true;
#else
				false;
#endif
		}
	}
}