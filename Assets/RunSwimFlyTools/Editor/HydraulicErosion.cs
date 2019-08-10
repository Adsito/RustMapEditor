//Comments and suggestions to business@runswimfly.com

using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

namespace UnityEditor.Experimental.TerrainAPI
{
    [ExecuteAlways, UnityEditor.InitializeOnLoad]

    public class HydraulicErosion : TerrainPaintTool<HydraulicErosion>
    {
        GameObject renderSurface;
        Material renderMaterial;

        [SerializeField]

        bool m_TextureMask = false;
        bool m_TextureStencil = false;
        int m_maskIndex = 0;

        float m_Erosion = 0.5f;
        float m_Slip = 0.5f;
        float m_Smoothing = 0.5f;
        float m_Evaporation = 0.5f;
        float m_Viscosity = 0.1f;
        float m_Opacity = 0.7f;
        float m_Extent = 0.4f;
        float m_SimSpeed = 0.5f;
        float m_BrushVel = 0.0f;
        int m_Steps = 1000;


        int m_stencilIndex = 0;
        int stepsRemaining = 0;
        int nextErode = 0;
        private float m_brushStrength;
        PaintContext heightContext;

        PaintContext maskContext = null;
        private int src = 0;
        private RenderTexture[] soilWater;
        private bool continuing = false;

        private bool m_lmb;

        private Vector2 uvBase;
        private float oldBrushSize = 0.0f;

        Vector2 m_PrevBrushPos = new Vector2(0.0f, 0.0f);


        private Terrain oldTerrain = null;

        Material m_Material = null;

        string m_desc;

        void GetPaintMaterials()
        {
            if (m_Material == null)
                m_Material = new Material(Shader.Find("RunSwimFlyTools/HydraulicErosion"));
        }


        public override string GetName()
        {
            return "RunSwimFly Tools/HydraulicErosion";
        }

        public override string GetDesc()
        {
            return m_desc;
        }

        private void ProcessInput(Terrain terrain, IOnSceneGUI editContext)
        {
            // update Left Mouse Button state
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && editContext.hitValidTerrain)
                m_lmb = !Event.current.command && !Event.current.alt && !Event.current.control;
            else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                m_lmb = false;

            if (m_lmb)
            {
                m_Steps = (int)((1.1f - m_Evaporation) * 10000.0f);
                stepsRemaining = m_Steps;
            }

            if (Event.current.shift && !m_lmb)
            {
                stepsRemaining = Mathf.Min(stepsRemaining, 1);
            }
        }

        public override void OnSceneGUI(Terrain terrain, IOnSceneGUI editContext)
        {
            ProcessInput(terrain, editContext);

            TerrainPaintUtilityEditor.ShowDefaultPreviewBrush(terrain,
                                                              editContext.brushTexture,
                                                              editContext.brushSize);
        }

