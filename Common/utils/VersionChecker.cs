using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;

using UnityEngine;

namespace Common.Utils
{
	using Reflection;

	struct Version
	{
		const char separator = '.';

		public readonly int major, minor, patch;

		public Version(int major, int minor, int patch)
		{
			this.major = major;
			this.minor = minor;
			this.patch = patch;
		}

		public Version(string version)
		{
			var v = version.Split(separator);
			Debug.assert(v.Length >= 3);

			major = v[0].convert<int>();
			minor = v[1].convert<int>();
			patch = v[2].convert<int>();
		}

		public static bool operator ==(Version a, Version b) => a.compareTo(b) == 0;
		public static bool operator !=(Version a, Version b) => a.compareTo(b) != 0;

		public static bool operator >(Version a, Version b) => a.compareTo(b) == 1;
		public static bool operator <(Version a, Version b) => a.compareTo(b) == -1;

		int compareTo(Version v)
		{
			if (major != v.major) return major > v.major? 1: -1;
			if (minor != v.minor) return minor > v.minor? 1: -1;
			if (patch != v.patch) return patch > v.patch? 1: -1;

			return 0;
		}

		public override bool Equals(object obj) => obj is Version ver && this == ver;
		public override int  GetHashCode() => (major & 0xFF) << 16 | (minor & 0xFF) << 8 | (patch & 0xFF);

		public override string ToString() => $"{major}{separator}{minor}{separator}{patch}";
	}


	static class VersionChecker
	{
		const float checkDelaySecs		= Mod.Consts.isDevBuild? 0f: 15f;
		const float checkDelayRangeSecs = Mod.Consts.isDevBuild? 0f: 60f;
		const float checkPeriodHours	= Mod.Consts.isDevBuild? 0f: 1f;

		static readonly string versionFilePath = Paths.modRootPath + "latest-version.txt";

		static string versionURL;
		static GameObject go;

		public static Version getLatestVersion(string url)
		{
			init(url);

			try   { return File.Exists(versionFilePath)? new Version(File.ReadAllText(versionFilePath)): default; }
			catch { return default; }
		}

		static void init(string url)
		{
			versionURL = url;

			if (!url.isNullOrEmpty() && !go)
				go = UnityHelper.createPersistentGameObject<CheckVersion>($"{Mod.id}.VersionChecker");
		}

		class CheckVersion: MonoBehaviour
		{
			IEnumerator Start()
			{
				yield return new WaitForSeconds(checkDelaySecs + UnityEngine.Random.Range(0f, checkDelayRangeSecs));

				var thread = new Thread(checkVersion);
				thread.Start();

				yield return new WaitWhile(() => thread.IsAlive);

				Destroy(gameObject);
			}

			void OnDestroy() => "VersionChecker: done".logDbg();

			void checkVersion()
			{																								"VersionChecker: start".logDbg();
				try
				{
					// checking version file's timestamp
					if (File.Exists(versionFilePath) && DateTime.Now.Subtract(File.GetLastWriteTime(versionFilePath)).TotalHours < checkPeriodHours)
						return;

					// downloading mod.json and updating version file
					using var client = new WebClient();

					var manifest = SimpleJSON.Parse(client.DownloadString(versionURL));
					var version = new Version(manifest["Version"]);											$"VersionChecker: latest version retrieved: {version}".logDbg();

					File.WriteAllText(versionFilePath, version.ToString()); // using Version to make sure it's valid version string
				}
				catch (Exception e) { $"Error while trying to check for update: {e.Message}".logError(); }
			}
		}
	}
}