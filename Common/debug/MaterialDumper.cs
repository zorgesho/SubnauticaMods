using System.Text;
using UnityEngine;

namespace Common
{
	static partial class Debug
	{
		public static void dump(this Material material, string filename = null)
		{
			MaterialDumper.dump(material, filename);
		}

		static class MaterialDumper
		{
			// based on code from https://github.com/brett-taylor/SubnauticaRuntimeUnityEditor/ and https://github.com/Fenolphthalien/BaseClocks/
			public static void dump(Material material, string filename = null)
			{
				if (material?.shader != Shader.Find("MarmosetUBER"))
					return;

				StringBuilder sb = new();

				void _add(string name, string val) => sb.AppendLine($"{name} = {val}");

				void _addInt(string name)	  => _add(name, $"{material.GetInt(name)}");
				void _addBool(string name)	  => _add(name, $"{material.GetFloat(name) == 1f}");
				void _addColor(string name)	  => _add(name, $"{material.GetColor(name)}");
				void _addFloat(string name)	  => _add(name, $"{material.GetFloat(name):F4}");
				void _addVector(string name)  => _add(name, material.GetVector(name).ToString("F4"));
				void _addTexture(string name) => _add(name, $"{material.GetTexture(name)?.name ?? "[null]"} (scale: {material.GetTextureScale(name).ToString("F2")})");

				void _addBools(params string[] names)	 => names.forEach(_addBool);
				void _addColors(params string[] names)	 => names.forEach(_addColor);
				void _addFloats(params string[] names)	 => names.forEach(_addFloat);
				void _addVectors(params string[] names)	 => names.forEach(_addVector);
				void _addTextures(params string[] names) => names.forEach(_addTexture);


				sb.AppendLine("Material properties:");

				sb.Append("Keywords:");
				material.shaderKeywords.forEach(word => sb.Append(" " + word));
				sb.AppendLine();

				sb.AppendLine("Textures:");
				_addTextures("_MainTex", "_BumpMap", "_Lightmap", "_SIGMap", "_SpecTex", "_Illum");
				sb.AppendLine();

				_addInt("_MarmoSpecEnum");
				_addColors("_Color", "_Color2", "_Color3");
				_addFloats("_Mode", "_Fresnel", "_Shininess", "_SpecInt");
				_addBools("_EnableGlow", "_EnableLighting", "_Enable3Color");
				_addColors("_SpecColor", "_SpecColor2", "_SpecColor3");
				_addFloats("_SrcBlend", "_DstBlend", "_SrcBlend2", "_DstBlend2");
				_addFloats("_AddSrcBlend", "_AddDstBlend", "_AddSrcBlend2", "_AddDstBlend2");

				_addBools("_EnableMisc", "_ZWrite");
				_addFloats("_ZOffset", "_IBLreductionAtNight");
				_addBools("_Cutoff", "_EnableCutOff", "_EnableDitherAlpha", "_EnableVrFadeOut");
				_addBools("_EnableSimpleGlass", "_EnableVertexColor", "_EnableSchoolFish", "_EnableMainMaps");

				_addColor("_GlowColor");
				_addBool("_GlowUVfromVC");
				_addFloats("_GlowStrength", "_GlowStrengthNight", "_EmissionLM", "_EmissionLMNight");

				_addBools("_EnableDetailMaps", "_EnableLightmap");
				_addVector("_DetailIntensities");
				_addFloat("_LightmapStrength");
				_addVector("_DeformParams");
				_addFloats("_FillSack", "_OverlayStrength", "_Hypnotize");
				_addColors("_GlowScrollColor", "_ScrollColor");
				_addVectors("_ColorStrength", "_GlowMaskSpeed", "_ScrollSpeed");
				_addColors("_DetailsColor", "_SquaresColor", "_BorderColor");
				_addFloats("_SquaresTile", "_SquaresSpeed", "_SquaresIntensityPow");
				_addVectors("_NoiseSpeed", "_FakeSSSparams", "_FakeSSSSpeed");
				_addFloats("_Built", "_BuildLinear", "_NoiseThickness", "_NoiseStr");
				_addVectors("_BuildParams", "_Scale", "_Frequency", "_Speed", "_ObjectUp");
				_addFloats("_WaveUpMin", "_Fallof", "_RopeGravity", "_minYpos", "_maxYpos");
				_addBool("_EnableBurst");
				_addFloats("_Displacement", "_BurstStrength");
				_addVector("_Range");
				_addFloat("_ClipRange");

				_addBools("_EnableInfection", "_EnablePlayerInfection");
				_addFloat("_InfectionHeightStrength");
				_addVectors("_InfectionScale", "_InfectionOffset", "_InfectionSpeed");

				_addFloat("_MyCullVariable");

				sb.ToString().saveToFile(filename ?? material.name);
			}
		}
	}
}