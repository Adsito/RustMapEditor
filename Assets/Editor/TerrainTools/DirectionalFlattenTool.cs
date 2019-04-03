using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using System;

namespace UnityEditor.Experimental.TerrainAPI
{
    public class DirectionalFlattenTool : TerrainPaintTool<DirectionalFlattenTool>
    {
        [NonSerialized] Material m_Material;

        Material GetPaintMaterial() {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("TerrainTools/DirectionalFlatten"));
            return m_Material;
        }

        
        enum FlattenModes
        {
            Down = -1,
            Up = 1
        }

        [SerializeField] float height;
        [SerializeField] FlattenModes mode;

        public override string GetName()
        {
            return "Utility/Directional Flatten";
        }

        public override string GetDesc()
        {
            return "Regular flatten brush, but with Up/Down only behaviour\n\n" +
                "Shift + Click to set the target height.\n\n" +
                "Ctrl [hold] to temporarily invert mode";
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(terrain, editContext.brushTexture, editContext.brushSize);
        }

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            EditorGUI.BeginChangeCheck();
            height = EditorGUILayout.FloatField(new GUIContent("Target Height", "Shift to set"), height);
            mode = (FlattenModes)EditorGUILayout.EnumPopup("Mode", mode);
            editContext.ShowBrushesGUI(0);
            if (EditorGUI.EndChangeCheck()) Save(true);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            Vector2 uv = editContext.uv;

            if (Event.current.shift)
            {
                height = terrain.terrainData.GetInterpolatedHeight(uv.x, uv.y) / terrain.terrainData.size.y;
                return true;
            }

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, 0.0f);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);

            paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;

            Material mat = GetPaintMaterial();
            int currentMode = (int)mode;
            if (Event.current.control) currentMode *= -1;
            float modeValue = Mathf.Clamp01(currentMode);
            Vector4 brushParams = new Vector4(editContext.brushStrength, 0.5f*height, modeValue, 0);
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);

            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Directional Flatten Height");
            return false;
        }
    }
}
