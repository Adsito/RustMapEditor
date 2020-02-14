using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using RustMapEditor.Variables;

namespace RustMapEditor.UI
{
	class PrefabsListWindow : EditorWindow
	{
		[NonSerialized] bool m_Initialized;
		[SerializeField] TreeViewState treeViewState;
		[SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
		SearchField m_SearchField;
		PrefabsListTreeView m_TreeView;

		Rect multiColumnTreeViewRect
		{
			get { return new Rect(20, 30, position.width - position.width / 3, position.height - 45); }
		}

		Rect previewImageRect
		{
			get { return new Rect(position.width / 3 * 2 + 40, 60, position.width - (position.width / 3 * 2 + 40) - 20, position.width - (position.width / 3 * 2 + 40) - 20); }
		}

		Rect previewImageDetails
		{
			get { return new Rect(position.width / 3 * 2 + 40, position.width - (position.width / 3 * 2 + 40) + 60, position.width - (position.width / 3 * 2 + 40) - 20, position.width - (position.width / 3 * 2 + 40) - 20); }
		}

		Rect toolbarRect
		{
			get { return new Rect(20f, 10f, position.width - 40f, 20f); }
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
		void InitIfNeeded()
		{
			if (!m_Initialized)
			{
				// Check if it already exists (deserialized from window layout file or scriptable object)
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
			SearchBar(toolbarRect);
			DoTreeView(multiColumnTreeViewRect);
			DrawPreviewImage(previewImageRect);
			DrawPreviewDetails(previewImageDetails);
		}

		void SearchBar(Rect rect)
		{
			treeView.searchString = m_SearchField.OnGUI(rect, treeView.searchString);
		}


		void DoTreeView(Rect rect)
		{
			m_TreeView.OnGUI(rect);
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
	}
}