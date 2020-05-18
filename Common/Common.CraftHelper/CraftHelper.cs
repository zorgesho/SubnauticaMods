using System;
using System.Collections.Generic;

namespace Common.Crafting
{
	using Reflection;

	static partial class CraftHelper
	{
		// classes with this attribute will not be patched by patchAll
		[AttributeUsage(AttributeTargets.Class, Inherited = false)]
		public class NoAutoPatchAttribute: Attribute {}

		// classes with this attribute will be patched by patchAll before classes without it
		[AttributeUsage(AttributeTargets.Class, Inherited = false)]
		public class PatchFirstAttribute: Attribute {}

		static bool allPatched = false;

		// search for classes derived from CraftableObject and patch them
		// can be controlled by using attributes above ^
		public static void patchAll()
		{
			if (allPatched || !(allPatched = true))
				return;

			var toPatch = new List<Type>();

			foreach (var type in ReflectionHelper.definedTypes)
			{
				if (!typeof(CraftableObject).IsAssignableFrom(type) || type.checkAttr<NoAutoPatchAttribute>())
					continue;

				if (type.checkAttr<PatchFirstAttribute>())
					patchClass(type);
				else
					toPatch.Add(type);
			}

			toPatch.ForEach(type => patchClass(type));
		}

		static void patchClass(Type type)
		{																						$"CraftHelper: patching {type}".logDbg();
			(Activator.CreateInstance(type) as CraftableObject)?.patch();
		}
	}
}