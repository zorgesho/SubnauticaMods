using System.Collections.Generic;

#if GAME_SN
	using _TechInfo = SMLHelper.V2.Crafting.TechData;
	using _Ingredient = SMLHelper.V2.Crafting.Ingredient;
#elif GAME_BZ
	using _TechInfo = SMLHelper.V2.Crafting.RecipeData;
	using _Ingredient = Ingredient;
#endif

namespace Common.Crafting
{
	// intermediate class that used for conversion to SMLHelper's TechData (GAME_SN) or RecipeData (GAME_BZ)
	class TechInfo
	{
		public record Ing(TechType techType, int amount = 1);

		public int craftAmount { get; init; } = 1;
		public readonly List<Ing> ingredients = new();
		public readonly List<TechType> linkedItems = new();

		public TechInfo(params Ing[] ingredients) => this.ingredients.AddRange(ingredients);

		public static implicit operator _TechInfo(TechInfo techInfo)
		{
			_TechInfo result = new()
			{
				craftAmount = techInfo.craftAmount,
				LinkedItems = techInfo.linkedItems
			};

			techInfo.ingredients.ForEach(ing => result.Ingredients.Add(new _Ingredient(ing.techType, ing.amount)));

			return result;
		}
	}
}