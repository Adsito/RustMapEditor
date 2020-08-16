using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace RustMapEditor.UI
{
	internal class PrefabHierarchyTreeView : TreeViewWithTreeModel<PrefabHierarchyElement>
	{
		const float kRowHeights = 20f;
		const float kToggleWidth = 18f;
		
		enum Columns
		{
			Name,
			Type,
			Category,
            RustID,
		}

		public enum SortOption
		{
			Name,
			Type,
            Category,
            RustID,
        }

		SortOption[] m_SortOptions = 
		{
			SortOption.Name, 
			SortOption.Type,
			SortOption.Category,
            SortOption.RustID,
        };

		public static void TreeToList (TreeViewItem root, IList<TreeViewItem> result)
		{
			if (root == null)
				throw new NullReferenceException("root");
			if (result == null)
				throw new NullReferenceException("result");

			result.Clear();
	
			if (root.children == null)
				return;

			Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
			for (int i = root.children.Count - 1; i >= 0; i--)
				stack.Push(root.children[i]);

			while (stack.Count > 0)
			{
				TreeViewItem current = stack.Pop();
				result.Add(current);

				if (current.hasChildren && current.children[0] != null)
					for (int i = current.children.Count - 1; i >= 0; i--)
						stack.Push(current.children[i]);
			}
		}

		public PrefabHierarchyTreeView (TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<PrefabHierarchyElement> model) : base (state, multicolumnHeader, model)
		{
			Assert.AreEqual(m_SortOptions.Length , Enum.GetValues(typeof(Columns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

			rowHeight = kRowHeights;
			columnIndexForTreeFoldouts = 0;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
			extraSpaceBeforeIconAndLabel = kToggleWidth;
			multicolumnHeader.sortingChanged += OnSortingChanged;
			Reload();
		}

        public static List<PrefabHierarchyElement> GetPrefabHierachyElements()
        {
            List<PrefabHierarchyElement> prefabHierachyElements = new List<PrefabHierarchyElement>();
            prefabHierachyElements.Add(new PrefabHierarchyElement(-1, -1, ""));
			if (PrefabManager.PrefabParent == null)
				return prefabHierachyElements;

            var prefabs = PrefabManager.CurrentMapPrefabs;
            for (int i = 0; i < prefabs.Length; i++)
            {
                string name = String.Format("{0}:{1}:{2}:{3}", prefabs[i].name.Replace(':', ' '), "Rust", prefabs[i].prefabData.category, prefabs[i].prefabData.id);
                prefabHierachyElements.Add(new PrefabHierarchyElement(0, i, name, "Rust", prefabs[i].prefabData.category, prefabs[i].prefabData.id) 
				{
					prefabDataHolder = prefabs[i],
				});
            }
            return prefabHierachyElements;
        }

		public static List<PrefabDataHolder> PrefabDataFromSelection(PrefabHierarchyTreeView treeView)
        {
			List<PrefabDataHolder> prefabDataList = new List<PrefabDataHolder>();
			foreach (var item in treeView.GetSelection())
				prefabDataList.Add(treeView.treeModel.Find(item).prefabDataHolder);

			return prefabDataList;
		}

		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows (root);
			SortIfNeeded (root, rows);
			return rows;
		}

		void OnSortingChanged (MultiColumnHeader multiColumnHeader)
		{
			SortIfNeeded (rootItem, GetRows());
		}

		void SortIfNeeded (TreeViewItem root, IList<TreeViewItem> rows)
		{
			if (rows.Count <= 1)
				return;
			
			if (multiColumnHeader.sortedColumnIndex == -1)
				return;

			SortByMultipleColumns ();
			TreeToList(root, rows);
			Repaint();
		}

		void SortByMultipleColumns ()
		{
			var sortedColumns = multiColumnHeader.state.sortedColumns;

			if (sortedColumns.Length == 0)
				return;

			var myTypes = rootItem.children.Cast<TreeViewItem<PrefabHierarchyElement> >();
			var orderedQuery = InitialOrder (myTypes, sortedColumns);
			for (int i=1; i<sortedColumns.Length; i++)
			{
				SortOption sortOption = m_SortOptions[sortedColumns[i]];
				bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

				switch (sortOption)
				{
					case SortOption.Name:
						orderedQuery = orderedQuery.ThenBy(l => l.data.PrefabName, ascending);
						break;
					case SortOption.Type:
						orderedQuery = orderedQuery.ThenBy(l => l.data.Type, ascending);
						break;
					case SortOption.Category:
						orderedQuery = orderedQuery.ThenBy(l => l.data.Category, ascending);
						break;
                    case SortOption.RustID:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.RustID, ascending);
                        break;
                }
			}

			rootItem.children = orderedQuery.Cast<TreeViewItem> ().ToList ();
		}

		IOrderedEnumerable<TreeViewItem<PrefabHierarchyElement>> InitialOrder(IEnumerable<TreeViewItem<PrefabHierarchyElement>> myTypes, int[] history)
		{
			SortOption sortOption = m_SortOptions[history[0]];
			bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
			switch (sortOption)
			{
				case SortOption.Name:
					return myTypes.Order(l => l.data.PrefabName, ascending);
				case SortOption.Type:
					return myTypes.Order(l => l.data.Type, ascending);
				case SortOption.Category:
					return myTypes.Order(l => l.data.Category, ascending);
                case SortOption.RustID:
                    return myTypes.Order(l => l.data.RustID, ascending);
            }
			return myTypes.Order(l => l.data.name, ascending);
		}

		protected override void RowGUI (RowGUIArgs args)
		{
			var item = (TreeViewItem<PrefabHierarchyElement>) args.item;

			for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
				CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
		}

		void CellGUI (Rect cellRect, TreeViewItem<PrefabHierarchyElement> item, Columns column, ref RowGUIArgs args)
		{
			CenterRectUsingSingleLineHeight(ref cellRect);

			switch (column)
			{
				case Columns.Name:
                    Rect textRect = cellRect;
                    textRect.x += GetContentIndent(item);
                    textRect.xMax = cellRect.xMax - textRect.x;
					GUI.Label(cellRect, item.data.PrefabName); 
                    break;
				case Columns.Type:
                    GUI.Label(cellRect, item.data.Type);
					break;
                case Columns.Category:
                    GUI.Label(cellRect, item.data.Category);
                    break;
                case Columns.RustID:
                    GUI.Label(cellRect, item.data.RustID.ToString());
                    break;
            }
		}

		protected override bool CanMultiSelect (TreeViewItem item)
		{
			return true;
		}

		protected override bool CanStartDrag(CanStartDragArgs args)
		{
			return false;
		}

        protected override void DoubleClickedItem(int id)
		{
			SceneView.lastActiveSceneView.LookAt(treeModel.Find(id).prefabDataHolder.gameObject.transform.position);
			Selection.activeObject = treeModel.Find(id).prefabDataHolder.gameObject;
		}
	}
}