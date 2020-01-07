namespace Common
{
	static partial class SpriteHelper
	{
		public static Atlas.Sprite getSprite(string spriteID) => new Atlas.Sprite(AssetsHelper.loadSprite(spriteID));
	}
}