        public override void OnInspectorGUI(Terrain terrain, IOnInspectorGUI editContext)
        {
            m_desc = "";
            if (!terrain.drawInstanced)
                m_desc += "Please check the Draw Instanced option under\nterrain settings to optimise use of this tool\n\n";
            m_desc += "Left mouse to add water to the scene.\n\nPress shift or left mouse outside of the\n" +
                "simulation area to remove all water.\n\nThe selected texture index will mask or stencil the operation.";

            EditorGUI.BeginChangeCheck();

            m_Erosion = EditorGUILayout.Slider(new GUIContent("Erosion", "Controls the rate at which fluid flow erodes height."), m_Erosion, 0, 1.0f);
            m_Slip = EditorGUILayout.Slider(new GUIContent("Slip", "Controls the rate at which fluid drags height in its direction of motion."), m_Slip, 0, 1.0f);
            m_Smoothing = EditorGUILayout.Slider(new GUIContent("Smoothing", "Controls the level of smoothing applied to eroded regions."), m_Smoothing, 0, 1.0f);
            m_Evaporation = EditorGUILayout.Slider(new GUIContent("Evaporation", "Set Evaporation Rate."), m_Evaporation, 0, 1.0f);
            m_Viscosity = EditorGUILayout.Slider(new GUIContent("Viscosity", "Set fluid viscosity."), m_Viscosity, 0, 1.0f);
            m_Extent = EditorGUILayout.Slider(new GUIContent("Extent", "Set simulation area."), m_Extent, 0.0f, 1.0f);
            m_SimSpeed = EditorGUILayout.Slider(new GUIContent("Sim Speed", "Set simulation speed."), m_SimSpeed, 0.0f, 1.0f);
            m_Opacity = EditorGUILayout.Slider(new GUIContent("Fluid Opacity", "Set fluid visual opacity."), m_Opacity, 0.2f, 1.0f);
            m_BrushVel = EditorGUILayout.Slider(new GUIContent("Brush Velocity", "Amount of water velocity inherited from brush."), m_BrushVel, 0.0f, 1.0f);

            EditorGUILayout.BeginHorizontal();
            bool oldMask = m_TextureMask;
            bool oldStencil = m_TextureStencil;
            m_TextureMask = EditorGUILayout.Toggle(new GUIContent("Texture Mask", "Toggles whether a second texture will be used to mask the operation."), m_TextureMask);
            m_TextureStencil = EditorGUILayout.Toggle(new GUIContent("Texture Stencil", "Toggles whether a second texture will be used to stencil the operation."), m_TextureStencil);
            if (m_TextureMask && !oldMask)
                m_TextureStencil = false;
            if (m_TextureStencil)
                m_TextureMask = false;
            if (m_TextureMask!=oldMask || m_TextureStencil!=oldStencil)
                stepsRemaining = Mathf.Min(stepsRemaining, 1);

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

            editContext.ShowBrushesGUI(0);

            if (EditorGUI.EndChangeCheck())
                Save(true);
        }

        private Vector3 UVtoPos(Terrain terrain, Vector2 uv)
        {
            Vector3 size = terrain.terrainData.size;

            Vector3 vec = new Vector3(uv.x * size.x, 0.0f, uv.y * size.z);
            return vec + terrain.transform.position;
        }

        void CreateMesh(int vertX, int vertZ, Vector2 size, float zOffset, float zStart, float zSpan)
        {
            Vector3[] newVertices;
            Vector2[] newUV;
            int[] tri;

            GameObject meshObject = new GameObject();
            meshObject.transform.parent = renderSurface.transform;
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localRotation = Quaternion.identity;
            meshObject.AddComponent<MeshFilter>();
            meshObject.AddComponent<MeshRenderer>();

            newVertices = new Vector3[vertX * vertZ];

            newUV = new Vector2[vertX * vertZ];


            for (int z = 0; z < vertZ; z++)
            {
                float interpZ = (float)z / (float)(vertZ - 1);
                for (int x = 0; x < vertX; x++)
                {
                    float interpX = (float)x / (float)(vertX - 1);

                    newVertices[z * vertX + x] = new Vector3(Mathf.Lerp(size.x, -size.x, interpX), 0.0f, Mathf.Lerp(size.y, -size.y, interpZ) + zOffset);
                    newUV[z * vertX + x] = new Vector2(interpX, zStart + interpZ * zSpan);
                }
            }


            tri = new int[(vertZ - 1) * (vertX - 1) * 6];
            for (int z = 0; z < vertZ - 1; z++)
            {
                for (int x = 0; x < vertX - 1; x++)
                {
                    int triStart = (z * (vertX - 1) + x) * 6;

                    tri[triStart] = z * vertX + x;
                    tri[triStart + 1] = (z + 1) * vertX + x;
                    tri[triStart + 2] = z * vertX + x + 1;
                    tri[triStart + 3] = (z + 1) * vertX + x;
                    tri[triStart + 4] = (z + 1) * vertX + x + 1;
                    tri[triStart + 5] = z * vertX + x + 1;
                }
            }

            Mesh mesh = new Mesh();

            mesh.vertices = newVertices;
            mesh.uv = newUV;
            mesh.triangles = tri;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;


        }

