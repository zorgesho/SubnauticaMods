using System;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using Harmony;

namespace Common
{
	static class HarmonyHelper
	{
		#region Public interface

		static public HarmonyInstance harmonyInstance { get; private set; } = null;

		// expected to called only from mod entry function
		static public void patchAll()
		{
			findConfig("Main", "config"); // need to be called before harmony patching

			harmonyInstance = HarmonyInstance.Create(Strings.modName);
			harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}

		// for using in transpilers
		static public IEnumerable<CodeInstruction> changeConstToConfigVar<T>(IEnumerable<CodeInstruction> instructions, T val, string configVar)
		{
			FieldInfo varField = mainConfigField?.FieldType.GetField(configVar, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (varField == null)
				$"changeConstToConfigVar: varField for {configVar} is not found".logError();

			return varField != null? changeConstToVar(instructions, val, mainConfigField, varField): null;
		}

		// dynamic patching/unpatching, for use with OptionalPatch attribute
		static public void setPatchEnabled(bool val, Type type)
		{
			if (Attribute.GetCustomAttribute(type, typeof(OptionalPatchAttribute)) is OptionalPatchAttribute patchAttribute)
				patchAttribute.setEnabled(val, type);
		}

		
		[AttributeUsage(AttributeTargets.Class)]
		public class OptionalPatchAttribute: Attribute
		{
			const BindingFlags bf = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
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


		#region Private stuff
		static FieldInfo mainConfigField = null; // for using in transpiler helper functions
		
		static void findConfig(string mainClassName, string configFieldName)
		{
			string modNamespace = new StackTrace().GetFrame(2).GetMethod().ReflectedType.Namespace; // expected to called only from patchAll

			Type mainType = Assembly.GetExecutingAssembly().GetType(modNamespace + "." + mainClassName);

			mainConfigField = mainType?.GetField(configFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

			if (mainConfigField == null)
				"HarmonyHelper: main config was not found".logWarning();
		}

		// Clearly an overkill, simple typeof() comparision would be enough, just wanted to test template specialization in C#
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

			static public OpCode get<T>() => GetOpCode<T>.S.get();
		}


		static IEnumerable<CodeInstruction> changeConstToVar<T>(IEnumerable<CodeInstruction> instructions,
																 T val,
																 FieldInfo staticObject,
																 FieldInfo objectField)
		{																												"HarmonyHelper.changeConstToVar".logDbg();
			bool injected = false;
			OpCode opcodeToCheck = OpCodeByType.get<T>();																$"HarmonyHelper.changeConstToVar: searching opcode:{opcodeToCheck}, val:{val}".logDbg();

			foreach (var instruction in instructions)
			{																											
				if (!injected && instruction.opcode.Equals(opcodeToCheck) && instruction.operand.Equals(val))
				{																										"HarmonyHelper.changeConstToVar: injected".logDbg();
					injected = true;
					yield return new CodeInstruction(OpCodes.Ldsfld, staticObject);
					yield return new CodeInstruction(OpCodes.Ldfld, objectField);

					continue;
				}
				
				yield return instruction;
			}
		}
		#endregion
	}
}