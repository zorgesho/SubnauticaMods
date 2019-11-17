using System.Collections.Generic;
using Harmony;

namespace Common.Crafting
{
	static class TreeNodeExtension
	{
		public static TreeNode insertNode(this TreeNode parent, string idAfter, TreeNode child)
		{
			if (parent == null || child == null || parent[child.id] != null)
			{
				$"TreeNode.insertNode failed ('{parent}' '{child}'".logError();
				return parent;
			}

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
			public TechType techType;
			
			public string idAfter;
			public string[] path;

			public CraftNode(TechType _techType, string _path, TechType techTypeAfter)
			{
				techType = _techType;
				path = _path.Split('/');
				idAfter = (techTypeAfter == TechType.None? null: techTypeAfter.AsString());
			}
		}

		static readonly Dictionary<CraftTree.Type, List<CraftNode>> nodesToAdd = new Dictionary<CraftTree.Type, List<CraftNode>>();

		public static void addNode(CraftTree.Type treeType, TechType techType, string path, TechType techTypeAfter)
		{
			// patching craft tree only if we need to
			if (!nodesToAdd.TryGetValue(treeType, out List<CraftNode> nodes))
			{
				patchCraftTree(treeType);
				nodes = new List<CraftNode>();
				nodesToAdd[treeType] = nodes;
			}

			nodes.Add(new CraftNode(techType, path, techTypeAfter));
		}

		static void addNodesToTree(CraftTree.Type treeType, ref global::CraftNode rootNode)
		{
			foreach (CraftNode node in nodesToAdd[treeType])
			{
				TreeNode parentNode = rootNode.FindNodeByPath(node.path);
				parentNode?.insertNode(node.idAfter, new global::CraftNode(node.techType.AsString(), TreeAction.Craft, node.techType));
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

			HarmonyHelper.harmonyInstance.Patch(AccessTools.Method(typeof(CraftTree), targetMethod),
				postfix: new HarmonyMethod(AccessTools.Method(typeof(CraftNodesCustomOrder), "addNodesTo_" + treeType.ToString())));
		}
	}
}