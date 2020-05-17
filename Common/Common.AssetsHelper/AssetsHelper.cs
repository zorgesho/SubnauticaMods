using System.IO;
using UnityEngine;

namespace Common
{
	static class AssetsHelper
	{
		public static Sprite loadSprite(string textureName) =>
			textureToSprite(loadTextureFromFile(Paths.assetsPath + textureName + ".png"));

		static Sprite textureToSprite(Texture2D tex) =>
			tex == null? null: Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));

		static Texture2D loadTextureFromFile(string textureFilePath)
		{
			if (!File.Exists(textureFilePath))
				return null;

			Texture2D tex = new Texture2D(2, 2);
			return tex.LoadImage(File.ReadAllBytes(textureFilePath))? tex: null;
		}
	}
}