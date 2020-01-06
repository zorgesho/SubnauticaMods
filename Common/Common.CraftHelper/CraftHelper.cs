using System;
using System.Collections.Generic;

namespace Common.Crafting
{
	static class CraftHelper
	{
		// classes with this attribute will not be patched by patchAll
		[AttributeUsage(AttributeTargets.Class, Inherited = false)]
		public class NoAutoPatchAttribute: Attribute {}

		// classes with this attribute will be patched by patchAll before classes without it
		[AttributeUsage(AttributeTargets.Class, Inherited = false)]
		public class PatchFirstAttribute: Attribute {}

		static bool allPatched = false;

		public static void patchAll()
		{
			if (allPatched)
				return;

			allPatched = true;

			List<Type> toPatch = new List<Type>();

			foreach (var type in ReflectionHelper.definedTypes)
			{
				if (typeof(CraftableObject).IsAssignableFrom(type) && Attribute.GetCustomAttribute(type, typeof(NoAutoPatchAttribute)) == null)
				{
					if (Attribute.GetCustomAttribute(type, typeof(PatchFirstAttribute)) != null)
						patchObject(type);
					else
						toPatch.Add(type);
				}
			}

			toPatch.ForEach(type => patchObject(type));
		}

		static void patchObject(Type type)
		{																						$"CraftHelper: patching {type}".logDbg();
			(Activator.CreateInstance(type) as CraftableObject)?.patch();
		}
	}
}