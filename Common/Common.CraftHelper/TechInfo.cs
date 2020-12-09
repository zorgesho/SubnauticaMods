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
		public class Ing
		{
			public TechType techType;
			public int amount;

			public Ing(TechType techType, int amount = 1)
			{
				this.techType = techType;
				this.amount = amount;
			}
		}

		public int craftAmount = 1;
		public List<Ing> ingredients = new List<Ing>();
		public List<TechType> linkedItems = new List<TechType>();

		public TechInfo(params Ing[] ingredients) => ingredients.forEach(ing => this.ingredients.Add(ing));

		public static implicit operator _TechInfo(TechInfo techInfo)
		{
			var result = new _TechInfo()
			{
				craftAmount = techInfo.craftAmount,
				LinkedItems = techInfo.linkedItems
			};

			techInfo.ingredients.ForEach(ing => result.Ingredients.Add(new _Ingredient(ing.techType, ing.amount)));

			return result;
		}
	}
}