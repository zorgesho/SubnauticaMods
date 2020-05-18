using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Runtime.CompilerServices // nice trick (for use in .NET 4.0)
{
	// https://thomaslevesque.com/2012/06/13/using-c-5-caller-info-attributes-when-targeting-earlier-versions-of-the-net-framework/

	[AttributeUsage(AttributeTargets.Parameter)] class CallerFilePathAttribute: Attribute {}
	[AttributeUsage(AttributeTargets.Parameter)] class CallerMemberNameAttribute: Attribute {}
	[AttributeUsage(AttributeTargets.Parameter)] class CallerLineNumberAttribute: Attribute {}
}

namespace Common
{
	using Reflection;

	static partial class Debug
	{
		public static void logStack(string msg = "")
		{
			var stackFrames = new StackTrace().GetFrames();
			var sb = new StringBuilder($"Callstack {msg}:{Environment.NewLine}");

			for (int i = 1; i < stackFrames.Length; i++) // don't print first item, it is "logStack"
				sb.AppendLine($"\t{stackFrames[i].GetMethod().fullName()}");

			sb.ToString().log();
		}

		[Conditional("DEBUG")]
		public static void assert(bool condition, string message = null, [CallerFilePath] string __filename = "", [CallerLineNumber] int __line = 0)
		{
			if (condition)
				return;

			string msg = "Assertion failed" + (message != null? $": {message}": "") + $" ({__filename}:{__line})";

			$"{msg}".logError();
			throw new Exception(msg);
		}
	}
}