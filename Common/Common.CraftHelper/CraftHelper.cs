using System;
using System.Reflection;

namespace Common.Crafting
{
	static class CraftHelper
	{
		[AttributeUsage(AttributeTargets.Class, Inherited = false)]
		public class NoAutoPatchAttribute: Attribute {}

		static bool allPatched = false;
		
		public static void patchAll()
		{
			if (allPatched)
				return;

			foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
			{
				if (typeof(CraftableObject).IsAssignableFrom(type) && Attribute.GetCustomAttribute(type, typeof(NoAutoPatchAttribute)) == null)
				{																																		$"CraftHelper: patching {type}".logDbg();
					(Activator.CreateInstance(type) as CraftableObject)?.patch();
				}
			}

			allPatched = true;
		}
	}
}