using System;
using System.IO;
using System.Diagnostics;

namespace Common
{
	static class StringLogExtensions
	{
		static public void log(this string s)			=> Log.msg(s, Log.MsgType.INFO);
		static public void logWarning(this string s)	=> Log.msg(s, Log.MsgType.WARNING);
		static public void logError(this string s)		=> Log.msg(s, Log.MsgType.ERROR);

		[Conditional("DEBUG")]
		static public void logDbg(this string s)		=> Log.msg(s, Log.MsgType.DEBUG);
		
		[Conditional("DEBUG")]
		static public void logDbg(this string s, bool condition) // for removing condition check if !DEBUG
		{
			if (condition)
				Log.msg(s, Log.MsgType.DEBUG);
		}
		
		[Conditional("DEBUG")]
		static public void logDbgError(this string s, bool condition) // for removing condition check if !DEBUG
		{
			if (condition)
				Log.msg(s, Log.MsgType.ERROR);
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

		static string logPrefix = Strings.modName;
#if DEBUG
		static string customLogPath = PathHelpers.ModPath.rootPath + logPrefix + ".log";
		static Log() => File.Delete(customLogPath);
#endif
		static public void msg(string str, MsgType msgType)
		{
			string formattedMsg = $"[{logPrefix}] {msgType}: {str}";
			Console.WriteLine(formattedMsg);
#if DEBUG
				using (StreamWriter writer = File.AppendText(customLogPath))
					writer.WriteLine(formattedMsg);
#endif
		}

		static public void msg(Exception e, string str = "")
		{
			msg(str + "\t" + formatException(e), MsgType.EXCEPTION);
		}

		static string formatException(Exception e)
		{
			return (e != null)? $"{e.GetType()}\tMessage: {e.Message}\n\tStacktrace: {e.StackTrace}\n" + formatException(e.InnerException): string.Empty;
		}
	}
}