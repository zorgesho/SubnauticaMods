using System.IO;
using System.Text;
using System.Collections;

using UnityEngine;

using Common;

namespace HabitatPlatform
{
	public static class TextWriterExtensions
	{
		public static void Line(this TextWriter textWriter)
		{
			textWriter.Write('\n');
		}

		public static void WriteIndented(this TextWriter textWriter, string text, int indentation)
		{
			for (int i = 0; i < indentation; i++)
				textWriter.Write('\t');

			textWriter.Write(text);
		}

		public static void WriteLineIndented(this TextWriter textWriter, string text, int indentation)
		{
			for (int i = 0; i < indentation; i++)
				textWriter.Write('\t');

			textWriter.WriteLine(text);
		}
	}

	class ConsoleCommands: PersistentConsoleCommands
	{
		static GameObject floor = null;

		static void dumpMaterialProperties(Material material, string filename = null)
		{
			if (material?.shader != Shader.Find("MarmosetUBER"))
				return;

			var sb = new StringBuilder();
			sb.AppendLine("Material properties:");
			sb.Append("Keywords:");
			material.shaderKeywords.ForEach(word => sb.Append(" " + word));

			//sb.AppendLine(material.GetColor())

			//textWriter.WriteIndented("_Color = ", indentation);
			//		textWriter.WriteLine(sharedMaterial.GetColor("_Color"));

			if (filename == null)
				sb.ToString().log();
		}

