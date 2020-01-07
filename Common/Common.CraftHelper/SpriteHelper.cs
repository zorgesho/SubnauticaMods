namespace Common
{
	static partial class SpriteHelper
	{
		public static Atlas.Sprite getSprite(TechType spriteID) => SpriteManager.Get(spriteID);
	}
}