        void DestroyMeshes()
        {
            int childNum = renderSurface.transform.childCount;
            for (int i = childNum - 1; i >= 0; i--)
            {
                DestroyImmediate(renderSurface.transform.GetChild(i).gameObject);
            }
        }

        void CreateMeshes(int vertX, int vertZ, Vector2 size)
        {
            DestroyMeshes();

            int total = vertX * vertZ;
            int rows = 0;
            float rowSize = 2.0f * size.y / (float)vertZ;
            while (rows < vertZ)
            {
                int rowsToAdd = Mathf.Min(vertZ - rows, 65535 / vertX);
                float zStart = (float)rows / (float)vertZ;
                float zSpan = (float)rowsToAdd / (float)vertZ;
                float zOffset = (float)rows + (float)(rowsToAdd - vertZ) * 0.5f;
                CreateMesh(vertX, rowsToAdd, new Vector2(size.x, rowSize * 0.5f * (float)rowsToAdd), -zOffset * rowSize, zStart, zSpan);
                rows += rowsToAdd;
            }
        }


        private Vector2 BrushUVDelta(Vector2 newUV, Terrain terrain, int width, int height)
        {
            Vector2 waterShift = (uvBase - newUV) * (float)terrain.terrainData.heightmapResolution;
            waterShift.x /= soilWater[0].width;
            waterShift.y /= soilWater[0].height;
            return waterShift;
        }

        private void InitWaterSim(int width, int height)
        {
            soilWater = new RenderTexture[2];
            for (int j = 0; j < 2; j++)
            {
                soilWater[j] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
                soilWater[j].filterMode = FilterMode.Bilinear;
                soilWater[j].wrapMode = TextureWrapMode.Clamp;
            }
        }

