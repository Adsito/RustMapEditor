//Comments and suggestions to business@runswimfly.com

using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

namespace UnityEditor.Experimental.TerrainAPI
{
    public class MaskedRidgeErodeTool : TerrainPaintTool<MaskedRidgeErodeTool>
    {
        float m_ErosionStrength = 16.0f;

        [SerializeField]
        float m_MixStrength = 0.7f;

        bool m_TextureMask = false;
        bool m_TextureStencil = false;
        int m_maskIndex = 0;
        int m_stencilIndex = 0;

        Material m_Material = null;

        Material GetPaintMaterial()
        {
            if (m_Material == null)
				m_Material = new Material(Shader.Find("RunSwimFlyTools/MaskedRidgeErode"));
            return m_Material;
        }

        public override string GetName()
        {
			return "RunSwimFly Tools/Masked Ridging Erosion";
        }

        public override string GetDesc()
        {
            return "Click to erode.\n\nThe selected texture index will mask or stencil the operation.";
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(
                terrain,
                editContext.brushTexture,
                editContext.brushSize);
        }

        public void SetMasks(Terrain terrain)
        {
            EditorGUILayout.BeginHorizontal();
            bool oldMask = m_TextureMask;
            m_TextureMask = EditorGUILayout.Toggle(new GUIContent("Texture Mask", "Toggles whether a second texture will be used to mask the operation."), m_TextureMask);
            m_TextureStencil = EditorGUILayout.Toggle(new GUIContent("Texture Stencil", "Toggles whether a second texture will be used to stencil the operation."), m_TextureStencil);
            if (m_TextureMask && !oldMask)
                m_TextureStencil = false;
            if (m_TextureStencil)
                m_TextureMask = false;
            EditorGUILayout.EndHorizontal();

            if (m_TextureMask)
            {
                m_maskIndex = EditorGUILayout.IntSlider(new GUIContent("Mask Index", "Select the index of the texture to be used as a mask"), m_maskIndex, 0, terrain.terrainData.terrainLayers.Length - 1);
                EditorGUILayout.LabelField(terrain.terrainData.terrainLayers[m_maskIndex].name);
            }

            if (m_TextureStencil)
            {
                m_stencilIndex = EditorGUILayout.IntSlider(new GUIContent("Stencil Index", "Select the index of the texture to be used as a stencil"), m_stencilIndex, 0, terrain.terrainData.terrainLayers.Length - 1);
                EditorGUILayout.LabelField(terrain.terrainData.terrainLayers[m_stencilIndex].name);
            }
        }

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
			EditorGUI.BeginChangeCheck();
			//m_ErosionStrength = EditorGUILayout.Slider(new GUIContent("Erosion strength"), m_ErosionStrength, 1, 128.0f);
			m_MixStrength = EditorGUILayout.Slider(new GUIContent("Feature Sharpness"), m_MixStrength, 0, 1);

            SetMasks(terrain);

            editContext.ShowBrushesGUI(0);

            if (EditorGUI.EndChangeCheck())
				Save(true);
		}
		
		public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, 0.0f);

            Rect rect = brushXform.GetBrushXYBounds();

            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, rect, 1);

            Material mat = GetPaintMaterial();

            PaintContext maskContext = null;
            if (m_TextureMask || m_TextureStencil)
            {
                TerrainLayer maskTerrainLayer = terrain.terrainData.terrainLayers[m_TextureMask ? m_maskIndex : m_stencilIndex];
                maskContext = TerrainPaintUtility.BeginPaintTexture(terrain, rect, maskTerrainLayer);
                if (maskContext == null)
                    return false;
                mat.SetTexture("_MaskTex", maskContext.sourceRenderTexture);
            }
            mat.SetInt("_MaskStencil", m_TextureMask ? 1 : (m_TextureStencil ? 2 : 0));

            // apply brush
            Vector4 brushParams = new Vector4(
                editContext.brushStrength,
                m_ErosionStrength,
                m_MixStrength,
                0.0f);

            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);

            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Masked Ridge Erode");

            if (maskContext != null)
            {
                TerrainPaintUtility.ReleaseContextResources(maskContext);
            }

            return false;
        }
    }
}
