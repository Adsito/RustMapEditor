//Comments and suggestions to business@runswimfly.com

using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using System.Collections.Generic;
using System;

namespace UnityEditor.Experimental.TerrainAPI 
{
    public class MaskedTexture : TerrainPaintTool<MaskedTexture>
    {
        [SerializeField]

        public class TextureParam
        {
            public bool slopeMask;
            public bool heightMask;
            public bool textureMask;
            public bool textureStencil;
            public float maxValue;
            public float slopeMaxPoint;
            public float slopeExtent;
            public float slopeBlend;
            public float heightMaxPoint;
            public float heightExtent;
            public float heightBlend;
            public int maskIndex;
            public int stencilIndex;
            public bool heightRelative;
            public float heightFeatureSize;
        }

        List<TextureParam> textureParams = new List<TextureParam>();
        string m_desc;

        bool disablePaint = false;
        bool drawLine = false;

        int m_textureIndex = 0;

        Material m_Material = null;

        TerrainLayer m_SelectedTerrainLayer = null;
        Vector3 anchorPoint;

        const float relativeHeightMult = 40.0f;


        private void AddTextureParam(float maxHeight)
        {
            TextureParam param = new TextureParam();
            param.slopeMask = false;
            param.heightMask = false;
            param.textureMask = false;
            param.textureStencil = false;
            param.maxValue = 1.0f;
            param.slopeMaxPoint = 0.5f;
            param.slopeExtent = 0.4f;
            param.slopeBlend = 0.5f;
            param.heightMaxPoint = 0.3f*maxHeight;
            param.heightExtent = 0.3f*maxHeight;
            param.heightBlend = 0.5f;
            param.maskIndex = 0;
            param.stencilIndex = 0;
            param.heightRelative = false;
            param.heightFeatureSize = 10.0f;

            textureParams.Add(param);
        }

