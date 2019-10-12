using System;
using System.Reflection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Code Quality", "IDE0051", Scope = "namespaceanddescendants", Target = "Common")]
[assembly: SuppressMessage("Code Quality", "IDE0060", Scope = "namespaceanddescendants", Target = "Common")]

namespace Common
{
	static class Debug
	{
		// based on code from http://www.csharp-examples.net/reflection-callstack/
		static public void logStack(string msg = "")
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame[] stackFrames = stackTrace.GetFrames();
			
			string output = $"Callstack {msg}:" + Environment.NewLine;

			for (int i = 1; i < stackFrames.Length; ++i) // dont print first item, it is "printStack"
			{
				MethodBase method = stackFrames[i].GetMethod();
				output += $"\t{method.DeclaringType.Name}.{method.Name}" + Environment.NewLine;
			}

			output.log();
		}
	}
}
