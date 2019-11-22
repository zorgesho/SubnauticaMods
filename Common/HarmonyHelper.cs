using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using UnityEngine;
using Harmony;

namespace Common
{
	using Instructions = IEnumerable<CodeInstruction>;

	static class HarmonyHelper
	{
		#region Public interface
		public static HarmonyInstance harmonyInstance { get; private set; } = null;

		// expected to called only from mod entry function
		public static void patchAll(bool searchConfig = true)
		{
			if (searchConfig)
				findConfig("Main", "config"); // need to be called before harmony patching

			harmonyInstance = HarmonyInstance.Create(Strings.modName);
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}

		
		// dynamic patching/unpatching, for use with OptionalPatch attribute
		public static void setPatchEnabled(bool val, Type type)
		{
			if (Attribute.GetCustomAttribute(type, typeof(OptionalPatchAttribute)) is OptionalPatchAttribute patchAttribute)
				patchAttribute.setEnabled(val, type);
		}


		public static bool isLDC<T>(this CodeInstruction instruction, T val)
		{
			return instruction.opcode.Equals(OpCodeByType.get<T>()) && instruction.operand.Equals(val);
		}
		
		
		[AttributeUsage(AttributeTargets.Class)]
		public class OptionalPatchAttribute: Attribute
		{
			static readonly BindingFlags bf = _BindingFlags.all;
			readonly MethodInfo method;

			public OptionalPatchAttribute(Type type, string methodName)
			{
				method = type.GetMethod(methodName, bf);								$"OptionalPatchAttribute {type} {methodName}".logDbg();
			}

			public void setEnabled(bool val, Type type)
			{
				if (method == null)
				{
					$"OptionalPatchAttribute method is null!".logError();
					return;
				}

				var prefix = type.GetMethod("Prefix", bf);
				var postfix = type.GetMethod("Postfix", bf);
				var transpiler = type.GetMethod("Transpiler", bf);

				if (val)
				{
					Patches patches = harmonyInstance.GetPatchInfo(method);

					bool patched =  patches != null && (patches.Prefixes.FirstOrDefault(p => p.patch == prefix) != null ||
														patches.Postfixes.FirstOrDefault(p => p.patch == postfix) != null ||
														patches.Transpilers.FirstOrDefault(p => p.patch == transpiler) != null);

					$"OptionalPatchAttribute {method} is already patched".logDbg(patched);

					if (!patched)
						harmonyInstance.Patch(method,	(prefix == null)? null: new HarmonyMethod(prefix),
														(postfix == null)? null: new HarmonyMethod(postfix),
														(transpiler == null)? null: new HarmonyMethod(transpiler));
				}
				else
				{
					if (prefix != null)
						harmonyInstance.Unpatch(method, prefix);
				
					if (postfix != null)
						harmonyInstance.Unpatch(method, postfix);
				
					if (transpiler != null)
						harmonyInstance.Unpatch(method, transpiler);
				}
			}
		}
		#endregion

		
		static FieldInfo mainConfigField = null; // for using in transpiler helper functions
		
