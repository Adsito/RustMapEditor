using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace RustMapEditor.UI
{
    internal class PrefabsListTreeView : TreeViewWithTreeModel<PrefabsListElement>
    {
        const float kRowHeights = 20f;
        const float kToggleWidth = 18f;

        public Texture2D previewImage;
        public WorldSerialization.PrefabData prefabData;
        public string prefabName;

        enum Columns
        {
            Name,
        }

        public enum SortOption
        {
            Name,
        }

        SortOption[] m_SortOptions =
        {
            SortOption.Name,
        };

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
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
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        public PrefabsListTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<PrefabsListElement> model) : base(state, multicolumnHeader, model)
        {
            Assert.AreEqual(m_SortOptions.Length, Enum.GetValues(typeof(Columns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

            rowHeight = kRowHeights;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kToggleWidth;
            multicolumnHeader.sortingChanged += OnSortingChanged;
            Reload();
        }
        public static List<PrefabsListElement> GetPrefabsListElements()
        {
            Dictionary<string, PrefabsListElement> treeviewParents = new Dictionary<string, PrefabsListElement>();
            List<PrefabsListElement> prefabsListElements = new List<PrefabsListElement>();
            prefabsListElements.Add(new PrefabsListElement("Root", -1, 0));
            var manifestStrings = BundleManager.GetManifestStrings();
            if (manifestStrings == null)
                return prefabsListElements;
            var prefabID = -1000000; // Set this really low so it doesn't ever go into the positives or otherwise run into the risk of being the same id as a prefab.
            var parentID = -2000;
            foreach (var manifestString in manifestStrings)
            {
                if (manifestString.Contains(".prefab"))
                {
                    var assetNameSplit = manifestString.Split('/');
                    for (int i = 0; i < assetNameSplit.Length; i++)
                    {
                        var treePath = "";
                        for (int j = 0; j <= i; j++)
                        {
                            treePath += assetNameSplit[j];
                        }
                        if (!treeviewParents.ContainsKey(treePath))
                        {
                            var prefabName = assetNameSplit[assetNameSplit.Length - 1].Replace(".prefab", "");
                            if (i != assetNameSplit.Length - 1)
                            {
                                var treeviewItem = new PrefabsListElement(assetNameSplit[i], i, parentID);
                                prefabsListElements.Add(treeviewItem);
                                treeviewParents.Add(treePath, treeviewItem);
                                parentID++;
                            }
                            else
                            {
                                var treeviewItem = new PrefabsListElement(prefabName, i, prefabID);
                                treeviewItem.rustID = StringPool.Get(manifestString);
                                if (treeviewItem.rustID == 0)
                                    continue;
                                prefabsListElements.Add(treeviewItem);
                                treeviewParents.Add(treePath, treeviewItem);
                                prefabID++;
                            }
                        }
                    }
                }
            }
            return prefabsListElements;
        }

        // Note we only build the visible rows, only the backend has the full tree information. 
        // The treeview only creates info for the row list.
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            // Sort the roots of the existing tree items
            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
        }

        void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<TreeViewItem<PrefabsListElement>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.prefabName, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<PrefabsListElement>> InitialOrder(IEnumerable<TreeViewItem<PrefabsListElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Name:
                    return myTypes.Order(l => l.data.prefabName, ascending);
            }
            return myTypes.Order(l => l.data.name, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<PrefabsListElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<PrefabsListElement> item, Columns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case Columns.Name:
                    Rect textRect = cellRect;
                    textRect.x += GetContentIndent(item);
                    textRect.xMax = cellRect.xMax - textRect.x;
                    GUI.Label(textRect, item.data.prefabName);
                    break;
            }
        }
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            // Disables
        }

        protected override void SingleClickedItem(int id)
        {
            var itemClicked = treeModel.Find(id);
            if (itemClicked.rustID == 0)
                return;
            previewImage = AssetPreview.GetAssetPreview(PrefabManager.Load(itemClicked.rustID));
            if (previewImage == null)
                previewImage = new Texture2D(60, 60);
            prefabData = PrefabManager.Load(itemClicked.rustID).GetComponent<PrefabDataHolder>().prefabData;
            prefabName = treeModel.Find(id).name;
        }

        protected override void DoubleClickedItem(int id)
        {
            var expand = !IsExpanded(id);
            SetExpanded(id, expand);
        }
    }
}