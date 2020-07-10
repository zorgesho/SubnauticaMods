using System;
using System.IO;

using UnityEngine;

namespace Common
{
	using Reflection;

	static class AssetsHelper
	{
		public static Sprite loadSprite(string textureName) => textureToSprite(loadTexture(textureName));

		public static Texture2D loadTexture(string textureName) => loadAsset<Texture2D>(textureName) ?? loadTextureFromFile(Paths.assetsPath + textureName);

		public static GameObject loadPrefab(string prefabName) => loadAsset<GameObject>(prefabName);


		// bundle should be placed and named like this - '{ModRoot}\assets\{ModID}.assets'
		static readonly object assetBundle;

		static AssetsHelper()
		{
			string bundlePath = Paths.assetsPath + Mod.id + ".assets";

			if (File.Exists(bundlePath))
			{
				MethodWrapper loadBundle = Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadFromFile", new[] { typeof(string) }).wrap();
				assetBundle = loadBundle.invoke(bundlePath);
			}
		}

		static MethodWrapper _loadAsset;

		static T loadAsset<T>(string name) where T: UnityEngine.Object
		{																								$"AssetHelper: trying to load asset '{name}' ({typeof(T)}) from bundle".logDbg();
			if (assetBundle == null)
				return null;

			_loadAsset ??= Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadAsset", new[] { typeof(string), typeof(Type) }).wrap();
			return _loadAsset.invoke(assetBundle, name, typeof(T)) as T;
		}

		static Sprite textureToSprite(Texture2D tex) =>
			tex == null? null: Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));

		static Texture2D loadTextureFromFile(string textureFilePath)
		{																								$"AssetHelper: trying to load texture from file '{textureFilePath}'".logDbg();
			if (Path.GetExtension(textureFilePath) == "")
			{
				textureFilePath += ".png";																$"AssetHelper: adding extension to filename ({textureFilePath})'".logDbg();
			}

			if (!File.Exists(textureFilePath))
				return null;

			Texture2D tex = new Texture2D(2, 2);
			return tex.LoadImage(File.ReadAllBytes(textureFilePath))? tex: null;
		}
	}
}