        private void InitHeightTextures(Terrain terrain, Rect rect)
        {
            heightContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, rect, 1);
            heightContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
            heightContext.destinationRenderTexture.filterMode = FilterMode.Bilinear;
            heightContext.sourceRenderTexture.wrapMode = TextureWrapMode.Clamp;
            heightContext.destinationRenderTexture.wrapMode = TextureWrapMode.Clamp;
        }

        private void CreateDisplay()
        {
            string objectName = "RSFWaterObject";

            renderSurface = GameObject.Find(objectName);
            if (renderSurface == null)
            {
                renderSurface = new GameObject(objectName);
            }
            renderSurface.transform.eulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
        }

        private void InitDisplay(Terrain terrain, Vector2 extents, Vector2 uv)
        {
            CreateDisplay();
            renderMaterial = new Material(Shader.Find("RunSwimFlyTools/WaterDisplay"));
            renderMaterial.SetVector("_Size", terrain.terrainData.size);
            renderMaterial.SetFloat("_Resolution", terrain.terrainData.heightmapResolution);

            renderSurface.transform.position = UVtoPos(terrain, uv);// + Vector3.up * 10.0f;
            CreateMeshes(soilWater[0].width, soilWater[0].height, extents * 0.5f);
            Renderer[] renderers = renderSurface.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.material = renderMaterial;
            }
        }

        private void SetMaterial(Material mat, IOnPaint editContext)
        {
            mat.SetTexture("_BrushTex", editContext.brushTexture);
            mat.SetInt("_MaskStencil", m_TextureMask ? 1 : (m_TextureStencil ? 2 : 0));
            if (maskContext != null)
                mat.SetTexture("_MaskTex", maskContext.sourceRenderTexture);
        }

        public override bool OnPaint(Terrain terrain, IOnPaint editContext)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                m_PrevBrushPos = editContext.uv;
            }

            float min = Mathf.Min(terrain.terrainData.size.x, terrain.terrainData.size.z);

            float extent = editContext.brushSize * Mathf.Lerp(1.2f, 1.0f, 1.0f - m_Extent) + min * Mathf.Lerp(0.1f, 1.0f, m_Extent);
            m_brushStrength = editContext.brushStrength;

            Vector2 waterVel = (editContext.uv - m_PrevBrushPos)*m_BrushVel*-10000.0f;
            m_PrevBrushPos = editContext.uv;

            GetPaintMaterials();

            m_Material.SetVector("_WaterVel", waterVel);

            if (continuing && editContext.brushSize == oldBrushSize && terrain == oldTerrain)
            {
                Vector2 shift = BrushUVDelta(editContext.uv, terrain, soilWater[0].width, soilWater[0].height);

                if (Mathf.Max(Mathf.Abs(shift.x), Mathf.Abs(shift.y)) < 0.5f)
                {
                    m_Material.SetVector("_WaterShift", shift);
                    return true;
                }
            }

            oldTerrain = terrain;

            oldBrushSize = editContext.brushSize;
            continuing = true;

            m_Material.SetFloat("_HeightScale", terrain.terrainData.size.y);

            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, editContext.uv, extent, 0.0f);
            Rect rect = brushXform.GetBrushXYBounds();

            Vector2 oldUV = rect.center; 
            oldUV.x *= 0.5f / terrain.terrainData.bounds.extents.x;
            oldUV.y *= 0.5f / terrain.terrainData.bounds.extents.z;

             if (terrain.leftNeighbor == null)
                rect.x = Mathf.Max(0.0f, rect.x);
            if (terrain.bottomNeighbor == null)
                rect.y = Mathf.Max(0.0f, rect.y);
            if (terrain.leftNeighbor == null && terrain.rightNeighbor == null)
                rect.width = Mathf.Min(rect.width, terrain.terrainData.bounds.extents.x * 2.0f);
            if (terrain.topNeighbor == null && terrain.bottomNeighbor == null)
                rect.height = Mathf.Min(rect.height, terrain.terrainData.bounds.extents.z * 2.0f);
            if (terrain.rightNeighbor == null)
                rect.x = Mathf.Min(rect.x, terrain.terrainData.bounds.extents.x * 2.0f - rect.width);
            if (terrain.topNeighbor == null)
                rect.y = Mathf.Min(rect.y, terrain.terrainData.bounds.extents.z * 2.0f - rect.height);

            Vector2 uv = rect.center;
            uv.x *= 0.5f / terrain.terrainData.bounds.extents.x;
            uv.y *= 0.5f / terrain.terrainData.bounds.extents.z;

            if (m_TextureMask || m_TextureStencil)
            {
                TerrainLayer maskTerrainLayer = terrain.terrainData.terrainLayers[m_TextureMask ? m_maskIndex : m_stencilIndex];
                maskContext = TerrainPaintUtility.BeginPaintTexture(terrain, rect, maskTerrainLayer);
                if (maskContext == null)
                    return false;
            }

            SetMaterial(m_Material, editContext);

            src = 0;

            InitHeightTextures(terrain, rect);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(heightContext, brushXform, m_Material);

            InitWaterSim(heightContext.sourceRenderTexture.width, heightContext.sourceRenderTexture.height);
            InitDisplay(terrain, new Vector2(rect.width,rect.height), uv);

            float interp = editContext.brushSize / extent;

            uvBase = uv * (1.0f - interp) + oldUV * interp;
            
            Vector2 shiftStart = BrushUVDelta(editContext.uv, terrain, soilWater[0].width, soilWater[0].height);

            m_Material.SetVector("_WaterShift", shiftStart);
            m_Material.SetFloat("_Extent", extent / editContext.brushSize);

            return false;
        }

        private void Erode()
        {
            const int evapStart = 500;
            stepsRemaining--;

            float waterRetain = stepsRemaining > evapStart ? 1.0f : Mathf.Max(0.0f, Mathf.Pow((float)(stepsRemaining - 1) / (float)evapStart, 0.005f));
            waterRetain *= 1.0f - Mathf.Pow(m_Evaporation, 4.0f) * 0.008f;
            Vector4 brushParams = new Vector4(m_brushStrength * m_brushStrength, waterRetain, (stepsRemaining > m_Steps - 5) ? 0.06f : 0.0f, 0.0f);
            m_Material.SetVector("_BrushParams", brushParams);


            Vector4 simParams = new Vector4(Mathf.Pow(m_Erosion,2.0f), Mathf.Pow(m_Slip,2.0f), Mathf.Pow(m_Smoothing, 2.0f), m_Viscosity);
            //small erosion and slip levels fall within the terrain height precision so we boost them and apply them less often
            if (--nextErode <=0) 
            {
                float sum = simParams.x + simParams.y;
                nextErode = (int)(5.0f / (sum+0.25f));
                float mult = (float)nextErode;
                simParams = Vector4.Scale(simParams, new Vector4(mult, mult, 1.0f, 1.0f));
            }
            else
            {
                simParams = Vector4.Scale(simParams, new Vector4(0.0f, 0.0f, 1.0f, 1.0f));
            }
            m_Material.SetVector("_SimParams", simParams);

            m_Material.SetTexture("_WaterSoilTex", soilWater[src]);

            RenderBuffer[] buffer = new RenderBuffer[2];

            RenderTexture heightSrc = (src == 0) ? heightContext.sourceRenderTexture : heightContext.destinationRenderTexture;
            RenderTexture heightDst = (src == 0) ? heightContext.destinationRenderTexture : heightContext.sourceRenderTexture;

            buffer[0] = heightDst.colorBuffer;
            buffer[1] = soilWater[1 - src].colorBuffer;
            Graphics.SetRenderTarget(buffer, heightDst.depthBuffer);
            Graphics.Blit(heightSrc, m_Material, 0);

            renderMaterial.SetTexture("_HeightTex", heightDst);
            renderMaterial.SetTexture("_MainTex", soilWater[1 - src]);
            renderMaterial.SetFloat("_Opacity", m_Opacity);
            renderMaterial.mainTexture.filterMode = FilterMode.Bilinear;

            src = 1 - src;
        }

        public override void OnEnterToolMode()
        {
            CreateDisplay();
            UnityEditor.EditorApplication.update += Update;
            base.OnEnterToolMode();
        }

        public override void OnExitToolMode()
        {
            DestroyImmediate(renderSurface);
            UnityEditor.EditorApplication.update -= Update;
            base.OnExitToolMode();
        }

        public override void OnDisable()
        {
            UnityEditor.EditorApplication.update -= Update;
            base.OnDisable();
        }

        // Start is called before the first frame update
        override public void OnEnable()
        {
            UnityEditor.EditorApplication.update += Update;
            base.OnEnable();
        }

        private void ToggleRenderSurface(bool flag)
        {
            if (renderSurface == null)
                return;
  
            int childNum = renderSurface.transform.childCount;
            for (int i = childNum - 1; i >= 0; i--)
            {
                renderSurface.transform.GetChild(i).gameObject.SetActive(flag);
            }
        }

        void Update()
        {
            if (stepsRemaining <= 0)
            {
                ToggleRenderSurface(false);
                return;
            }

            ToggleRenderSurface(true);

            int stepsThisUpdate = (int)(m_SimSpeed*20.0f)+1; 

            while (stepsThisUpdate-- > 0 && stepsRemaining > 0)  
                Erode();

            if (stepsRemaining <= 0)
            {
                continuing = false;
                ToggleRenderSurface(false);
                TerrainPaintUtility.EndPaintHeightmap(heightContext, "Terrain Paint - Hydraulic Erosion");
                if (maskContext != null)
                {
                    TerrainPaintUtility.ReleaseContextResources(maskContext);
                }
            }
            else
            {
                heightContext.ScatterHeightmap("Terrain Paint - Hydraulic Erosion");
            }
        }
    }
}
