#if GAME_SN
	using Sprite = Atlas.Sprite;
#elif GAME_BZ
	using Sprite = UnityEngine.Sprite;
#endif

namespace Common
{
	static partial class SpriteHelper
	{
		public static Sprite getSprite(string spriteID)
		{
			UnityEngine.Sprite sprite = AssetsHelper.loadSprite(spriteID);
#if GAME_SN
			return sprite == null? null: new Sprite(sprite);
#elif GAME_BZ
			return sprite;
#endif
		}
	}
}