using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

namespace Common
{
	static partial class StringExtensions
	{
		static public void log(this string s)			=> Log.msg(s, Log.MsgType.INFO);
		static public void logWarning(this string s)	=> Log.msg(s, Log.MsgType.WARNING);
		static public void logError(this string s)		=> Log.msg(s, Log.MsgType.ERROR);

		[Conditional("TRACE")]
		static public void logDbg(this string s)		=> Log.msg(s, Log.MsgType.DEBUG);
		
		[Conditional("TRACE")]
		static public void logDbg(this string s, bool condition) // for removing condition check if !TRACE
		{
			if (condition)
				Log.msg(s, Log.MsgType.DEBUG);
		}
		
		[Conditional("TRACE")]
		static public void logDbgError(this string s, bool condition) // for removing condition check if !TRACE
		{
			if (condition)
				Log.msg(s, Log.MsgType.ERROR);
		}

		[Conditional("TRACE")]
		static public void logDbg(this List<string> strings, string msg = "")
		{
			strings.ForEach(s => Log.msg(msg + s, Log.MsgType.DEBUG));
		}
	}


	static class Log
	{
		public enum MsgType
		{
			DEBUG,
			INFO,
			WARNING,
			ERROR,
			EXCEPTION
		}

		static readonly string logPrefix = Strings.modName;
#if TRACE
		static readonly string customLogPath = Paths.modRootPath + logPrefix + ".log";
		static Log()
		{
			try
			{
				File.Delete(customLogPath);
			}
			catch (UnauthorizedAccessException) {}
		}
#endif
		static public void msg(string str, MsgType msgType)
		{
			string formattedMsg = $"[{logPrefix}] {msgType}: {str}";
			Console.WriteLine(formattedMsg);
#if TRACE
			try
			{
				using (StreamWriter writer = File.AppendText(customLogPath))
					writer.WriteLine(formattedMsg);
			}
			catch (UnauthorizedAccessException) {}
#endif
		}

		static public void msg(Exception e, string str = "")
		{
			msg(str + (str == ""? "": "\t") + formatException(e), MsgType.EXCEPTION);
		}

		static string formatException(Exception e)
		{
			return (e != null)? $"{e.GetType()}\tMESSAGE: {e.Message}\n\tSTACKTRACE:\n{e.StackTrace}\n" + formatException(e.InnerException): "";
		}
	}
}