        Material GetPaintMaterial()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("RunSwimFlyTools/MaskedTexture"));
            return m_Material;
        }

        public override string GetName()
        {
            return "RunSwimFly Tools/Masked Paint Texture";
        }

        public override string GetDesc()
        {
            return m_desc;
        }

        private void RepaintInspector()
        {
            Editor[] ed = (Editor[])Resources.FindObjectsOfTypeAll<Editor>();

            for (int i = 0; i < ed.Length; ++i)
            {
                ed[i].Repaint();
            }
        }

        private Vector2 TerrainUVFromBrushLocation(Terrain terrain, Vector3 posWS)
        {
            // position relative to Terrain-space. doesnt handle rotations,
            // since that's not really supported at the moment
            Vector3 posTS = posWS - terrain.transform.position;
            Vector3 size = terrain.terrainData.size;

            return new Vector2(posTS.x / size.x, posTS.z / size.z);
        }

        private float SlopeAtPoint(Terrain terrain, Vector2 uv)
        {
            const float PI = 3.14159f;

            Vector3 normal = terrain.terrainData.GetInterpolatedNormal(uv.x, uv.y);
            return Mathf.Acos(normal.y) * 2.0f / PI;
        }

        private float HeightAtPoint(Terrain terrain, Vector2 uv)
        {
            return terrain.terrainData.GetInterpolatedHeight(uv.x, uv.y);
        }

        private float RelativeHeightAtPoint(Terrain terrain, Vector2 uv)
        {
            float height = HeightAtPoint(terrain, uv);
            float offsetX = textureParams[m_textureIndex].heightFeatureSize / terrain.terrainData.size.x;
            float offsetY = textureParams[m_textureIndex].heightFeatureSize / terrain.terrainData.size.z;
            float difference = height - HeightAtPoint(terrain, uv + new Vector2(-offsetX, 0.0f));
            difference += height - HeightAtPoint(terrain, uv + new Vector2(offsetX, 0.0f));
            difference += height - HeightAtPoint(terrain, uv + new Vector2(0.0f, -offsetY));
            difference += height - HeightAtPoint(terrain, uv + new Vector2(0.0f, offsetY));
            difference += height - HeightAtPoint(terrain, uv + new Vector2(offsetX * 0.707f, offsetY * 0.707f));
            difference += height - HeightAtPoint(terrain, uv + new Vector2(offsetX * -0.707f, offsetY * 0.707f));
            difference += height - HeightAtPoint(terrain, uv + new Vector2(offsetX * 0.707f, offsetY * -0.707f));
            difference += height - HeightAtPoint(terrain, uv + new Vector2(offsetX * -0.707f, offsetY * -0.707f));
            return (difference* relativeHeightMult / (textureParams[m_textureIndex].heightFeatureSize*terrain.terrainData.size.y));
        }

        private float GetMaxExtent(Terrain terrain, Vector3 origin, Vector3 target, out float extent, 
        Func<Terrain, Vector2, float> Sampler)
        {
            const int numSamples = 50;

            drawLine = true;

            Vector2 uvOrigin = TerrainUVFromBrushLocation(terrain, origin);
            Vector2 uvTarget = TerrainUVFromBrushLocation(terrain, target);

            float max = -1.0E10f;
            float min = 1.0E10f;

            for (int i=0; i< numSamples; i++)
            {
                Vector2 uv = Vector2.Lerp(uvOrigin, uvTarget, (float)i / (float)(numSamples - 1));

                float sample = Sampler(terrain,uv);
                min = Mathf.Min(min, sample);
                max = Mathf.Max(max, sample);
            }
            extent = (max - min) * 0.5f;
            return (max + min) * 0.5f;
        }

        private void SampleTerrain(Terrain terrain, IOnSceneGUI editContext)
        {
            if (!editContext.hitValidTerrain)
            {
                return;
            }
            Vector2 uv = TerrainUVFromBrushLocation(terrain, editContext.raycastHit.point);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                disablePaint = true;
                if (!textureParams[m_textureIndex].heightMask && !textureParams[m_textureIndex].slopeMask)
                    Debug.Log("A height or slope mask needs to be enabled for sampling to occur.");
                else
                {
                    if (textureParams[m_textureIndex].heightMask)
                    {
                        if (textureParams[m_textureIndex].heightRelative)
                            textureParams[m_textureIndex].heightMaxPoint = RelativeHeightAtPoint(terrain, uv);
                        else
                            textureParams[m_textureIndex].heightMaxPoint = HeightAtPoint(terrain, uv);
                    }
                    if (textureParams[m_textureIndex].slopeMask)
                    {
                        textureParams[m_textureIndex].slopeMaxPoint = SlopeAtPoint(terrain, uv);
                    }
                }
                anchorPoint = editContext.raycastHit.point;
            }

            if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
            {
                if (textureParams[m_textureIndex].heightMask)
                {
                    if (textureParams[m_textureIndex].heightRelative)
                    {
                        textureParams[m_textureIndex].heightMaxPoint = GetMaxExtent(terrain, anchorPoint,
                         editContext.raycastHit.point, out textureParams[m_textureIndex].heightExtent, RelativeHeightAtPoint);
                    }
                    else
                    {
                        textureParams[m_textureIndex].heightMaxPoint = GetMaxExtent(terrain, anchorPoint,
                         editContext.raycastHit.point, out textureParams[m_textureIndex].heightExtent, HeightAtPoint);
                    }

                }
                if (textureParams[m_textureIndex].slopeMask)
                {
                    textureParams[m_textureIndex].slopeMaxPoint = GetMaxExtent(terrain, anchorPoint, editContext.raycastHit.point,
                    out textureParams[m_textureIndex].slopeExtent, SlopeAtPoint);
                }
            }

            if (drawLine)
                Handles.DrawLine(anchorPoint, editContext.raycastHit.point);

            RepaintInspector();
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            if (Event.current.shift)
            {
                SampleTerrain(terrain, editContext);
            }
            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                disablePaint = false;
                drawLine = false;
            }

            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(terrain,
                                                              editContext.brushTexture,
                                                              editContext.brushSize);
        }

        void FillButton(Terrain terrain)
        {
            Event e = Event.current;

            GUIContent buttonText = new GUIContent("Fill");
            Rect buttonRect = GUILayoutUtility.GetRect(buttonText, GUIStyle.none);

            if (e.isMouse && buttonRect.Contains(e.mousePosition))
            {
                if (e.type == EventType.MouseDown)
                {
                    Rect rect = new Rect(0.0f, 0.0f, terrain.terrainData.size.x, terrain.terrainData.size.z);
     
                    BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, Vector2.one * 0.5f,
                     Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z), 0.0f);
                    DoPaint(terrain, 2.0f, null, rect, brushXform);
                    terrain.terrainData.SetBaseMapDirty();
                 }
            }

            GUI.Button(buttonRect, buttonText);
        }

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            if (terrain.normalmapTexture == null)
                m_desc = "Please check the Draw Instanced option under terrain settings to use this tool";
            else
                m_desc = "Left click to apply a texture.\n\n" +
                	"Hold shift and left click to sample into an enabled mask.\n\n" +
                	"Hold shift, left click and drag to set both\nthe maximum and extent for an enabled mask";

            EditorGUI.BeginChangeCheck();

            while (textureParams.Count > terrain.terrainData.terrainLayers.Length)
                textureParams.RemoveAt(textureParams.Count - 1);
            while (textureParams.Count < terrain.terrainData.terrainLayers.Length)
                AddTextureParam(terrain.terrainData.size.y);

            m_textureIndex = EditorGUILayout.IntSlider(new GUIContent("Texture Index", "Select the index of the texture to be applied"), m_textureIndex, 0, terrain.terrainData.terrainLayers.Length - 1);
            EditorGUILayout.LabelField(terrain.terrainData.terrainLayers[m_textureIndex].name);

            textureParams[m_textureIndex].maxValue = EditorGUILayout.Slider(new GUIContent("Maximum Value", "If texture intensity is above this value no more will be added."), textureParams[m_textureIndex].maxValue, 0.0f, 1.0f);

            textureParams[m_textureIndex].slopeMask = EditorGUILayout.Toggle(new GUIContent("Use Slope Mask", "Toggles whether slope will be used to mask the operation."), textureParams[m_textureIndex].slopeMask);
            if (textureParams[m_textureIndex].slopeMask)
            {
                textureParams[m_textureIndex].slopeMaxPoint = EditorGUILayout.Slider(new GUIContent("Slope Maximum Point", "The terrain slope at which the texture is applied most intensely"), textureParams[m_textureIndex].slopeMaxPoint, 0.0f, 1.0f);
                textureParams[m_textureIndex].slopeExtent = EditorGUILayout.Slider(new GUIContent("Slope Extent", "The slope range over which the brush will take effect."), textureParams[m_textureIndex].slopeExtent, 0.0f, 1.0f);
                textureParams[m_textureIndex].slopeBlend = EditorGUILayout.Slider(new GUIContent("Slope Blend", "Controls how smoothly brush intensity drops as the height value moves further from its maximum point."), textureParams[m_textureIndex].slopeBlend, 0.0f, 1.0f);
            }

            EditorGUILayout.BeginHorizontal();
            textureParams[m_textureIndex].heightMask = EditorGUILayout.Toggle(new GUIContent("Use Height Mask", "Toggles whether height will be used to stencil the operation."), textureParams[m_textureIndex].heightMask);
            textureParams[m_textureIndex].heightRelative = EditorGUILayout.Toggle(new GUIContent("Relative", "Toggles whether relative or absolute height is used."), textureParams[m_textureIndex].heightRelative);
            EditorGUILayout.EndHorizontal();
            if (textureParams[m_textureIndex].heightMask)
            {
                bool relative = textureParams[m_textureIndex].heightRelative;
                if (relative)
                {
                    textureParams[m_textureIndex].heightFeatureSize = EditorGUILayout.Slider(new GUIContent("Detail Size", "Larger value will affect larger features, smaller values will affect smaller features"), textureParams[m_textureIndex].heightFeatureSize, 1.0f, 100.0f);
                }
                textureParams[m_textureIndex].heightMaxPoint = EditorGUILayout.Slider(new GUIContent("Height Max", "The terrain height at which the texture is applied most intensely"), textureParams[m_textureIndex].heightMaxPoint, relative?-1.0f:0.0f, relative?1.0f:terrain.terrainData.size.y);
                textureParams[m_textureIndex].heightExtent = EditorGUILayout.Slider(new GUIContent("Height Extent", "The height range over which the brush will take effect."), textureParams[m_textureIndex].heightExtent, 0.0f, relative?1.0f:terrain.terrainData.size.y);
                textureParams[m_textureIndex].heightBlend = EditorGUILayout.Slider(new GUIContent("Height Blend", "Controls how smoothly brush intensity drops as the height value moves further from its maximum point."), textureParams[m_textureIndex].heightBlend, 0.0f, 1.0f);
            }

            EditorGUILayout.BeginHorizontal();
            bool oldMask = textureParams[m_textureIndex].textureMask;
            textureParams[m_textureIndex].textureMask = EditorGUILayout.Toggle(new GUIContent("Use Texture Mask", "Toggles whether a second texture will be used to mask the operation."), textureParams[m_textureIndex].textureMask);
            textureParams[m_textureIndex].textureStencil = EditorGUILayout.Toggle(new GUIContent("Use Texture Stencil", "Toggles whether a second texture will be used to stencil the operation."), textureParams[m_textureIndex].textureStencil);
            if (textureParams[m_textureIndex].textureMask && !oldMask)
                textureParams[m_textureIndex].textureStencil = false;
            if (textureParams[m_textureIndex].textureStencil)
                textureParams[m_textureIndex].textureMask = false;
            EditorGUILayout.EndHorizontal();

            if (textureParams[m_textureIndex].textureMask)
            {
                textureParams[m_textureIndex].maskIndex = EditorGUILayout.IntSlider(new GUIContent("Mask Index", "Select the index of the texture to be used as a mask"), textureParams[m_textureIndex].maskIndex, 0, terrain.terrainData.terrainLayers.Length - 1);
                EditorGUILayout.LabelField(terrain.terrainData.terrainLayers[textureParams[m_textureIndex].maskIndex].name);
            }

            if (textureParams[m_textureIndex].textureStencil)
            { 
                textureParams[m_textureIndex].stencilIndex = EditorGUILayout.IntSlider(new GUIContent("Stencil Index", "Select the index of the texture to be used as a stencil"), textureParams[m_textureIndex].stencilIndex, 0, terrain.terrainData.terrainLayers.Length - 1);
                EditorGUILayout.LabelField(terrain.terrainData.terrainLayers[textureParams[m_textureIndex].stencilIndex].name);
            }

            FillButton(terrain);

            editContext.ShowBrushesGUI(0);

            if (EditorGUI.EndChangeCheck())
                Save(true);


        }

        private bool DoPaint(Terrain terrain, float brushStrength, Texture brushTexture, Rect rect, BrushTransform brushXform)
        {
            if (disablePaint)
            {
                return false;
            }
           
            m_SelectedTerrainLayer = terrain.terrainData.terrainLayers[m_textureIndex];
            PaintContext paintContext = TerrainPaintUtility.BeginPaintTexture(terrain, rect, m_SelectedTerrainLayer);
            if (paintContext == null)
                return false;

            PaintContext heightContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, rect, 1);
            if (heightContext == null)
                return false;

            if (terrain.normalmapTexture == null)
            {
                Debug.Log("Please check the Draw Instanced option under terrain settings to use this tool");
                return false;
            }
            PaintContext normalContext = TerrainPaintUtility.CollectNormals(terrain, rect, 0);

            Material mat = GetPaintMaterial();

            float extent = textureParams[m_textureIndex].heightExtent;
            float blend = textureParams[m_textureIndex].heightBlend;
            float height = textureParams[m_textureIndex].heightMaxPoint;
            if (textureParams[m_textureIndex].heightRelative)
            {
                extent /= relativeHeightMult;
                height /= relativeHeightMult;
            }
            else
            {
                extent /= terrain.terrainData.size.y;
                height /= terrain.terrainData.size.y;
            }
            Vector4 brushParams = new Vector4(brushStrength, height, 
            textureParams[m_textureIndex].heightMask ? extent * (1.0f - blend) : 10000.0f, extent * blend);

            extent = textureParams[m_textureIndex].slopeExtent;
            blend = textureParams[m_textureIndex].slopeBlend;
            Vector4 slopeParams = new Vector4(textureParams[m_textureIndex].slopeMaxPoint, 
            textureParams[m_textureIndex].slopeMask ? extent * (1.0f - blend) : 1.0f, extent * blend, textureParams[m_textureIndex].maxValue);

            PaintContext maskContext = null;

            if (textureParams[m_textureIndex].textureMask || textureParams[m_textureIndex].textureStencil)
            {
                TerrainLayer maskTerrainLayer = terrain.terrainData.terrainLayers[textureParams[m_textureIndex].textureMask ? textureParams[m_textureIndex].maskIndex : textureParams[m_textureIndex].stencilIndex];
                maskContext = TerrainPaintUtility.BeginPaintTexture(terrain, rect, maskTerrainLayer);
                if (maskContext == null)
                    return false;
                mat.SetTexture("_MaskTex", maskContext.sourceRenderTexture);
            }
            mat.SetInt("_MaskStencil", textureParams[m_textureIndex].textureMask ? 1 : (textureParams[m_textureIndex].textureStencil ? 2 : 0));

            mat.SetTexture("_BrushTex", brushTexture);

            mat.SetTexture("_HeightTex", heightContext.sourceRenderTexture);
            mat.SetTexture("_NormalTex", normalContext.sourceRenderTexture);

            mat.SetVector("_BrushParams", brushParams);
            mat.SetVector("_SlopeParams", slopeParams);
            mat.SetFloat("_FeatureSize", textureParams[m_textureIndex].heightRelative?textureParams[m_textureIndex].heightFeatureSize:0.0f);

            if (brushTexture == null)
            {
                mat.SetFloat("_AspectRatio", terrain.terrainData.size.x / terrain.terrainData.size.z);
            }
            else
                mat.SetFloat("_AspectRatio", (float)paintContext.sourceRenderTexture.height / (float)paintContext.sourceRenderTexture.width);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
            TerrainPaintUtility.EndPaintTexture(paintContext, "Terrain Paint - Masked Texture");
            TerrainPaintUtility.ReleaseContextResources(heightContext);
            TerrainPaintUtility.ReleaseContextResources(normalContext);
            if (maskContext!=null)
            {
                TerrainPaintUtility.ReleaseContextResources(maskContext);
            }

            return true;
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, editContext.brushSize, 0.0f);

            return DoPaint(terrain, editContext.brushStrength, editContext.brushTexture, brushXform.GetBrushXYBounds(), brushXform);
        }
    }
}
