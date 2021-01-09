namespace Common
{
	static partial class Mod
	{
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
			public const bool isGameSNStable =
#if GAME_SN && BRANCH_STABLE
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