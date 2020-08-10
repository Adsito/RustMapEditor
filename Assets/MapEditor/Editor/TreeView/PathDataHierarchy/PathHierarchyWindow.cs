using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;

namespace RustMapEditor.UI
{
	class PathHierarchyWindow : EditorWindow
	{
		[NonSerialized] bool m_Initialized;
		[SerializeField] TreeViewState treeViewState;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		SearchField m_SearchField;
		PathHierarchyTreeView m_TreeView;

		Rect multiColumnTreeViewRect
		{
			get { return new Rect(20, 30, position.width-40, position.height-60); }
		}

		Rect toolbarRect
		{
			get { return new Rect (20f, 10f, position.width-40f, 20f); }
		}

		public PathHierarchyTreeView treeView
		{
			get { return m_TreeView; }
		}

        public static MultiColumnHeaderState DefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.pathHierachyName,
                    width = 60,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
				new MultiColumnHeaderState.Column
				{
					headerContent = ToolTips.pathHierachyWidth,
					width = 45,
					minWidth = 40,
					autoResize = true,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.pathHierachyInnerPadding,
                    width = 45,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.pathHierachyOuterPadding,
                    width = 45,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.pathHierachyInnerFade,
                    width = 45,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.pathHierachyOuterFade,
                    width = 45,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
            };

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
        void InitIfNeeded ()
		{
			if (!m_Initialized)
			{
				if (treeViewState == null)
					treeViewState = new TreeViewState();

				bool firstInit = m_MultiColumnHeaderState == null;
				var headerState = DefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
				if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
					MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
				m_MultiColumnHeaderState = headerState;
				
				var multiColumnHeader = new MyMultiColumnHeader(headerState);
				if (firstInit)
					multiColumnHeader.ResizeToFit ();

				var treeModel = new TreeModel<PathHierarchyElement>(PathHierarchyTreeView.GetPathHierachyElements());
				
				m_TreeView = new PathHierarchyTreeView(treeViewState, multiColumnHeader, treeModel);

				m_SearchField = new SearchField();
				m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

				m_Initialized = true;
			}
		}

		void OnSelectionChange ()
		{
			if (!m_Initialized)
				return;
		}

		void OnGUI ()
		{
			InitIfNeeded();
			SearchBar (toolbarRect);
			DoTreeView (multiColumnTreeViewRect);
		}

		void SearchBar (Rect rect)
		{
			treeView.searchString = m_SearchField.OnGUI (rect, treeView.searchString);
		}

		void DoTreeView (Rect rect)
		{
			m_TreeView.OnGUI(rect);
		}

		public static void ReloadTree()
		{
			if (EditorWindow.HasOpenInstances<PathHierarchyWindow>())
			{
				PathHierarchyWindow window = (PathHierarchyWindow)EditorWindow.GetWindow(typeof(PrefabHierarchyWindow), false, "Path Hierachy");
				window.m_Initialized = false;
			}
		}
	}
}