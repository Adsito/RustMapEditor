using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;
using System.Collections.Generic;

namespace RustMapEditor.UI
{
	class PrefabHierarchyWindow : EditorWindow
	{
		[NonSerialized] bool m_Initialized;
		[SerializeField] TreeViewState treeViewState;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		SearchField m_SearchField;
		PrefabHierarchyTreeView m_TreeView;

		[NonSerialized] string category;
		[NonSerialized] bool replace;

		Rect multiColumnTreeViewRect
		{
			get { return new Rect(20, 30, position.width - position.width / 3, position.height - 45); }
		}

		Rect optionsRect
		{
			get { return new Rect(position.width / 3 * 2 + 40, 10, position.width - (position.width / 3 * 2 + 40) - 20, position.height); }
		}

		Rect searchBarRect
		{
			get { return new Rect(20, 10, position.width - position.width / 3, 20); }
		}

		public PrefabHierarchyTreeView treeView
		{
			get { return m_TreeView; }
		}

        public static MultiColumnHeaderState DefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.prefabHierachyName,
                    width = 60,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.prefabHierachyType,
                    width = 45,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.prefabHierachyCategory,
                    width = 150,
                    minWidth = 40,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = ToolTips.prefabHierachyRustID,
                    width = 110,
                    minWidth = 40,
                    autoResize = false,
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

				var treeModel = new TreeModel<PrefabHierarchyElement>(PrefabHierarchyTreeView.GetPrefabHierachyElements());
				
				m_TreeView = new PrefabHierarchyTreeView(treeViewState, multiColumnHeader, treeModel);

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
			DrawSearchBar(searchBarRect);
			DrawTreeView(multiColumnTreeViewRect);
			DrawOptions(optionsRect);
		}

		void DrawSearchBar (Rect rect)
		{
			treeView.searchString = m_SearchField.OnGUI (rect, treeView.searchString);
		}

		void DrawTreeView (Rect rect)
		{
			m_TreeView.OnGUI(rect);
		}

		void DrawOptions(Rect rect)
        {
			GUILayout.BeginArea(rect);
			Functions.PrefabHierachyOptions(treeView, ref category, ref replace);
			GUILayout.EndArea();
        }

        private void OnHierarchyChange()
        {
			ReloadTree();
			treeView.SetSelection(new List<int>());
        }

        public static void ReloadTree()
		{
			if (HasOpenInstances<PrefabHierarchyWindow>())
			{
				PrefabHierarchyWindow window = (PrefabHierarchyWindow)GetWindow(typeof(PrefabHierarchyWindow), false, "Prefab Hierachy");
				window.m_Initialized = false;
			}
		}
	}

	internal class MyMultiColumnHeader : MultiColumnHeader
	{
		Mode m_Mode;

		public enum Mode
		{
			LargeHeader,
			DefaultHeader,
			MinimumHeaderWithoutSorting
		}

		public MyMultiColumnHeader(MultiColumnHeaderState state)
			: base(state)
		{
			mode = Mode.DefaultHeader;
		}

		public Mode mode
		{
			get
			{
				return m_Mode;
			}
			set
			{
				m_Mode = value;
				switch (m_Mode)
				{
					case Mode.LargeHeader:
						canSort = true;
						height = 37f;
						break;
					case Mode.DefaultHeader:
						canSort = true;
						height = DefaultGUI.defaultHeight;
						break;
					case Mode.MinimumHeaderWithoutSorting:
						canSort = false;
						height = DefaultGUI.minimumHeight;
						break;
				}
			}
		}

		protected override void ColumnHeaderGUI (MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
		{
			base.ColumnHeaderGUI(column, headerRect, columnIndex);

			if (mode == Mode.LargeHeader)
			{
				if (columnIndex > 2)
				{
					headerRect.xMax -= 3f;
					var oldAlignment = EditorStyles.largeLabel.alignment;
					EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
					GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
					EditorStyles.largeLabel.alignment = oldAlignment;
				}
			}
		}
	}
}