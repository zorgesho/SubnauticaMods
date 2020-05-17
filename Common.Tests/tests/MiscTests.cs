using System;
using System.Reflection;

using Harmony;
using UnityEngine;
using NUnit.Framework;

using Common;

namespace CommonTests
{
	[TestFixture]
	public class MiscTests
	{
		[Test]
		public void testReflection()
		{
			// not overloaded method
			MethodInfo float_IsInfinity_net = typeof(float).GetMethod("IsInfinity");
			MethodInfo float_IsInfinity_at = AccessTools.Method(typeof(float), "IsInfinity");
			MethodInfo float_IsInfinity_my = typeof(float).method("IsInfinity");

			Assert.That(float_IsInfinity_net, Is.Not.EqualTo(null));
			Assert.That(float_IsInfinity_net, Is.EqualTo(float_IsInfinity_at));
			Assert.That(float_IsInfinity_net, Is.EqualTo(float_IsInfinity_my));

			// overloaded method
			MethodInfo floatParse_net = typeof(float).GetMethod("Parse", new Type[] { typeof(string) });
			MethodInfo floatParse_at = AccessTools.Method(typeof(float), "Parse", new Type[] { typeof(string) });
			MethodInfo floatParse_my = typeof(float).method("Parse", typeof(string));
			MethodInfo floatParse_my_null = typeof(float).method("Parse"); // trying to get overloaded method but don't providing parameters

			Assert.That(floatParse_my_null, Is.EqualTo(null));
			Assert.That(floatParse_net, Is.Not.EqualTo(null));
			Assert.That(floatParse_net, Is.EqualTo(floatParse_at));
			Assert.That(floatParse_net, Is.EqualTo(floatParse_my));

			// overloaded generic method
			MethodInfo Component_GetComponent_net = typeof(Component).GetMethod("GetComponent", new Type[0]);
			MethodInfo Component_GetComponent_at = AccessTools.Method(typeof(Component), "GetComponent");
			MethodInfo Component_GetComponent_my = typeof(Component).method("GetComponent", new Type[0]);

			Assert.That(Component_GetComponent_net, Is.Not.EqualTo(null));
			Assert.That(Component_GetComponent_net, Is.EqualTo(Component_GetComponent_at));
			Assert.That(Component_GetComponent_net, Is.EqualTo(Component_GetComponent_my));
		}
	}
}