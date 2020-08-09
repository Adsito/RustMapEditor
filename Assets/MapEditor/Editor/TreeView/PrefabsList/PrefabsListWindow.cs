using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;

namespace RustMapEditor.UI
{
	class PrefabsListWindow : EditorWindow
	{
		[NonSerialized] public bool m_Initialized;
		[SerializeField] TreeViewState treeViewState;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		SearchField m_SearchField;
		PrefabsListTreeView m_TreeView;

		private bool showAllPrefabs = false;

		Rect multiColumnTreeViewRect
		{
			get { return new Rect(20, 30, position.width - position.width / 3, position.height - 45); }
		}

		Rect optionsRect
		{
			get { return new Rect(position.width / 3 * 2 + 40, 10, position.width - (position.width / 3 * 2 + 40) - 20, 50); }
		}

		Rect previewImageRect
		{
			get { return new Rect(position.width / 3 * 2 + 40, 60, position.width - (position.width / 3 * 2 + 40) - 20, position.width - (position.width / 3 * 2 + 40) - 20); }
		}

		Rect previewImageDetails
		{
			get { return new Rect(position.width / 3 * 2 + 40, position.width - (position.width / 3 * 2 + 40) + 60, position.width - (position.width / 3 * 2 + 40) - 20, position.width - (position.width / 3 * 2 + 40) - 20); }
		}

		Rect searchBarRect
		{
			get { return new Rect(20, 10, position.width - position.width / 3, 20); }
		}

		public PrefabsListTreeView treeView
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
					width = 120,
					minWidth = 90,
					autoResize = true,
					allowToggleVisibility = true
				},
				new MultiColumnHeaderState.Column
				{
					headerContent = ToolTips.prefabHierachyRustID,
					width = 60,
					minWidth = 60,
					autoResize = true,
					allowToggleVisibility = true
				},
			};

			var state = new MultiColumnHeaderState(columns);
			return state;
		}

        private void OnEnable()
        {
			EventManager.BundlesLoaded += ReloadTree;
		}

        void InitIfNeeded()
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
					multiColumnHeader.ResizeToFit();

				var treeModel = new TreeModel<PrefabsListElement>(PrefabsListTreeView.GetPrefabsListElements());

				m_TreeView = new PrefabsListTreeView(treeViewState, multiColumnHeader, treeModel);

				m_SearchField = new SearchField();
				m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

				treeView.previewImage = new Texture2D(60, 60);
				treeView.prefabData = new WorldSerialization.PrefabData() { id = 0 };

				m_Initialized = true;
			}
		}

		void OnSelectionChange()
		{
			if (!m_Initialized)
				return;
		}

		void OnGUI()
		{
			InitIfNeeded();
			DrawSearchBar(searchBarRect);
			DrawTreeView(multiColumnTreeViewRect);
			DrawOptions(optionsRect, treeView, ref showAllPrefabs);
			DrawPreviewImage(previewImageRect);
			DrawPreviewDetails(previewImageDetails);
		}

		void DrawSearchBar(Rect rect)
		{
			treeView.searchString = m_SearchField.OnGUI(rect, treeView.searchString);
		}

		void DrawTreeView(Rect rect)
		{
			m_TreeView.OnGUI(rect);
		}

		void DrawOptions(Rect rect, PrefabsListTreeView treeView, ref bool showAllPrefabs)
		{
			GUILayout.BeginArea(rect);
			Functions.SelectPrefabPaths(treeView, ref showAllPrefabs);
			GUILayout.EndArea();
		}

		void DrawPreviewImage(Rect rect)
		{
			GUI.DrawTexture(rect, treeView.previewImage);
		}

		void DrawPreviewDetails(Rect rect)
		{
			GUILayout.BeginArea(rect);
			Elements.BoldLabel(ToolTips.prefabDetailsLabel);
			Functions.DisplayPrefabName(treeView.prefabName);
			Functions.DisplayPrefabID(treeView.prefabData);
			Functions.DisplayPrefabPath(treeView.prefabData);
			GUILayout.EndArea();
		}

		public static void ReloadTree()
		{
			if (HasOpenInstances<PrefabsListWindow>())
			{
				PrefabsListWindow window = (PrefabsListWindow)GetWindow(typeof(PrefabsListWindow), false, "Prefab List");
				window.m_Initialized = false;
			}
		}
	}
}