using NUnit.Framework;

using Common.Configuration;

namespace CommonTests
{
	[TestFixture]
	public class ConfigTests
	{
		class SimpleTestConfig: Config
		{
			[Field.Bounds(Min: 20)]
			public int testMin = 15;

			[Field.Bounds(Max: 100)]
			public int testMax = 150;

			[Field.Bounds(100, 200)]
			public int test = 150;
		}

		[Test]
		public void testBoundsSimple()
		{
			SimpleTestConfig testConfig = Config.tryLoad<SimpleTestConfig>(null);

			Assert.That(testConfig.testMin, Is.EqualTo(20));
			Assert.That(testConfig.testMax, Is.EqualTo(100));
			Assert.That(testConfig.test, Is.EqualTo(150));
		}

		class VariableTestConfig: Config
		{
			[Field.Bounds(Min: 20)]
			public int testMin20;
			static public int testMinInitial;

			[Field.Bounds(Max: 50)]
			public float testMax50;
			static public float testMaxInitial;

			[Field.Bounds(-100, 100)]
			public float testRange_m100_100;
			static public float testRangeInitial;

			public VariableTestConfig()
			{
				testMin20 = testMinInitial;
				testMax50 = testMaxInitial;
				testRange_m100_100 = testRangeInitial;
			}
		}

		[Test]
		public void testBoundsVariable([Values(1000, -1000, 150)] int min, [Values(1000, -1000, 150)] int max, [Values(1000, -1000, 150)] int range)
		{
			testBounds(min, max, range);
		}
		
		[Test]
		public void testBoundsRandom([Random(-1000, 1000, 3)] float min, [Random(-1000, 1000, 3)] float max, [Random(-1000, 1000, 3)] float range)
		{
			testBounds(min, max, range);
		}

		void testBounds(float min, float max, float range)
		{
			VariableTestConfig.testMinInitial = (int)min;
			VariableTestConfig.testMaxInitial = max;
			VariableTestConfig.testRangeInitial = range;

			VariableTestConfig testConfig = Config.tryLoad<VariableTestConfig>(null);

			Assert.That(testConfig.testMin20, Is.GreaterThanOrEqualTo(20));
			Assert.That(testConfig.testMax50, Is.LessThanOrEqualTo(50));
			Assert.That(testConfig.testRange_m100_100, Is.InRange(-100, 100));
		}
	}
}
