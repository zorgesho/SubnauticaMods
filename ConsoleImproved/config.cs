using Common;
using Common.Configuration;

namespace ConsoleImproved
{
	class ModConfig: Config
	{
		public readonly bool consoleEnabled = true;

		[AddToConsole("console")]
		[Field.Action(typeof(HarmonyHelper.UpdateOptionalPatches))]
		public readonly bool keepMessagesOnScreen = true;

		public readonly bool fixVanillaCommandsFloatParse = false;

		public readonly int  historySizeToSave = 100;

		[AddToConsole("console")]
		public readonly bool setInvariantCultureAppWide =
#if DEBUG
			true;
#else
			false;
#endif


#if VER_1_1_0
		public class MessagesSettings
		{
			public readonly bool useDefault = false;

			public readonly float offsetX = 5f;				// 140f
			public readonly float offsetY = 5f;				// 140f

			public readonly float ySpacing = 2f;			// 10f
			public readonly float timeFlyIn = 0.01f;		// 0.3f
			public readonly float timeDelay = 5f;			// 5f
			public readonly float timeFadeOut = 0.01f;		// 0.6f
			public readonly float timeInvisible = 0.01f;	// 0.1f

			public readonly float textLineSpacing = 0.8f; // ??
		}
		public MessagesSettings msgsSettings = new MessagesSettings();
#endif
	}
}