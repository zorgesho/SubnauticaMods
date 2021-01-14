using System;
using System.IO;

using UnityEngine;

namespace Common
{
	using Reflection;

	static class AssetsHelper
	{
		public static Sprite loadSprite(string textureName) => textureToSprite(loadTexture(textureName));
		public static Sprite loadSprite(string textureName, float pixelsPerUnit, float border) => textureToSprite(loadTexture(textureName), pixelsPerUnit, border);

		public static Texture2D loadTexture(string textureName)
		{
			return loadAsset<Texture2D>(textureName) ??
				   loadTextureFromFile(Paths.assetsPath + textureName) ??
				   loadTextureFromFile(Paths.modRootPath + textureName) ??
				   loadTextureFromFile(textureName);
		}

		public static GameObject loadPrefab(string prefabName) => loadAsset<GameObject>(prefabName);


		// bundle should be placed and named like this - '{ModRoot}\assets\{ModID}.assets'
		static readonly object assetBundle;

		static AssetsHelper()
		{
			string bundlePath = Paths.assetsPath + Mod.id + ".assets";

			if (File.Exists(bundlePath))
			{
				MethodWrapper loadBundle = Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadFromFile", typeof(string)).wrap();
				assetBundle = loadBundle.invoke(bundlePath);
			}
		}

		static MethodWrapper _loadAsset;

		static T loadAsset<T>(string name) where T: UnityEngine.Object
		{																								$"AssetHelper: trying to load asset '{name}' ({typeof(T)}) from bundle".logDbg();
			if (assetBundle == null)
				return null;

			_loadAsset ??= Type.GetType("UnityEngine.AssetBundle, UnityEngine.AssetBundleModule").method("LoadAsset", typeof(string), typeof(Type)).wrap();
			return _loadAsset.invoke(assetBundle, name, typeof(T)) as T;
		}

		static Sprite textureToSprite(Texture2D tex) =>
			tex == null? null: Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));

		static Sprite textureToSprite(Texture2D tex, float pixelsPerUnit, float border) =>
			tex == null? null: Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit, 0, SpriteMeshType.Tight, new Vector4(border, border, border, border));

		static Texture2D loadTextureFromFile(string textureFilePath)
		{																								$"AssetHelper: trying to load texture from file '{textureFilePath}'".logDbg();
			if (!Path.HasExtension(textureFilePath))
			{
				var dir = Path.GetDirectoryName(textureFilePath);

				if (!Directory.Exists(dir))
					return null;

				var files = Directory.GetFiles(dir, Path.GetFileName(textureFilePath) + ".*");
				Debug.assert(files.isNullOrEmpty() || files.Length == 1);

				if (!files.isNullOrEmpty())
					textureFilePath = files[0];
			}

			if (!File.Exists(textureFilePath))
				return null;

			var tex = new Texture2D(2, 2);
			return tex.LoadImage(File.ReadAllBytes(textureFilePath))? tex: null;
		}
	}
}