using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using System;

namespace UnityEditor.Experimental.TerrainAPI
{
    public class DirectionalSmoothTool : TerrainPaintTool<DirectionalSmoothTool>
    {
        [NonSerialized] Material m_Material;

        Material GetPaintMaterial()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("TerrainTools/DirectionalSmooth"));
            return m_Material;
        }


        enum SmoothingDirections
        {
            Down = -1,
            Up = 1
        }

        [SerializeField] SmoothingDirections mode = SmoothingDirections.Down;

        public override string GetName()
        {
            return "Utility/Directional Smooth";
        }

        public override string GetDesc()
        {
            return "Regular smooth brush, but with Up/Down only behaviour\n\n" +
                "Ctrl [hold] to temporarily invert mode";
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(terrain, editContext.brushTexture, editContext.brushSize);
        }

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            EditorGUI.BeginChangeCheck();
            mode = (SmoothingDirections)EditorGUILayout.EnumPopup("Mode", mode);
            editContext.ShowBrushesGUI(0);
            if (EditorGUI.EndChangeCheck()) Save(true);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, 0.0f);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

            paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;

            Material mat = GetPaintMaterial();
            int currentMode = (int)mode;
            if (Event.current.control) currentMode *= -1;
            float modeValue = Mathf.Clamp01(currentMode);
            Vector4 brushParams = new Vector4(editContext.brushStrength, 0, modeValue, 0);
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);

            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Directional Smooth");
            return false;
        }
    }
}
