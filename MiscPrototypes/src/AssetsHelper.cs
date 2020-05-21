﻿// from old solution, TODO: refactor and integrate

//	static class AssetsHelper
//	{
//		// bundle should be in mod root folder, named [AssemblyName].assets
//		static AssetBundle bundle = AssetBundle.LoadFromFile(ModPath.rootPath + Assembly.GetExecutingAssembly().GetName().Name + ".assets");

//		static public bool assetsBundleOnly = 
//#if ASSETS_BUNDLE_ONLY
//		true;
//#else
//		false;
//#endif
//		static string localAssetsPath = "assets\\";

//		static public Sprite loadSprite(string textureName)
//		{
//			if (assetsBundleOnly)
//			{
//				if (!bundle)
//				{
//					"Assets bundle not found".logError();
//					return null;
//				}																					$"Texture '{textureName}' is not found in assets bundle".logDbgError(!bundle.Contains(textureName));

//				return bundle?.LoadAsset<Sprite>(textureName);
//			}
//			else
//			{
//				string texturePath = findTextureLocation(textureName, out bool isTexInBundle);		$"load Sprite {textureName}, texture location \"{texturePath}\", isBundle:{isTexInBundle}".logDbg();

//				if (texturePath != null)
//					return isTexInBundle? bundle.LoadAsset<Sprite>(texturePath): textureToSprite(loadTextureFromFile(texturePath));

//				$"Texture {textureName} not found".logError();

//				return null;
//			}
//		}


//		static string findTextureLocation(string textureName, out bool isInBundle)
//		{
//			isInBundle = false;

//			if (File.Exists(textureName)) // if this is full path to texture
//				return textureName;

//			string texturePath = ModPath.rootPath + localAssetsPath + textureName;

//			if (File.Exists(texturePath)) // if this just name with extension
//				return texturePath;

//			if (File.Exists(texturePath + ".png")) // if this just texture name
//				return texturePath + ".png";

//			if (bundle && (isInBundle = bundle.Contains(textureName))) // check assets bundle
//				return textureName;

//			return null;
//		}


//		static Texture2D loadTextureFromFile(string textureFilePath)
//		{
//			if (File.Exists(textureFilePath))
//			{
//				byte[] imageBytes = File.ReadAllBytes(textureFilePath);
//				Texture2D texture2D = new Texture2D(2, 2, TextureFormat.BC7, false);
//				if (texture2D.LoadImage(imageBytes))
//					return texture2D;
//				else
//					$"Image located at \"{textureFilePath}\" cannot not be loaded".logError();
//			}
//			else
//				$"Image located at \"{textureFilePath}\" has not been found".logError();

//			return null;
//		}

//		static public Sprite textureToSprite(Texture2D tex)//, Vector2 pivot = new Vector2(), float pixelsPerUnit = 100f, SpriteMeshType spriteType = SpriteMeshType.Tight, Vector4 border = new Vector4())
//		{
//			return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height));//, pivot, pixelsPerUnit, 0u, spriteType, border);
//		}
//	}