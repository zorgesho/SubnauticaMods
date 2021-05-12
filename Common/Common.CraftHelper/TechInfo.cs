using System.Collections.Generic;

#if GAME_SN
	using _TechInfo = SMLHelper.V2.Crafting.TechData;
#elif GAME_BZ
	using _TechInfo = SMLHelper.V2.Crafting.RecipeData;
#endif

namespace Common.Crafting
{
	// intermediate class that used for conversion to/from SMLHelper's TechData (GAME_SN) or RecipeData (GAME_BZ)
	class TechInfo
	{
		public record Ing(TechType techType, int amount = 1);

		public int craftAmount { get; init; } = 1;
		public List<Ing> ingredients { get; init; } = new();
		public List<TechType> linkedItems { get; init; } = new();

		public TechInfo(params Ing[] ingredients)
		{
			this.ingredients.AddRange(ingredients);
		}

		public TechInfo(TechInfo techInfo): this(techInfo.ingredients.ToArray())
		{
			craftAmount = techInfo.craftAmount;
			linkedItems.AddRange(techInfo.linkedItems);
		}

		public static implicit operator _TechInfo(TechInfo techInfo)
		{
			_TechInfo result = new()
			{
				craftAmount = techInfo.craftAmount,
				LinkedItems = techInfo.linkedItems
			};

			techInfo.ingredients.ForEach(ing => result.Ingredients.Add(new (ing.techType, ing.amount)));

			return result;
		}

		public static implicit operator TechInfo(_TechInfo techInfo)
		{
			TechInfo result = new()
			{
				craftAmount = techInfo.craftAmount,
				linkedItems = techInfo.LinkedItems
			};

			techInfo.Ingredients.ForEach(ing => result.ingredients.Add(new (ing.techType, ing.amount)));

			return result;
		}
#if GAME_SN
		public static implicit operator CraftData.TechData(TechInfo techInfo)
		{
			CraftData.TechData result = new()
			{
				_craftAmount = techInfo.craftAmount,
				_ingredients = new(),
				_linkedItems = techInfo.linkedItems
			};

			techInfo.ingredients.ForEach(ing => result._ingredients.Add(ing.techType, ing.amount));

			return result;
		}
#endif
	}
}