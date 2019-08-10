//Comments and suggestions to business@runswimfly.com

using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEditor.Experimental.TerrainAPI;

namespace UnityEditor.Experimental.TerrainAPI
{
    public class MaskedErodeHeightTool : TerrainPaintTool<MaskedErodeHeightTool>
    {
        [SerializeField]
        float m_FeatureSize;

        bool m_TextureMask = false;
        bool m_TextureStencil = false;
        int m_maskIndex = 0;
        int m_stencilIndex = 0;

        Material m_Material = null;
        Material GetPaintMaterial()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("RunSwimFlyTools/MaskedErodeHeight"));
            return m_Material;
        }

        public override string GetName()
        {
            return "RunSwimFly Tools/Masked Erode Height";
        }

        public override string GetDesc()
        {
            return "Click to erode the terrain height away from the local maxima.\n\nThe selected texture index will mask or stencil the operation.";
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
            m_FeatureSize = EditorGUILayout.Slider(new GUIContent("Detail Size", "Larger value will affect larger features, smaller values will affect smaller features"), m_FeatureSize, 1.0f, 100.0f);

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

            paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;

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

            Vector4 brushParams = new Vector4(editContext.brushStrength, 0.0f, 0.1f * m_FeatureSize, 0.0f);
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);

            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Masked Erode Height");

            if (maskContext != null)
            {
                TerrainPaintUtility.ReleaseContextResources(maskContext);
            }

            return false;
        }
    }
}
