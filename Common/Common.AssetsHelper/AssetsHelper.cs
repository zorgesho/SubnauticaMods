using System.IO;
using UnityEngine;

namespace Common
{
	static class AssetsHelper
	{
		public static Atlas.Sprite loadSprite(string textureName)
		{
			return textureToSprite(loadTextureFromFile(Paths.assetsPath + textureName + ".png"));
		}

		static Texture2D loadTextureFromFile(string textureFilePath)
		{
			if (!File.Exists(textureFilePath))
				return null;

			Texture2D tex = new Texture2D(2, 2);
			tex.LoadImage(File.ReadAllBytes(textureFilePath));

			return tex;
		}

		static Atlas.Sprite textureToSprite(Texture2D tex)
		{
			return new Atlas.Sprite(Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f)));
		}
	}
}