		static void findConfig(string mainClassName, string configFieldName)
		{
			string modNamespace = new StackTrace().GetFrame(2).GetMethod().ReflectedType.Namespace; // expected to called only from patchAll

			Type mainType = Assembly.GetExecutingAssembly().GetType(modNamespace + "." + mainClassName);

			mainConfigField = mainType?.GetField(configFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			if (mainConfigField == null)
				"HarmonyHelper: main config was not found".logWarning();
		}

		// Clearly an overkill, just wanted to test template specialization in C#
		// https://stackoverflow.com/questions/600978/how-to-do-template-specialization-in-c-sharp
		static class OpCodeByType
		{
			interface IGetOpCode<T> { OpCode get(); }

			class GetOpCode<T>: IGetOpCode<T>
			{
				class GetOpSpec: IGetOpCode<float>, IGetOpCode<sbyte>
				{
					public static readonly GetOpSpec S = new GetOpSpec();

					OpCode IGetOpCode<float>.get() => OpCodes.Ldc_R4;
					OpCode IGetOpCode<sbyte>.get() => OpCodes.Ldc_I4_S;
				}

				public static readonly IGetOpCode<T> S = GetOpSpec.S as IGetOpCode<T> ?? new GetOpCode<T>();

				OpCode IGetOpCode<T>.get() => OpCodes.Nop;
			}

			public static OpCode get<T>() => GetOpCode<T>.S.get();
		}

		public static Instructions changeConstToConfigVar<T>(Instructions ins, T val, string configVar)
		{
			bool injected = false;

			foreach (var i in ins)
			{
				if (!injected && i.isLDC(val))
				{																												"HarmonyHelper.changeConstToConfigVar: injected".logDbg();
					injected = true;

					foreach (var j in _codeForChangeConstToConfigVar(configVar))
						yield return j;

					continue;
				}

				yield return i;
			}
		}


		public static Instructions _codeForChangeConstToConfigVar(string configVar)
		{
			FieldInfo varField = mainConfigField?.FieldType.GetField(configVar, _BindingFlags.all);

			if (varField == null)
			{
				$"changeConstToConfigVar: varField for {configVar} is not found".logError();
				yield break;
			}

			yield return new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			yield return new CodeInstruction(OpCodes.Ldfld, varField);
		}


		public static Instructions _codeForChangeConstToConfigMethodCall(string configMethod)
		{
			MethodInfo method = mainConfigField?.FieldType.GetMethod(configMethod, _BindingFlags.all);
			
			if (method == null)
			{
				$"changeConstToConfigMethodCall: method '{configMethod}' is not found".logError();
				yield break;
			}

			yield return new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			yield return new CodeInstruction(OpCodes.Callvirt, method);
		}

		// changing constant to config field if gameobject have component C
		public static Instructions changeConstToConfigVar<T, C>(Instructions ins, T val, string configVar, ILGenerator ilg) where C: Component
		{
			bool injected = false;																						"HarmonyHelper.changeConstToVar".logDbg();

			foreach (var i in ins)
			{																											
				if (!injected && i.isLDC(val))
				{																										"HarmonyHelper.changeConstToVar: injected".logDbg();
					injected = true;

					foreach (var j in _codeForChangeConstToConfigVar<T, C>(val, configVar, ilg))
						yield return j;

					continue;
				}

				yield return i;
			}
		}


		public static Instructions _codeForChangeConstToConfigVar<T, C>(T value, string configVar, ILGenerator ilg) where C: Component
		{
			FieldInfo varField = mainConfigField?.FieldType.GetField(configVar, _BindingFlags.all);

			if (varField == null)
			{
				$"changeConstToConfigVar: varField for {configVar} is not found".logError();
				yield break;
			}

			Label lb1 = ilg.DefineLabel();
			Label lb2 = ilg.DefineLabel();

			yield return new CodeInstruction(OpCodes.Ldarg_0);
			yield return new CodeInstruction(OpCodes.Callvirt,
				AccessTools.Method(typeof(Component), "GetComponent").MakeGenericMethod(typeof(C)));

			yield return new CodeInstruction(OpCodes.Ldnull);
			yield return new CodeInstruction(OpCodes.Call,
				AccessTools.Method(typeof(UnityEngine.Object), "op_Inequality"));

			yield return new CodeInstruction(OpCodes.Brtrue_S, lb1);
			yield return new CodeInstruction(OpCodeByType.get<T>(), value);
			yield return new CodeInstruction(OpCodes.Br_S, lb2);

			var ci1 = new CodeInstruction(OpCodes.Ldsfld, mainConfigField);
			ci1.labels.Add(lb1);
			yield return ci1;

			yield return new CodeInstruction(OpCodes.Ldfld, varField);

			var ci2 = new CodeInstruction(OpCodes.Nop);
			ci2.labels.Add(lb2);
			yield return ci2;
		}
	}
}