		public static void _logMaterialProperties(Material material, string filename)
		{
			int indentation = 1;
			using TextWriter textWriter = File.CreateText(Paths.modRootPath + filename);

			Shader shader = Shader.Find("MarmosetUBER");
			if (true)
			{
				Material sharedMaterial = material;
				if (sharedMaterial != null && sharedMaterial.shader == shader)
				{
					textWriter.WriteIndented("Keywords = ", indentation);
					foreach (string value in sharedMaterial.shaderKeywords)
					{
						textWriter.Write(value);
						textWriter.Write(", ");
					}
					textWriter.Line();
					textWriter.Line();
					textWriter.WriteIndented("_Color = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetColor("_Color"));
					float @float = sharedMaterial.GetFloat("_EnableMainMaps");
					textWriter.WriteIndented("_EnableMainMaps = ", indentation);
					textWriter.WriteLine("{0} {1}", @float, (@float <= 1f) ? "Enabled" : "Disabled");
					Texture texture = sharedMaterial.GetTexture("_MainTex");
					textWriter.WriteIndented("_MainTex = ", indentation);
					textWriter.WriteLine((texture != null) ? texture.name : "Null");
					Texture texture2 = sharedMaterial.GetTexture("_BumpMap");
					textWriter.WriteIndented("_BumpMap = ", indentation);
					textWriter.WriteLine((texture2 != null) ? texture2.name : "Null");
					textWriter.WriteIndented("_SpecColor = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetColor("_SpecColor"));
					textWriter.WriteIndented("_SpecInt = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetFloat("_SpecInt"));
					textWriter.WriteIndented("_Shininess = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetFloat("_Shininess"));
					int @int = sharedMaterial.GetInt("_MarmoSpecEnum");
					//textWriter.WriteIndented("_MarmoSpecEnum = ", indentation);
					//textWriter.WriteLine("{0} {1}", @int, (MarmoSpecEnum)@int);
					float float2 = sharedMaterial.GetFloat("_EnableLighting");
					textWriter.WriteIndented("_EnableLighting = ", indentation);
					textWriter.WriteLine("{0} {1}", float2, (float2 <= 1f) ? "Enabled" : "Disabled");
					Texture texture3 = sharedMaterial.GetTexture("_SIGMap");
					textWriter.WriteIndented("_SIGMap = ", indentation);
					textWriter.WriteLine((texture3 != null) ? texture3.name : "Null");
					Texture texture4 = sharedMaterial.GetTexture("_SpecTex");
					textWriter.WriteIndented("_SpecTex = ", indentation);
					textWriter.WriteLine((texture4 != null) ? texture4.name : "Null");
					textWriter.WriteIndented("_Fresnel = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetFloat("_Fresnel"));
					float float3 = sharedMaterial.GetFloat("_EnableGlow");
					textWriter.WriteIndented("_EnableGlow = ", indentation);
					textWriter.WriteLine("{0} {1}", float3, (float3 <= 1f) ? "Enabled" : "Disabled");
					textWriter.WriteIndented("_GlowStrength = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetFloat("_GlowStrength"));
					textWriter.WriteIndented("_GlowStrengthNight = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetFloat("_GlowStrengthNight"));
					textWriter.WriteIndented("_EmissionLM = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetFloat("_EmissionLM"));
					textWriter.WriteIndented("_EmissionLMNight = ", indentation);
					textWriter.WriteLine(sharedMaterial.GetFloat("_EmissionLMNight"));
					Texture texture5 = sharedMaterial.GetTexture("_Illum");
					textWriter.WriteIndented("_Illum = ", indentation);
					textWriter.WriteLine((texture5 != null) ? texture5.name : "Null");
				}
				else
				{
					//textWriter.WriteLineIndented(string.Format("Material shader [{0}] is not MarmosetUBER", component.sharedMaterial.shader.name), indentation);
				}
			}
			else
			{
				//textWriter.WriteLineIndented("No Renderer", indentation);
			}
			//Transform transform = gameObject.transform;
			//int childCount = transform.childCount;
			//for (int j = 0; j < childCount; j++)
			//{
			//	MaterialLogger.LogUBERMaterialProperties(transform.GetChild(j).gameObject, textWriter, indentation);
			//}
		}

		void OnConsoleCommand_testassetplatform(NotificationCenter.Notification n)
		{
			Material matToCopy = null;

			{
				GameObject prefab = CraftData.GetPrefabForTechType(TechType.RocketBase);

				GameObject platformBase = prefab.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

				var rends = platformBase.GetComponentsInChildren<Renderer>();

				foreach (Renderer r in rends)
				{
					foreach (Material material in r.materials)
					{
						$"{material.name} {material.shader.name}".log();
					}
				}

				matToCopy = rends[0].materials[6];
			}

			//return;
			(Paths.modRootPath + "bundle").log();

			//foreach (var s in bundle.GetAllAssetNames())
			//	s.log();

			//return;

			//var cube = Instantiate(bundle.LoadAsset<GameObject>("NutrientBlock.prefab"));
			//var cube = Instantiate(bundle.LoadAsset<GameObject>("platform.prefab"));
			var cube = Instantiate(AssetsHelper.loadPrefab("platform.prefab"));

			cube.GetComponentInChildren<MeshRenderer>().renderingLayerMask = 4294967295;
			cube.dump();

			cube.GetFullName().log();
			cube.transform.position = Player.main.transform.position + new Vector3(20f, 20f, 20f);
			//cube.AddComponent<SkyApplier>();
			//cube.transform.localScale = new Vector3(100f, 10f, 100f);

			var renderers = cube.GetComponentsInChildren<Renderer>();

			//foreach (Renderer renderer in renderers)
			//{
			//	foreach (Material material in renderer.materials)
			//	{
			//		$"{material.name} {material.shader.name}".log();
			//	}
			//}

			var shader = Shader.Find("MarmosetUBER");
			renderers[0].materials[6].shader = shader;
			renderers[0].materials[6].CopyPropertiesFromMaterial(matToCopy);

			//foreach (var renderer in renderers)
			//{
			//	foreach (Material mat in renderer.materials)
			//	{
			//		mat.shader = shader;
			//	}
			//}

			//var skyApplier = cube.AddComponent<SkyApplier>();
			//skyApplier.renderers = renderers;
			//skyApplier.anchorSky = Skies.Auto;
		}

		void OnConsoleCommand_testfloor(NotificationCenter.Notification n)
		{
			Material matToCopy = null;

			{
				GameObject prefab = CraftData.GetPrefabForTechType(TechType.RocketBase);

				GameObject platformBase = prefab.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

				var rends = platformBase.GetComponentsInChildren<Renderer>();

				foreach (Renderer r in rends)
				{
					foreach (Material material in r.materials)
					{
						$"{material.name} {material.shader.name}".log();
					}
				}

				matToCopy = rends[0].materials[6];
			}

			var cube = Instantiate(AssetsHelper.loadPrefab("floor.prefab"));

			var renderers = cube.GetComponentsInChildren<Renderer>();

			Vector2 vv = renderers[0].materials[0].mainTextureScale;
			vv.ToString().log();
			renderers[0].materials[0].GetTextureScale("_MainTex").ToString().log();

			Texture tex1 = renderers[0].materials[0].GetTexture("_MainTex");
			Texture tex2 = renderers[0].materials[0].GetTexture("_BumpMap");

			var shader = Shader.Find("MarmosetUBER");
			renderers[0].materials[0].shader = shader;

			_logMaterialProperties(renderers[0].materials[0], "111.txt");

			renderers[0].materials[0].CopyPropertiesFromMaterial(matToCopy);
			_logMaterialProperties(renderers[0].materials[0], "222.txt");

			renderers[0].materials[0].DisableKeyword("UWE_LIGHTMAP");

			renderers[0].materials[0].SetTexture("_MainTex", tex1);
			renderers[0].materials[0].SetTexture("_BumpMap", tex2);
			renderers[0].materials[0].SetTexture("_SpecTex", tex1);

			renderers[0].materials[0].SetTextureScale("_MainTex", vv);
			renderers[0].materials[0].SetTextureScale("_BumpMap", vv);
			renderers[0].materials[0].SetTextureScale("_SpecTex", vv);

			renderers[0].materials[0].GetTextureScale("_MainTex").ToString().log();

			cube.transform.position = Player.main.transform.position + new Vector3(20f, 20f, 20f);

			if (UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject is GameObject platform)
			{
				cube.transform.parent = platform.transform;
				cube.transform.localPosition = new Vector3(0.05f, 2.8629f, 0.065f); // [HabitatPlatform] 01:00:19.474   INFO: pos: 0.1930002 2.8629 0.065
				cube.transform.localScale = new Vector3(42.44f, 0.1f, 34.51f);
				floor = cube;

				//GameObject platformBase = platform.getChild("Base/rocketship_platform/Rocket_Geo/Rocketship_platform/Rocketship_platform_base-1/Rocketship_platform_base_MeshPart0");

				//var rends = platformBase.GetComponentsInChildren<Renderer>();

				//var cubeT = bundle.LoadAsset<GameObject>("Cube_transparent.prefab");

				//"---".log();
				//cubeT.GetComponentsInChildren<Renderer>()[0].materials[0].color.ToString().log();
				//"---".log();
				//rends[0].materials[2] = cubeT.GetComponentsInChildren<Renderer>()[0].materials[0];

				//Shader sh1 = Shader.Find("Transparent/Diffuse");
				//$"------------------ {sh1}".log();

				//rends[0].materials[2].shader = sh1;
				//rends[0].materials[2].color = new Color(1, 0, 0, 0);


				//foreach (Renderer r in rends)
				//{
				//	for (int i = 0; i < r.sharedMaterials.Length; i++)
				//	{
				//		//r.materials[i] = cubeT.GetComponentsInChildren<Renderer>()[0].materials[0];
				//		r.materials[i].shader = sh1;
				//	}
				//}


				//rends[0].materials[2].color = new Color(1, 0, 0, 0);
				//Material mat = rends[0].materials[2];
				//rends[0].materials[2].shader = Shader.Find("Standart");
				//mat.SetColor("_Color", new Color(1, 1, 1, 0));
			}

			//var skyApplier = cube.AddComponent<SkyApplier>();
			//skyApplier.renderers = renderers;
			//skyApplier.anchorSky = Skies.Auto;
		}

		void OnConsoleCommand_movefloor(NotificationCenter.Notification n)
		{
			if (!floor)
				return;

			var pos = floor.transform.localPosition;
			pos += new Vector3(n.getArg<float>(0), n.getArg<float>(1), n.getArg<float>(2)) * Main.config.stepMove;
			floor.transform.localPosition = pos;

			$"{pos.x} {pos.y} {pos.z}".onScreen("pos");
			$"pos: {pos.x} {pos.y} {pos.z}".log();
		}

		void OnConsoleCommand_scalefloor(NotificationCenter.Notification n)
		{
			if (!floor)
				return;

			var pos = floor.transform.localScale;
			pos += new Vector3(n.getArg<float>(0), 0f, n.getArg<float>(1)) * Main.config.stepMove;
			floor.transform.localScale = pos;

			$"{pos.x} {pos.y} {pos.z}".onScreen("scale");
			$"scale: {pos.x} {pos.y} {pos.z}".log();
		}

		void OnConsoleCommand_testblock(NotificationCenter.Notification n)
		{
			var cube = Common.Crafting.CraftHelper.Utils.prefabCopy(TechType.NutrientBlock);

			cube.transform.position = Player.main.transform.position + new Vector3(20f, 20f, -20f);
			cube.transform.localScale = new Vector3(100f, 10f, 100f);

			var renderers = cube.GetComponentsInChildren<Renderer>();

			foreach (Renderer renderer in renderers)
			{
				foreach (Material material in renderer.materials)
				{
					$"{material.name} {material.shader.name}".log();
				}
			}
		}


		void OnConsoleCommand_hbpl_debug(NotificationCenter.Notification n)
		{
			const float delay = 0.5f;

			if (n.getArg<bool>(0))
				StartCoroutine(_dbg());
			else
				StopAllCoroutines();

			static IEnumerator _dbg()
			{
				while (true)
				{
					$"{FindObjectsOfType<Base>().Length}".onScreen("bases count");
					$"{FindObjectsOfType<BaseFoundationPiece>().Length}".onScreen("foundation count");

					yield return new WaitForSeconds(delay);
				}
			}
		}

		void OnConsoleCommand_dbg_base_move(NotificationCenter.Notification n)
		{
			if (UnityHelper.findNearestToCam<Base>()?.gameObject is GameObject baseGo)
			{
				var vecParam = new Vector3(n.getArg<float>(0), n.getArg<float>(1), n.getArg<float>(2));
				baseGo.transform.localPosition += vecParam * Main.config.stepMove;

				$"{baseGo.transform.localPosition}".onScreen("foundation pos");
			}
		}

		void OnConsoleCommand_dbg_platform_move(NotificationCenter.Notification n)
		{
			if (UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject is GameObject platform)
			{
				Vector3 pos = platform.transform.position;
				pos += Main.config.stepMove * n.getArg<float>(0) * (Quaternion.AngleAxis(90, Vector3.up) * platform.transform.forward);
				pos += Main.config.stepMove * n.getArg<float>(1) * (Quaternion.AngleAxis(90, Vector3.up) * platform.transform.right);
				platform.transform.position = pos;
			}
		}

		void OnConsoleCommand_dbg_platform_rotate(NotificationCenter.Notification n)
		{
			if (UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject is GameObject platform)
				platform.transform.rotation *= Quaternion.AngleAxis(Main.config.stepRotate * n.getArg<float>(0), Vector3.up);
		}

		void OnConsoleCommand_dbg_platform_toggle_foundations(NotificationCenter.Notification _)
		{
			if (UnityHelper.findNearestToCam<HabitatPlatform.Tag>()?.gameObject is GameObject platform)
			{
				foreach (var f in platform.GetComponentsInChildren<BaseFoundationPiece>())
				{
					GameObject models = f.gameObject.getChild("models");

					for (int i = 0; i < models.transform.childCount; i++)
					{
						var rend = models.transform.GetChild(i).GetComponent<MeshRenderer>();

						if (rend)
							rend.enabled = !rend.enabled;
					}
				}
			}
		}
	}
}