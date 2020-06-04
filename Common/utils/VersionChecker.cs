using System;
using System.IO;
using System.Net;
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
		public static bool operator !=(Version a, Version b) => !(a == b);

		public static bool operator >(Version a, Version b) => a.compareTo(b) == 1;
		public static bool operator <(Version a, Version b) => a.compareTo(b) == -1;

		int compareTo(Version v)
		{
			if (major != v.major) return major > v.major? 1: -1;
			if (minor != v.minor) return minor > v.minor? 1: -1;
			if (patch != v.patch) return patch > v.patch? 1: -1;

			return 0;
		}

		public override bool Equals(object obj) => obj is Version? this == (Version)obj: false;
		public override int  GetHashCode() => (major & 0xFF) << 16 | (minor & 0xFF) << 8 | (patch & 0xFF);

		public override string ToString() => $"{major}{separator}{minor}{separator}{patch}";
	}


	static class VersionChecker
	{
		const float checkDelaySecs = Mod.isDevBuild? 0f: 15f;
		const float checkDelayRangeSecs = Mod.isDevBuild? 0f: 30f;

		const float checkPeriodHours = Mod.isDevBuild? 0f: 1f;

		const string filenameVersion = "latest-version.txt";

		static string versionURL;
		static GameObject versionChecker;

		public static Version getLatestVersion(string url)
		{
			init(url);

			string path = Paths.modRootPath + filenameVersion;
			return File.Exists(path)? new Version(File.ReadAllText(path)): default;
		}

		static void init(string url)
		{
			versionURL = url;

			if (!url.isNullOrEmpty() && !versionChecker)
				versionChecker = UnityHelper.createPersistentGameObject<CheckVersion>("VersionChecker." + Mod.id);
		}

		static bool checkConnection()
		{
			try
			{
				using (var client = new WebClient())
					using (client.OpenRead("http://google.com/generate_204"))
						return true;
			}
			catch { return false; }
		}

		class CheckVersion: MonoBehaviour
		{
			IEnumerator Start()
			{
				yield return new WaitForSeconds(checkDelaySecs + UnityEngine.Random.Range(0f, checkDelayRangeSecs));

				if (!checkVersion())
					stop();
			}

			void stop() => Destroy(gameObject);

			bool checkVersion()
			{																								"VersionChecker: start checking".logDbg();
				string path = Paths.modRootPath + filenameVersion;

				if (File.Exists(path))
				{
					TimeSpan sinceLastCheck = DateTime.Now.Subtract(File.GetLastWriteTime(path));

					if (sinceLastCheck.TotalHours < checkPeriodHours)
						return false;
				}

				if (!checkConnection())
					return false;

				using var client = new WebClient();

				client.DownloadStringCompleted += (sender, e) =>
				{
					// TODO check for errors and such

					var node = SimpleJSON.Parse(e.Result);
					string version = node["Version"];														$"VersionChecker: latest version retrieved: {version}".logDbg();
					File.WriteAllText(path, version);

					stop();
				};

				client.DownloadStringAsync(new Uri(versionURL));
				return true;
			}

			void OnDestroy() => "VersionChecker: stopped".logDbg();
		}
	}
}