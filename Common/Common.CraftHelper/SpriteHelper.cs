#if GAME_SN
	using Sprite = Atlas.Sprite;
#elif GAME_BZ
	using Sprite = UnityEngine.Sprite;
#endif

namespace Common
{
	static partial class SpriteHelper
	{
		public static Sprite getSprite(TechType spriteID) => SpriteManager.Get(spriteID);
	}
}