using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Configuration
{
	using Reflection;

	partial class Options
	{
		public class FactoryPriority: Attribute
		{
			public const int Last = 0;
			public const int VeryLow = 100;
			public const int Low = 200;
			public const int LowerThanNormal = 300;
			public const int Normal = 400;
			public const int HigherThanNormal = 500;
			public const int High = 600;
			public const int VeryHigh = 700;
			public const int First = 800;

			public readonly int priority;

			public FactoryPriority(int priority) => this.priority = priority;
		}
		public interface ICreator
		{
			ModOption create(Config.Field cfgField);
		}
		public interface IModifier
		{
			void process(ModOption option);
		}

		static partial class Factory
		{
			static readonly List<ICreator>  creators  = _getList<ICreator>();
			static readonly List<IModifier> modifiers = _getList<IModifier>();

			static List<I> _getList<I>() =>
				typeof(Factory).GetNestedTypes(ReflectionHelper.bfAll).
					Where(type => !type.IsInterface && typeof(I).IsAssignableFrom(type)).
					OrderByDescending(type => type.getAttr<FactoryPriority>()?.priority ?? FactoryPriority.Normal).
					Select(Activator.CreateInstance).Cast<I>().
					ToList();

			public static void add(ICreator creator) => creators.Add(creator);

			// create mod option based on underlying type and attributes of cfgField
			public static ModOption create(Config.Field cfgField)
			{
				ModOption option = null;

				foreach (var c in creators)
					if ((option = c.create(cfgField)) != null)
						break;

				if (option != null)
					modifiers.ForEach(m => m.process(option));

				return option;
			}
		}
	}
}