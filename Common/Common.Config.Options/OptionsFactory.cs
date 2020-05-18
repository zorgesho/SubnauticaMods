using System;
using System.Linq;
using System.Collections.Generic;

namespace Common.Configuration
{
	using Reflection;

	partial class Options
	{
		static partial class Factory
		{
			interface ICreator
			{
				ModOption create(Config.Field cfgField);
			}
			interface IModifier
			{
				void process(ModOption option);
			}

			static readonly List<ICreator>  creators  = _getList<ICreator>();
			static readonly List<IModifier> modifiers = _getList<IModifier>();

			static List<I> _getList<I>() => typeof(Factory).GetNestedTypes(ReflectionHelper.bfAll).
															Where(type => !type.IsInterface && typeof(I).IsAssignableFrom(type)).
															Select(Activator.CreateInstance).Cast<I>().
															ToList();

			// create mod option based on underlying type and attributes of cfgField
			public static ModOption create(Config.Field cfgField)
			{
				ModOption option = null;
#if DEBUG
				// trying to use all creators to check for ambiguity
				foreach (var c in creators)
				{
					var optionTmp = c.create(cfgField);

					Debug.assert(option == null || optionTmp == null,
						$"Options.Factory: ambiguity for field '{cfgField.path}' (both {option?.GetType().Name} and {optionTmp?.GetType().Name})");

					option ??= optionTmp;
				}
#else
				foreach (var c in creators)
					if ((option = c.create(cfgField)) != null)
						break;
#endif
				if (option != null)
					modifiers.ForEach(m => m.process(option));

				return option;
			}
		}
	}
}