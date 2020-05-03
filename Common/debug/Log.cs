using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Common
{
	static partial class StringExtensions
	{
		// can be used in conditions -> if (checkForError() && "write error to log".logError()) return;
		public static bool log(this string s)			{ Log.msg(s, Log.MsgType.INFO);		return true; }
		public static bool logWarning(this string s)	{ Log.msg(s, Log.MsgType.WARNING);	return true; }
		public static bool logError(this string s)		{ Log.msg(s, Log.MsgType.ERROR);	return true; }

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

		[Conditional("TRACE")]
		public static void logDbg(this List<string> strings, string msg = "") =>
			strings.ForEach(s => Log.msg(msg + s, Log.MsgType.DBG));
	}


	static class Log
	{
		public enum MsgType { DBG, INFO, WARNING, ERROR, EXCEPTION }

		static readonly string logPrefix = Mod.id;
#if DEBUG
		static readonly string customLogPath = Paths.modRootPath + logPrefix + ".log";
		static Log()
		{
			try { File.Delete(customLogPath); }
			catch (UnauthorizedAccessException) {}
		}
#endif
		public static void msg(string str, MsgType msgType)
		{
			string formattedMsg = $"[{logPrefix}] {DateTime.Now.ToString("HH:mm:ss.fff")}   {msgType}: {str}{Environment.NewLine}";
			Console.Write(formattedMsg);
#if DEBUG
			try { File.AppendAllText(customLogPath, formattedMsg); }
			catch (UnauthorizedAccessException) {}
#endif
		}

		public static void msg(Exception e, string str = "", bool verbose = true) =>
			msg($"{str}{(str == ""? "": ": ")}{(verbose? formatException(e): e.Message)}", MsgType.EXCEPTION);

		static string formatException(Exception e) =>
			(e == null)? "": $"\r\n{e.GetType()}: {e.Message}\r\nSTACKTRACE:\r\n{e.StackTrace}\r\n" + formatException(e.InnerException);
	}
}