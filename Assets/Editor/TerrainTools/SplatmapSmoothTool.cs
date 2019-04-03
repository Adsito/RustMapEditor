using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using System;

namespace UnityEditor.Experimental.TerrainAPI
{
    public class SplatmapSmoothTool : TerrainPaintTool<SplatmapSmoothTool>
    {
        [NonSerialized] Material m_Material;

        Material GetPaintMaterial()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("TerrainTools/SplatmapSmooth"));
            return m_Material;
        }

        public override string GetName()
        {
            return "Utility/Splatmap Smooth";
        }

        public override string GetDesc()
        {
            return "Blurs splatmaps";
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(terrain, editContext.brushTexture, editContext.brushSize);
        }

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            EditorGUI.BeginChangeCheck();
            editContext.ShowBrushesGUI(0);
            if (EditorGUI.EndChangeCheck()) Save(true);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            BrushTransform targetBrushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, 1);

            Material mat = GetPaintMaterial();
            Vector4 brushParams = new Vector4(editContext.brushStrength, 0, 0, 0f);
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);

            Rect targetRect = targetBrushXform.GetBrushXYBounds();
            int numSampleTerrainLayers = terrain.terrainData.terrainLayers.Length;

            for (int i = 0; i < numSampleTerrainLayers; ++i)
            {
                TerrainLayer layer = terrain.terrainData.terrainLayers[i];
                if (layer == null) continue;

                int layerIndex = TerrainPaintUtility.FindTerrainLayerIndex(terrain, layer);
                Texture2D layerTexture = TerrainPaintUtility.GetTerrainAlphaMapChecked(terrain, layerIndex >> 2);
                PaintContext targetContext = PaintContext.CreateFromBounds(terrain, targetRect, layerTexture.width, layerTexture.height);
                targetContext.CreateRenderTargets(RenderTextureFormat.R8);
                targetContext.GatherAlphamap(layer, true);
                Graphics.Blit(targetContext.sourceRenderTexture, targetContext.destinationRenderTexture, mat, 0);

                TerrainPaintUtility.EndPaintTexture(targetContext, "Terrain Paint - Smooth Splatmaps");
            }
            return false;
        }
    }
}
