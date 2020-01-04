using Common.Configuration;

namespace ConsoleImproved
{
	class ModConfig: Config
	{
		public readonly bool consoleEnabled = true;
		public readonly bool keepMessagesOnScreen = true;
		public readonly bool fixVanillaCommandsFloatParse = true;

		public readonly int  historySizeToSave = 100;

		[AddToConsole("console")]
		public readonly bool setInvariantCultureAppWide = true;

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
		}
		public MessagesSettings msgsSettings = new MessagesSettings();
#endif
	}
}