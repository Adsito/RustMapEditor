//Comments and suggestions to business@runswimfly.com

using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEditor.Experimental.TerrainAPI;

namespace UnityEditor.Experimental.TerrainAPI
{
    public class MaskedTerraceErosion : TerrainPaintTool<MaskedTerraceErosion>
    {
        [SerializeField]
        float m_FeatureSize = 150.0f;

        [SerializeField]
        float m_BevelAmountInterior = 0.0f;

        bool m_TextureMask = false;
        bool m_TextureStencil = false;
        int m_maskIndex = 0;
        int m_stencilIndex = 0;

        Material m_Material = null;
        Material GetPaintMaterial()
        {
                m_Material = new Material(Shader.Find("RunSwimFlyTools/MaskedTerraceErosion"));
            return m_Material;
        }

        public override string GetName()
        {
            return "RunSwimFly Tools/Masked Terrace Erosion";
        }

        public override string GetDesc()
        {
            return "Use to terrace terrain.\n\nThe selected texture index will mask or stencil the operation.";
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(terrain,
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
            m_FeatureSize = EditorGUILayout.Slider(new GUIContent("Terrace Count", "Larger value will result in more terraces"), m_FeatureSize, 2.0f, 1000.0f);
            m_BevelAmountInterior = EditorGUILayout.Slider(new GUIContent("Interior Corner Weight", "Amount to retain the original height in each interior corner of the terrace steps"), m_BevelAmountInterior, 0.0f, 1.0f);

            SetMasks(terrain);

            editContext.ShowBrushesGUI(0);

            if (EditorGUI.EndChangeCheck())
                Save(true);
        }

        private void ApplyBrushInternal(PaintContext paintContext, float brushStrength, Texture brushTexture, BrushTransform brushXform, Material mat)
        {
            Vector4 brushParams = new Vector4(brushStrength, m_FeatureSize, m_BevelAmountInterior, 0.0f);
            mat.SetTexture("_BrushTex", brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, 0.0f);
            Rect rect = brushXform.GetBrushXYBounds();

            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, rect);
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

            ApplyBrushInternal(paintContext, editContext.brushStrength, editContext.brushTexture, brushXform, mat);

            if (maskContext != null)
            {
                TerrainPaintUtility.ReleaseContextResources(maskContext);
            }

            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Masked Terrace Erosion");
            return false;
        }
    }
}
