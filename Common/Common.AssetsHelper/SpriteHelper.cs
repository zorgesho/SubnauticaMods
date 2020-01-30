using UnityEngine;

namespace Common
{
	static partial class SpriteHelper
	{
		public static Atlas.Sprite getSprite(string spriteID)
		{
			Sprite sprite = AssetsHelper.loadSprite(spriteID);
			return sprite == null? null: new Atlas.Sprite(sprite);
		}
	}
}