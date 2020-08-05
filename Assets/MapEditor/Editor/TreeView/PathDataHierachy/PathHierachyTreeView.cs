using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace RustMapEditor.UI
{
    internal class PathHierachyTreeView : TreeViewWithTreeModel<PathHierachyElement>
    {
        const float kRowHeights = 20f;
        const float kToggleWidth = 18f;

        enum Columns
        {
            Name,
            Width,
            InnerPadding,
            OuterPadding,
            InnerFade,
            OuterFade,
        }

        public enum SortOption
        {
            Name,
            Width,
            InnerPadding,
            OuterPadding,
            InnerFade,
            OuterFade,
        }

        SortOption[] m_SortOptions =
        {
            SortOption.Name,
            SortOption.Width,
            SortOption.InnerPadding,
            SortOption.OuterPadding,
            SortOption.InnerFade,
            SortOption.OuterFade,
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

        public PathHierachyTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<PathHierachyElement> model) : base(state, multicolumnHeader, model)
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
        public static List<PathHierachyElement> GetPathHierachyElements()
        {
            List<PathHierachyElement> pathHierachyElements = new List<PathHierachyElement>();
            pathHierachyElements.Add(new PathHierachyElement("", -1, -1));
            if (PathManager.DefaultPath == null)
                return pathHierachyElements;

            var paths = PathManager.CurrentMapPaths;
            for (int i = 0; i < paths.Length; i++)
            {
                string name = String.Format("{0}:{1}:{2}:{3}:{4}:{5}", paths[i].pathData.name, paths[i].pathData.width, paths[i].pathData.innerPadding, paths[i].pathData.outerPadding, paths[i].pathData.innerFade, paths[i].pathData.outerFade);
                pathHierachyElements.Add(new PathHierachyElement(name, 0, i) 
                { 
                    pathData = paths[i]
                });
            }
            return pathHierachyElements;
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

            var myTypes = rootItem.children.Cast<TreeViewItem<PathHierachyElement>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.pathName, ascending);
                        break;
                    case SortOption.Width:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.width, ascending);
                        break;
                    case SortOption.InnerPadding:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.innerPadding, ascending);
                        break;
                    case SortOption.OuterPadding:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.outerPadding, ascending);
                        break;
                    case SortOption.InnerFade:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.innerFade, ascending);
                        break;
                    case SortOption.OuterFade:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.outerFade, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<PathHierachyElement>> InitialOrder(IEnumerable<TreeViewItem<PathHierachyElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Name:
                    return myTypes.Order(l => l.data.pathName, ascending);
                case SortOption.Width:
                    return myTypes.Order(l => l.data.width, ascending);
                case SortOption.InnerPadding:
                    return myTypes.Order(l => l.data.innerPadding, ascending);
                case SortOption.OuterPadding:
                    return myTypes.Order(l => l.data.outerPadding, ascending);
                case SortOption.InnerFade:
                    return myTypes.Order(l => l.data.innerFade, ascending);
                case SortOption.OuterFade:
                    return myTypes.Order(l => l.data.outerFade, ascending);
            }
            return myTypes.Order(l => l.data.name, ascending);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<PathHierachyElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), item, (Columns)args.GetColumn(i), ref args);
        }

        void CellGUI(Rect cellRect, TreeViewItem<PathHierachyElement> item, Columns column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case Columns.Name:
                    Rect textRect = cellRect;
                    textRect.x += GetContentIndent(item);
                    textRect.xMax = cellRect.xMax - textRect.x;
                    GUI.Label(textRect, item.data.pathName);
                    break;
                case Columns.Width:
                    item.data.width = EditorGUI.FloatField(cellRect, item.data.width);
                    break;
                case Columns.InnerPadding:
                    item.data.innerPadding = EditorGUI.FloatField(cellRect, item.data.innerPadding);
                    break;
                case Columns.OuterPadding:
                    item.data.outerPadding = EditorGUI.FloatField(cellRect, item.data.outerPadding);
                    break;
                case Columns.InnerFade:
                    item.data.innerFade = EditorGUI.FloatField(cellRect, item.data.innerFade);
                    break;
                case Columns.OuterFade:
                    item.data.outerFade = EditorGUI.FloatField(cellRect, item.data.outerFade);
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return false;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            Selection.activeObject = treeModel.Find(selectedIds[0]).pathData.gameObject;
        }

        protected override void DoubleClickedItem(int id)
        {
            SceneView.lastActiveSceneView.LookAt(treeModel.Find(id).pathData.gameObject.transform.position);
        }
    }
}