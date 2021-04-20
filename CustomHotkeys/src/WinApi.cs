using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if DEBUG
using Common;
#endif

namespace CustomHotkeys
{
	static class WinApi
	{
		public static void startProcess(string filename, string args)
		{
			var process = new Process();

			process.StartInfo.FileName = filename;
			process.StartInfo.Arguments = args;
			process.Start();
		}

		public static string getExecutableByExtension(string ext)
		{
			string app = assocQueryString(AssocStr.DDEApplication, ext);
			return (app == "shell32")? null: assocQueryString(AssocStr.Executable, ext);
		}

		public static void setWindowPos(int x, int y)
		{
			SetWindowPos(GetActiveWindow(), 0, x, y, 0, 0, 0x0001);
		}

#if DEBUG
		public static void logAccos(string ext)
		{
			$"Extension associations for '{ext}'".log();
			foreach (var assoc in Enum.GetValues(typeof(AssocStr)))
				$"{assoc}: '{assocQueryString((AssocStr)assoc, "." + ext)}'".log();
		}
#endif
		static string assocQueryString(AssocStr assocStr, string extension)
		{
			const int S_OK = 0, S_FALSE = 1;

			uint length = 0;
			uint ret = AssocQueryString(AssocFlags.None, assocStr, extension, null, null, ref length);
			if (ret != S_FALSE)
				return null;

			var sb = new StringBuilder((int)length);
			ret = AssocQueryString(AssocFlags.None, assocStr, extension, null, sb, ref length);
			if (ret != S_OK)
				return null;

			return sb.ToString();
		}

		#region winapi exports
		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		[DllImport("user32.dll")]
		static extern IntPtr GetActiveWindow();

		//https://stackoverflow.com/questions/162331/finding-the-default-application-for-opening-a-particular-file-type-on-windows
		[DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
		static extern uint AssocQueryString(
			AssocFlags flags,
			AssocStr str,
			string pszAssoc,
			string pszExtra,
			[Out] StringBuilder pszOut,
			ref uint pcchOut);

		[Flags]
		enum AssocFlags
		{
			None = 0,
			Init_NoRemapCLSID = 0x1,
			Init_ByExeName = 0x2,
			Open_ByExeName = 0x2,
			Init_DefaultToStar = 0x4,
			Init_DefaultToFolder = 0x8,
			NoUserSettings = 0x10,
			NoTruncate = 0x20,
			Verify = 0x40,
			RemapRunDll = 0x80,
			NoFixUps = 0x100,
			IgnoreBaseClass = 0x200,
			Init_IgnoreUnknown = 0x400,
			Init_Fixed_ProgId = 0x800,
			Is_Protocol = 0x1000,
			Init_For_File = 0x2000
		}

		enum AssocStr
		{
			Command = 1,
			Executable,
			FriendlyDocName,
			FriendlyAppName,
			NoOpen,
			ShellNewValue,
			DDECommand,
			DDEIfExec,
			DDEApplication,
			DDETopic,
			InfoTip,
			QuickTip,
			TileInfo,
			ContentType,
			DefaultIcon,
			ShellExtension,
			DropTarget,
			DelegateExecute,
			Supported_Uri_Protocols,
			ProgID,
			AppID,
			AppPublisher,
			AppIconReference,
			Max
		}
		#endregion
	}
}