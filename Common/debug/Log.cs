using System;
using System.Diagnostics;

#if DEBUG
using System.IO;
#endif

namespace Common
{
	static partial class StringExtensions
	{
		// can be used in conditions -> if (checkForError() && "write error to log".logError()) return;
		public static bool log(this string s)		 { Log.msg(s, Log.MsgType.INFO);	return true; }
		public static bool logWarning(this string s) { Log.msg(s, Log.MsgType.WARNING);	return true; }
		public static bool logError(this string s)	 { Log.msg(s, Log.MsgType.ERROR);	return true; }

		[Conditional("TRACE")]
		public static void logDbg(this string s) => Log.msg(s, Log.MsgType.DBG);

		[Conditional("TRACE")]
		public static void logDbg(this string s, bool condition) // for removing condition check if !TRACE
		{
			if (condition)
				Log.msg(s, Log.MsgType.DBG);
		}

		[Conditional("TRACE")]
		public static void logDbgError(this string s, bool condition) // for removing condition check if !TRACE
		{
			if (condition)
				Log.msg(s, Log.MsgType.ERROR);
		}
	}


	static class Log
	{
		public enum MsgType { DBG, INFO, WARNING, ERROR, EXCEPTION }

		static readonly string logPrefix = Mod.id;
#if DEBUG
		static readonly string customLogPath = Paths.modRootPath + logPrefix + ".log";
		static readonly string warningsLogPath = Path.Combine(Paths.modRootPath, "..", "warnings.log"); // consolidated log for warnings and up (in QMods root)
		static Log()
		{
			try { File.Delete(customLogPath); }
			catch (UnauthorizedAccessException) {}
		}
#endif
		public static void msg(string str, MsgType msgType)
		{
			string currentFrame = Mod.isShuttingDown? "": $" [{UnityEngine.Time.frameCount}]"; // we can't access to Time class while in shutdown state
			string formattedMsg = $"[{logPrefix}] {DateTime.Now:HH:mm:ss.fff}{currentFrame}  {msgType}: {str}{Environment.NewLine}";
			Console.Write(formattedMsg);
#if DEBUG
			try
			{
				File.AppendAllText(customLogPath, formattedMsg);

				if (msgType >= MsgType.WARNING)
					File.AppendAllText(warningsLogPath, formattedMsg);
			}
			catch (UnauthorizedAccessException) {}
#endif
		}

		public static void msg(Exception e, string str = "", bool verbose = true) =>
			msg($"{str}{(str == ""? "": ": ")}{(verbose? formatException(e): e.Message)}", MsgType.EXCEPTION);

		static string formatException(Exception e) =>
			(e == null)? "": $"\r\n{e.GetType()}: {e.Message}\r\nSTACKTRACE:\r\n{e.StackTrace}\r\n" + formatException(e.InnerException);
	}
}