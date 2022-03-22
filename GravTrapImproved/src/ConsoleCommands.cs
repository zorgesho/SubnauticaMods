using System.Collections.Generic;
using Common;

namespace GravTrapImproved
{
	class ConsoleCommands: PersistentConsoleCommands
	{
		static readonly string configPath = Paths.formatFileName("types_config", "json");
		static List<TypesConfig.TechTypeList> techTypeLists => Main.typesConfig.techTypeLists;

		static void updateLists()
		{
			Main.typesConfig.reinit();
			Main.typesConfig.save(configPath); // typesConfig is read-only, so we need to use full path

			foreach (var g in FindObjectsOfType<Gravsphere>())
			{
				g.OnPickedUp(g.pickupable);
				g.OnDropped(g.pickupable);
			}
		}

		static TypesConfig.TechTypeList getList(string name)
		{
			var list = techTypeLists.Find(list => list.name.ToLower().startsWith(name));
			list ??= techTypeLists.Find(list => list.name.ToLower().Contains(name));

			if (list == null)
				$"Tech type list '{name}' not found!".onScreen();

			return list;
		}

		public void gti_addtech(string listName, TechType techType)
		{
			if (getList(listName) is not TypesConfig.TechTypeList list)
				return;

			if (list.add(techType))
			{
				updateLists();
				$"Tech type '{techType}' added to '{list.name}' list".onScreen();
			}
		}

		public void gti_removetech(string listName, TechType techType)
		{
			if (getList(listName) is not TypesConfig.TechTypeList list)
				return;

			if (list.remove(techType))
			{
				updateLists();
				$"Tech type '{techType}' removed from '{list.name}' list".onScreen();
			}
		}

		[Command(caseSensitive = true, combineArgs = true)]
		public void gti_addlist(string listName)
		{
			techTypeLists.Add(new (listName));
			updateLists();
			$"Tech type list '{listName}' added".onScreen();
		}

		public void gti_removelist(string listName)
		{
			if (getList(listName) is not TypesConfig.TechTypeList list)
				return;

			techTypeLists.Remove(list);
			updateLists();
			$"Tech type list '{list.name}' removed".onScreen();
		}
	}
}