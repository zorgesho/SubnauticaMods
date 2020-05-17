using NUnit.Framework;
using Common.Configuration;

namespace CommonTests
{
	[TestFixture]
	public class ConfigTests
	{
		class SimpleTestConfig: Config
		{
			[Field.Range(min: 20)]
			public int testMin = 15;

			[Field.Range(max: 100)]
			public int testMax = 150;

			[Field.Range(100, 200)]
			public int test = 150;
		}

		[Test]
		public void testRangeSimple()
		{
			SimpleTestConfig testConfig = Config.tryLoad<SimpleTestConfig>(null);

			Assert.That(testConfig.testMin, Is.EqualTo(20));
			Assert.That(testConfig.testMax, Is.EqualTo(100));
			Assert.That(testConfig.test, Is.EqualTo(150));
		}

		class VariableTestConfig: Config
		{
			[Field.Range(min: 20)]
			public int testMin20;
			public static int testMinInitial;

			[Field.Range(max: 50)]
			public float testMax50;
			public static float testMaxInitial;

			[Field.Range(-100, 100)]
			public float testRange_m100_100;
			public static float testRangeInitial;

			public VariableTestConfig()
			{
				testMin20 = testMinInitial;
				testMax50 = testMaxInitial;
				testRange_m100_100 = testRangeInitial;
			}
		}

		[Test]
		public void testRangeVariable([Values(1000, -1000, 150)] int min, [Values(1000, -1000, 150)] int max, [Values(1000, -1000, 150)] int range)
		{
			testRange(min, max, range);
		}

		[Test]
		public void testRangeRandom([Random(-1000, 1000, 3)] float min, [Random(-1000, 1000, 3)] float max, [Random(-1000, 1000, 3)] float range)
		{
			testRange(min, max, range);
		}

		void testRange(float min, float max, float range)
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