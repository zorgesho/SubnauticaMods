using System.Linq;

namespace Common.Configuration.Utils
{
	static class InterleavedParams
	{
		public static void split(object[] iparams, out string[] strings, out object[] values)
		{
			Debug.assert(validate(iparams));

			int length = iparams.Length / 2;
			strings = Enumerable.Range(0, length).Select(i => iparams[i * 2] as string).ToArray();
			values  = Enumerable.Range(0, length).Select(i => iparams[i * 2 + 1]).ToArray();
		}

		static bool validate(object[] iparams)
		{
			if (iparams.Length % 2 != 0)
				return false;

			for (int i = 0; i < iparams.Length; i++)
			{
				if (iparams[i] == null)
					return false;

				var type = iparams[i].GetType();

				if (i % 2 == 0 && type != typeof(string))
					return false;

				if (i % 2 == 1 && !type.IsEnum && type != typeof(int) && type != typeof(float))
					return false;
			}

			return true;
		}
	}
}