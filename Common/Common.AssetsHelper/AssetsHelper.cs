using System;
using System.IO;
using UnityEngine;

namespace Common
{
	using Reflection;

	static class AssetsHelper
	{
		// bundle should be placed and named like this - '{ModRoot}\assets\{ModID}.assets'
		static readonly object assetBundle;

		static AssetsHelper()
		{
			string bundlePath = Paths.assetsPath + Mod.id + ".assets";

			if (File.Exists(bundlePath))
			{
				MethodWrapper loadBundle = Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadFromFile", new [] {typeof(string)}).wrap();
				assetBundle = loadBundle.invoke(bundlePath);
			}
		}

		static MethodWrapper loadAsset;

		public static GameObject loadPrefab(string prefabName)
		{
			loadAsset ??= Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadAsset", new [] {typeof(string), typeof(Type)}).wrap();
			return loadAsset.invoke(assetBundle, prefabName, typeof(GameObject)) as GameObject;
		}

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