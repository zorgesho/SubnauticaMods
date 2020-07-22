using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Common.Reflection
{
	static class ReflectionHelper
	{
		public const BindingFlags bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

		public static Type getCallingType() => new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().ReflectedType;

		// for getting mod's defined types, don't include any of Common projects types (or types without namespace)
		public static readonly List<Type> definedTypes =
			Assembly.GetExecutingAssembly().GetTypes().Where(type => type.Namespace?.StartsWith(nameof(Common)) == false).ToList();
	}
}