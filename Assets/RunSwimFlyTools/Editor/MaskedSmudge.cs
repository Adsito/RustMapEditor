//Comments and suggestions to business@runswimfly.com

using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

namespace UnityEditor.Experimental.TerrainAPI
{
    public class MaskedSmudgeTool : TerrainPaintTool<MaskedSmudgeTool>
    {
        bool m_TextureMask = false;
        bool m_TextureStencil = false;
        int m_maskIndex = 0;
        int m_stencilIndex = 0;

        EventType m_PreviousEvent = EventType.Ignore;
        Vector2 m_PrevBrushPos = new Vector2(0.0f, 0.0f);

        Material m_Material = null;
        Material GetPaintMaterial()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("RunSwimFlyTools/MaskedSmudge"));
            return m_Material;
        }

        public override string GetName()
        {
            return "RunSwimFly Tools/Masked Smudge Height";
        }

        public override string GetDesc()
        {
            return "Click to Smudge the terrain height in the direction of the brush stroke.\n\nThe selected texture index will mask or stencil the operation.";
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

            SetMasks(terrain);

            editContext.ShowBrushesGUI(0);

            if (EditorGUI.EndChangeCheck())
                Save(true);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            if(Event.current.type == EventType.MouseDown)
            {
                m_PrevBrushPos = editContext.uv;
                return false;
            }
            
            if (Event.current.type == EventType.MouseDrag && m_PreviousEvent == EventType.MouseDrag) 
            {
                BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, 0.0f);
                PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

                Vector2 smudgeDir = editContext.uv - m_PrevBrushPos;

                paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;

                Material mat = GetPaintMaterial();

                PaintContext maskContext = null;
                if (m_TextureMask || m_TextureStencil)
                {
                    TerrainLayer maskTerrainLayer = terrain.terrainData.terrainLayers[m_TextureMask ? m_maskIndex : m_stencilIndex];
                    maskContext = TerrainPaintUtility.BeginPaintTexture(terrain, brushXform.GetBrushXYBounds(), maskTerrainLayer);
                    if (maskContext == null)
                        return false;
                    mat.SetTexture("_MaskTex", maskContext.sourceRenderTexture);
                }
                mat.SetInt("_MaskStencil", m_TextureMask ? 1 : (m_TextureStencil ? 2 : 0));

                Vector4 brushParams = new Vector4(editContext.brushStrength, smudgeDir.x, smudgeDir.y, 0);
                mat.SetTexture("_BrushTex", editContext.brushTexture);
                mat.SetVector("_BrushParams", brushParams);

                TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
                Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);

                TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Masked Smudge");
                if (maskContext != null)
                {
                    TerrainPaintUtility.ReleaseContextResources(maskContext);
                }

                m_PrevBrushPos = editContext.uv;
            }
            m_PreviousEvent = Event.current.type;
            return false;
        }
    }
}
