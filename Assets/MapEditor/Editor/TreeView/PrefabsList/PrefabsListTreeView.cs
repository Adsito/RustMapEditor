using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace RustMapEditor.UI
{
    public class PrefabsListTreeView : TreeViewWithTreeModel<PrefabsListElement>
    {
        const float kRowHeights = 20f;
        const float kToggleWidth = 18f;

        public Texture2D previewImage;
        public WorldSerialization.PrefabData prefabData;
        public string prefabName;

        public bool showAll = false;

        enum Columns
        {
            Name,
            ID
        }

        public enum SortOption
        {
            Name,
            ID
        }

        SortOption[] m_SortOptions =
        {
            SortOption.Name,
            SortOption.ID
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
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f;
            extraSpaceBeforeIconAndLabel = kToggleWidth;
            multicolumnHeader.sortingChanged += OnSortingChanged;
            Reload();
        }
        public static List<PrefabsListElement> GetPrefabsListElements(bool showAll = false)
        {
            Dictionary<string, PrefabsListElement> treeviewParents = new Dictionary<string, PrefabsListElement>();
            List<PrefabsListElement> prefabsListElements = new List<PrefabsListElement>();
            prefabsListElements.Add(new PrefabsListElement("Root", -1, 0));
            var manifestStrings = AssetManager.GetManifestStrings();
            if (manifestStrings == null)
                return prefabsListElements;

            var prefabStrings = showAll ? manifestStrings.Where(x => x.Contains(".prefab")): manifestStrings.Where(x => SettingsManager.PrefabPaths.Any(y => x.Contains(y)));
            int prefabID = 1, parentID = -1;
            foreach (var manifestString in prefabStrings)
            {
                var assetNameSplit = manifestString.Split('/');
                for (int i = 0; i < assetNameSplit.Length; i++)
                {
                    var treePath = "";
                    for (int j = 0; j <= i; j++)
                        treePath += assetNameSplit[j];

                    if (!treeviewParents.ContainsKey(treePath))
                    {
                        var prefabName = assetNameSplit[assetNameSplit.Length - 1].Replace(".prefab", "");
                        if (i != assetNameSplit.Length - 1)
                        {
                            var treeviewItem = new PrefabsListElement(assetNameSplit[i], i, parentID--);
                            prefabsListElements.Add(treeviewItem);
                            treeviewParents.Add(treePath, treeviewItem);
                        }
                        else
                        {
                            var treeviewItem = new PrefabsListElement(prefabName.Replace('_', ' '), i, prefabID++, manifestString);
                            if (treeviewItem.rustID == 0)
                                continue;
                            prefabsListElements.Add(treeviewItem);
                            treeviewParents.Add(treePath, treeviewItem);
                        }
                    }
                }
            }
            return prefabsListElements;
        }

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
                return;
            }
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
                    case SortOption.ID:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.rustID, ascending);
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
                case SortOption.ID:
                    return myTypes.Order(l => l.data.rustID, ascending);
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

            Rect textRect = cellRect;
            textRect.x += GetContentIndent(item);
            textRect.xMax = cellRect.xMax - textRect.x;
            switch (column)
            {
                case Columns.Name:
                    GUI.Label(textRect, item.data.prefabName);
                    break;
                case Columns.ID:
                    if (item.data.rustID != 0)
                    GUI.Label(cellRect, item.data.rustID.ToString());
                    break;
            }
        }

        public void RefreshTreeView(bool showAllPrefabs = false)
        {
            treeModel.SetData(GetPrefabsListElements(showAllPrefabs));
            Reload();
        }

        void SetItemSelected(int id)
        {
            var itemClicked = treeModel.Find(id);
            if (itemClicked.rustID == 0)
                return;

            PrefabManager.Load(itemClicked.rustID).SetActive(true);
            previewImage = AssetPreview.GetAssetPreview(PrefabManager.Load(itemClicked.rustID));
            if (previewImage == null)
                previewImage = new Texture2D(60, 60);

            prefabData = PrefabManager.Load(itemClicked.rustID).GetComponent<PrefabDataHolder>().prefabData;
            prefabName = itemClicked.prefabName;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            PrefabsListElement itemClicked = treeModel.Find(args.draggedItemIDs[0]);
            if (itemClicked.rustID != 0)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(prefabName, itemClicked);
                DragAndDrop.StartDrag("Spawn Prefab");
                PrefabManager.PrefabToSpawn = PrefabManager.Load(itemClicked.rustID);
            }
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return DragAndDropVisualMode.None;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            SetItemSelected(selectedIds[0]);
        }

        protected override void DoubleClickedItem(int id)
        {
            PrefabsListElement itemClicked = treeModel.Find(id);
            if (itemClicked.rustID != 0)
                PrefabManager.PrefabToSpawn = PrefabManager.Load(itemClicked.rustID);
            else
            {
                var expand = !IsExpanded(id);
                SetExpanded(id, expand);
            }
        }
    }
}