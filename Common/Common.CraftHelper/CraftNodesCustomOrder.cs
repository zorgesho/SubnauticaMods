using System.Collections.Generic;
using SMLHelper.V2.Handlers;

namespace Common.Crafting
{
	static class TreeNodeExtension
	{
		public static TreeNode insertNode(this TreeNode parent, string idAfter, TreeNode child)
		{
			if (parent == null || child == null || parent[child.id] != null && $"TreeNode.insertNode failed: '{parent}' '{child}'".logError())
				return parent;

			// if idAfter is null we adding new node as first node
			int indexAfter = idAfter == null? -1: parent.nodes.FindIndex(n => n.id == idAfter);

			if (indexAfter < 0 && idAfter != null)
				parent.nodes.Add(child);
			else
				parent.nodes.Insert(indexAfter + 1, child);

			child.SetParent(parent);																		$"TreeNode.insertNode parentNode:{parent.id} indexAfter:{indexAfter}".logDbg();

			return parent;
		}
	}

	static class CraftNodesCustomOrder
	{
		class CraftNode // hides global CraftNode
		{
			public readonly string id;
			public readonly string idAfter;
			public readonly string[] path;

			public readonly TechType techType;
			public readonly TreeAction treeAction;

			CraftNode(string id, TechType techType, string path, string idAfter)
			{
				this.id = id;
				this.techType = techType;
				this.path = path.Split('/');
				this.idAfter = idAfter;
			}

			public CraftNode(TechType techType, string path, TechType techTypeAfter):
				this(techType.AsString(), techType, path, techTypeAfter == TechType.None? null: techTypeAfter.AsString())
			{
				treeAction = TreeAction.Craft;
			}

			public CraftNode(string id, string path, string idAfter):
				this(id, TechType.None, path, idAfter)
			{
				treeAction = TreeAction.Expand;
			}
		}

		static readonly Dictionary<CraftTree.Type, List<CraftNode>> nodesToAdd = new Dictionary<CraftTree.Type, List<CraftNode>>();

		static List<CraftNode> getNodeList(CraftTree.Type treeType)
		{
			// patching craft tree only if we need to
			if (!nodesToAdd.TryGetValue(treeType, out List<CraftNode> nodes))
			{
				patchCraftTree(treeType);
				nodes = new List<CraftNode>();
				nodesToAdd[treeType] = nodes;
			}

			return nodes;
		}

		// for adding crafting node
		public static void addNode(CraftTree.Type treeType, TechType techType, string path, TechType techTypeAfter)
		{
			getNodeList(treeType).Add(new CraftNode(techType, path, techTypeAfter));
		}

		// for adding group node
		public static void addNode(CraftTree.Type treeType, string id, string displayName, string path, string idAfter, TechType spriteTechType)
		{
			getNodeList(treeType).Add(new CraftNode(id, path, idAfter));

			LanguageHandler.SetLanguageLine($"{treeType.ToString()}Menu_{id}", displayName);
			SpriteHandler.RegisterSprite(SpriteManager.Group.Category, $"{treeType.ToString()}_{id}", SpriteManager.Get(spriteTechType));
		}

		static void addNodesToTree(CraftTree.Type treeType, ref global::CraftNode rootNode)
		{
			foreach (CraftNode node in nodesToAdd[treeType])
			{
				TreeNode parentNode = rootNode.FindNodeByPath(node.path) ?? rootNode;
				parentNode.insertNode(node.idAfter, new global::CraftNode(node.id, node.treeAction, node.techType));
			}
		}

		static void addNodesTo_MapRoom(ref global::CraftNode __result)			 =>	addNodesToTree(CraftTree.Type.MapRoom, ref __result);
		static void addNodesTo_Workbench(ref global::CraftNode __result)		 =>	addNodesToTree(CraftTree.Type.Workbench, ref __result);
		static void addNodesTo_Fabricator(ref global::CraftNode __result)		 =>	addNodesToTree(CraftTree.Type.Fabricator, ref __result);
		static void addNodesTo_Constructor(ref global::CraftNode __result)		 =>	addNodesToTree(CraftTree.Type.Constructor, ref __result);
		static void addNodesTo_SeamothUpgrades(ref global::CraftNode __result)	 =>	addNodesToTree(CraftTree.Type.SeamothUpgrades, ref __result);
		static void addNodesTo_CyclopsFabricator(ref global::CraftNode __result) => addNodesToTree(CraftTree.Type.CyclopsFabricator, ref __result);

		static void patchCraftTree(CraftTree.Type treeType)
		{
			string targetMethod = treeType.ToString() + (treeType != CraftTree.Type.MapRoom? "Scheme": "Sheme"); // :-\

			HarmonyHelper.patch(typeof(CraftTree).method(targetMethod), postfix: typeof(CraftNodesCustomOrder).method("addNodesTo_" + treeType.ToString()));
		}
	}
}