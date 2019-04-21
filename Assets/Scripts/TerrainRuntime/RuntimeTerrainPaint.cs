using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;
using UnityEditor.Experimental.TerrainAPI;

public class RuntimeTerrainPaint : MonoBehaviour
{
    public Terrain terrain;
    public Texture brushTexture;
    public TerrainLayer terrainTexture;
    AnimationCurve widthProfile = AnimationCurve.Linear(0, 1, 1, 1);
    AnimationCurve heightProfile = AnimationCurve.Linear(0, 0, 1, 0);
    AnimationCurve strengthProfile = AnimationCurve.Linear(0, 1, 1, 1);
    AnimationCurve jitterProfile = AnimationCurve.Linear(0, 0, 1, 0);
    RaycastHit hit;
    Ray ray;
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftControl))
        {
            lmb = true;
            ctrl = false;
            wasPainting = isPainting;
            isPainting = true;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 3500f))
            {
                UpdateBrushLocations(terrain, hit);
                CloneTool(terrain, hit.point, MovementBehavior.FollowAlways, 0f, true, true, hit.textureCoord, brushTexture, 1f, 100f, 0f);
            }
        }
        else if(Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.LeftControl))
        {
            ctrl = true;
            lmb = true;
            isPainting = false;
            wasPainting = isPainting;
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 3500f))
            {
                UpdateBrushLocations(terrain, hit);
            }
        }
        else if(!Input.GetKey(KeyCode.Mouse0) && !Input.GetKey(KeyCode.LeftControl))
        {
            ctrl = false;
            lmb = false;
            wasPainting = isPainting;
            isPainting = lmb && !ctrl;
        }
    }
    public void RaiseLowerHeight(Terrain terrain, bool invert, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        if (invert)
        {
            brushStrength *= -1;
        }
        Vector4 brushParams = new Vector4(brushStrength * 0.01f, 0.0f, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.RaiseLowerHeight);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - RaiseLowerHeight");
    }
    public void PaintTexture(Terrain terrain, TerrainLayer texture, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintTexture(terrain, brushXform.GetBrushXYBounds(), texture);
        Vector4 brushParams = new Vector4(brushStrength, 1.0f, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.PaintTexture);
        TerrainPaintUtility.EndPaintTexture(paintContext, "RuntimeTerrainPaint - PaintTexture");
    }
    public void SmoothHeight(Terrain terrain, float smoothDirection, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(brushStrength, 0.0f, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        Vector4 smoothWeights = new Vector4(Mathf.Clamp01(1.0f - Mathf.Abs(smoothDirection)), Mathf.Clamp01(-smoothDirection), Mathf.Clamp01(smoothDirection), 0.0f);                                          // unused
        mat.SetVector("_SmoothWeights", smoothWeights);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SmoothHeights);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint  - SmoothHeight");
    }
    public void SetHeight(Terrain terrain, float height, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        float terrainHeight = Mathf.Clamp01((height - terrain.transform.position.y) / terrain.terrainData.size.y);
        Vector4 brushParams = new Vector4(brushStrength * 0.01f, 0.5f * terrainHeight, 0.0f, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.SetHeights);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint  - SetHeight");
    }
    public void StampTerrain(Terrain terrain, float stampHeight, float maxHeightAdd, bool subtract, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = TerrainPaintUtility.GetBuiltinPaintMaterial();
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());
        float height = stampHeight / terrain.terrainData.size.y;
        if (subtract)
        {
            height = -height;
        }
        Vector4 brushParams = new Vector4(brushStrength, 0.0f, height, maxHeightAdd);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, (int)TerrainPaintUtility.BuiltinPaintMaterialPasses.StampHeight);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - StampTerrain");
    }
    public void SlopeFlatten(Terrain terrain, float featureSize, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/SlopeFlatten"));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
        Vector4 brushParams = new Vector4(brushStrength, 0.0f, featureSize, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - SlopeFlatten");
    }
    public void TwistHeight(Terrain terrain, float twistAmount, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/TwistHeight"));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        float finalTwistAmount = twistAmount * -0.001f;
        paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
        Vector4 brushParams = new Vector4(brushStrength, 0.0f, twistAmount, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - TwistHeight");
    }
    Vector2 prevBrushPos = new Vector2(0.0f, 0.0f);
    Vector2 prevMousePos = new Vector2(0.0f, 0.0f);
    public void SmudgeHeight(Terrain terrain, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/SmudgeHeight"));
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            prevBrushPos = textureCoord;
        }
        if (Input.GetKey(KeyCode.Mouse0) && prevMousePos != new Vector2(Input.mousePosition.x, Input.mousePosition.y))
        {
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
            Vector2 smudgeDir = textureCoord - prevBrushPos;
            paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
            Vector4 brushParams = new Vector4(brushStrength, smudgeDir.x, smudgeDir.y, 0.0f);
            mat.SetTexture("_BrushTex", brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
            TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - SmudgeHeight");
        }
        prevMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }
    public void TerraceErosion(Terrain terrain, float featureSize, float interiorCornerWeight, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/TerraceErosion"));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds());
        Vector4 brushParams = new Vector4(brushStrength, featureSize, interiorCornerWeight, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - TerraceErosion");
    }
    public void SharpenHeight(Terrain terrain, float featureSize, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/SharpenHeight"));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, brushRotation);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
        Vector4 brushParams = new Vector4(brushStrength, 0.0f, featureSize, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - SharpenHeight");
    }
    public void RidgeErode(Terrain terrain, float erosionStrength, float mixStrength, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/RidgeErode"));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        Vector4 brushParams = new Vector4(brushStrength, erosionStrength, mixStrength, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - RidgeErode");
    }
    public void PinchHeight(Terrain terrain, float pinchAmount, bool bulge, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/PinchHeight"));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        float finalPinchAmount = pinchAmount * 0.005f;
        if (bulge)
        {
            finalPinchAmount *= -1.0f;
        }
        Vector4 brushParams = new Vector4(brushStrength, 0.0f, finalPinchAmount, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - PinchHeight");
    }
    public void ErodeHeight(Terrain terrain, float featureSize, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/ErodeHeight"));
        BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, 0.0f);
        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, brushXform.GetBrushXYBounds(), 1);
        paintContext.sourceRenderTexture.filterMode = FilterMode.Bilinear;
        Vector4 brushParams = new Vector4(brushStrength, 0.0f, 0.1f * featureSize, 0.0f);
        mat.SetTexture("_BrushTex", brushTexture);
        mat.SetVector("_BrushParams", brushParams);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
        Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
        TerrainPaintUtility.EndPaintHeightmap(paintContext, "RuntimeTerrainPaint - ErodeHeight");
    }
    Terrain startTerrain;
    Vector3 startPoint;
    private Vector2 transformToWorld(Terrain t, Vector2 uvs)
    {
        Vector3 tilePos = t.GetPosition();
        return new Vector2(tilePos.x, tilePos.z) + uvs * new Vector2(t.terrainData.size.x, t.terrainData.size.z);
    }

    private Vector2 transformToUVSpace(Terrain originTile, Vector2 worldPos)
    {
        Vector3 originTilePos = originTile.GetPosition();
        Vector2 uvPos = new Vector2((worldPos.x - originTilePos.x) / originTile.terrainData.size.x,
                                    (worldPos.y - originTilePos.z) / originTile.terrainData.size.z);
        return uvPos;
    }
    public void BridgeTool(Terrain terrain, float spacing, AnimationCurve widthProfile, AnimationCurve heightProfile, AnimationCurve strengthProfile, AnimationCurve jitterProfile, 
        Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/SetExactHeight"));
        Vector2 uv = textureCoord;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            float height = terrain.terrainData.GetInterpolatedHeight(uv.x, uv.y) / terrain.terrainData.size.y;
            startPoint = new Vector3(uv.x, uv.y, height);
            startTerrain = terrain;
        }
        float targetHeight = terrain.terrainData.GetInterpolatedHeight(uv.x, uv.y) / terrain.terrainData.size.y;
        Vector3 targetPos = new Vector3(uv.x, uv.y, targetHeight);
        if (terrain != startTerrain)
        {
            Vector2 targetWorld = transformToWorld(terrain, uv);
            Vector2 targetUVs = transformToUVSpace(startTerrain, targetWorld);
            targetPos.x = targetUVs.x;
            targetPos.y = targetUVs.y;
        }
        Vector3 stroke = targetPos - startPoint;
        float strokeLength = stroke.magnitude;
        int numSplats = (int)(strokeLength / (0.001f * spacing));
        Terrain currTerrain = startTerrain;
        Vector2 posOffset = new Vector2(0.0f, 0.0f);
        Vector2 currUV = new Vector2();
        Vector4 brushParams = new Vector4();
        Vector2 jitterVec = new Vector2(-stroke.z, stroke.x);
        jitterVec.Normalize();
        for (int i = 0; i < numSplats; i++)
        {
            float pct = (float)i / (float)numSplats;
            float widthScale = widthProfile.Evaluate(pct);
            float heightOffset = heightProfile.Evaluate(pct) / currTerrain.terrainData.size.y;
            float strengthScale = strengthProfile.Evaluate(pct);
            float jitterOffset = jitterProfile.Evaluate(pct) / Mathf.Max(currTerrain.terrainData.size.x, currTerrain.terrainData.size.z);
            Vector3 currPos = startPoint + pct * stroke;
            currPos.x += posOffset.x + jitterOffset * jitterVec.x;
            currPos.y += posOffset.y + jitterOffset * jitterVec.y;
            if (currPos.x >= 1.0f && (currTerrain.rightNeighbor != null))
            {
                currTerrain = currTerrain.rightNeighbor;
                currPos.x -= 1.0f;
                posOffset.x -= 1.0f;
            }
            if (currPos.x <= 0.0f && (currTerrain.leftNeighbor != null))
            {
                currTerrain = currTerrain.leftNeighbor;
                currPos.x += 1.0f;
                posOffset.x += 1.0f;
            }
            if (currPos.y >= 1.0f && (currTerrain.topNeighbor != null))
            {
                currTerrain = currTerrain.topNeighbor;
                currPos.y -= 1.0f;
                posOffset.y -= 1.0f;
            }
            if (currPos.y <= 0.0f && (currTerrain.bottomNeighbor != null))
            {
                currTerrain = currTerrain.bottomNeighbor;
                currPos.y += 1.0f;
                posOffset.y += 1.0f;
            }
            currUV.x = currPos.x;
            currUV.y = currPos.y;
            int finalBrushSize = (int)(widthScale * (float)brushSize);
            float finalHeight = (startPoint + pct * stroke).z + heightOffset;
            BrushTransform brushXform = TerrainPaintUtility.CalculateBrushTransform(currTerrain, currUV, finalBrushSize, 0.0f);
            PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(currTerrain, brushXform.GetBrushXYBounds());
            mat.SetTexture("_BrushTex", brushTexture);
            brushParams.x = brushStrength * strengthScale;
            brushParams.y = 0.5f * finalHeight;
            mat.SetVector("_BrushParams", brushParams);
            TerrainPaintUtility.SetupTerrainToolMaterialProperties(paintContext, brushXform, mat);
            Graphics.Blit(paintContext.sourceRenderTexture, paintContext.destinationRenderTexture, mat, 0);
            TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain Paint - Bridge");
        }
    }
    public enum MovementBehavior
    {
        Snap = 0,       // clone snaps back to set sample location on mouse up
        FollowOnPaint,  // clone location will move with the brush only when painting
        FollowAlways,   // clone location will move with the brush always
        Fixed,          // clone wont move at all and will sample same location always
    }
    private MovementBehavior movementBehavior;
    private BrushLocationData sampleLocation;
    private BrushLocationData snapbackLocation;
    private BrushLocationData prevBrushLocation;
    private bool lmb;
    private bool ctrl;
    private bool wasPainting;
    private bool isPainting;
    private bool hasDoneFirstPaint;
    struct BrushLocationData
    {
        public Terrain terrain;
        public Vector3 pos;

        public void Set(Terrain terrain, Vector3 pos)
        {
            this.terrain = terrain;
            this.pos = pos;
        }
    }
    private void UpdateBrushLocations(Terrain terrain, RaycastHit raycastHit)
    {
        if (!isPainting)
        {
            if (lmb && ctrl)
            {
                hasDoneFirstPaint = false;
                sampleLocation.Set(terrain, raycastHit.point);
                snapbackLocation.Set(terrain, raycastHit.point);
            }
            if (movementBehavior == MovementBehavior.Snap)
            {
                sampleLocation.Set(snapbackLocation.terrain, snapbackLocation.pos);
            }
        }
        else if (!wasPainting && isPainting) // first frame of user painting
        {
            hasDoneFirstPaint = true;
            prevBrushLocation.Set(terrain, raycastHit.point);
        }
        bool updateClone = (isPainting && movementBehavior != MovementBehavior.Fixed) ||
                            (isPainting && movementBehavior == MovementBehavior.FollowOnPaint) ||
                            (hasDoneFirstPaint && movementBehavior == MovementBehavior.FollowAlways);
        if (updateClone)
        {
            HandleBrushCrossingSeams(ref sampleLocation, raycastHit.point, prevBrushLocation.pos);
        }
        prevBrushLocation.Set(terrain, raycastHit.point);
    }
    private Vector2 TerrainUVFromBrushLocation(Terrain terrain, Vector3 posWS)
    {
        Vector3 posTS = posWS - terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        return new Vector2(posTS.x / size.x, posTS.z / size.z);
    }
    private void HandleBrushCrossingSeams(ref BrushLocationData brushLocation, Vector3 currBrushPos, Vector3 prevBrushPos)
    {
        if (brushLocation.terrain == null)
            return;
        Vector3 deltaPos = currBrushPos - prevBrushPos;
        brushLocation.Set(brushLocation.terrain, brushLocation.pos + deltaPos);
        Vector2 currUV = TerrainUVFromBrushLocation(brushLocation.terrain, brushLocation.pos);
        if (currUV.x >= 1.0f && brushLocation.terrain.rightNeighbor != null)
            brushLocation.terrain = brushLocation.terrain.rightNeighbor;
        else if (currUV.x < 0.0f && brushLocation.terrain.leftNeighbor != null)
            brushLocation.terrain = brushLocation.terrain.leftNeighbor;

        if (currUV.y >= 1.0f && brushLocation.terrain.topNeighbor != null)
            brushLocation.terrain = brushLocation.terrain.topNeighbor;
        else if (currUV.y < 0.0f && brushLocation.terrain.bottomNeighbor != null)
            brushLocation.terrain = brushLocation.terrain.bottomNeighbor;
    }
    private void ApplyHeightmap(PaintContext sampleContext, PaintContext targetContext, BrushTransform targetXform, Terrain targetTerrain, Texture brushTexture, float brushStrength, float meshStampOffset)
    {
        Material paintMat = new Material(Shader.Find("TerrainToolSamples/CloneBrush"));
        Vector4 brushParams = new Vector4(brushStrength, meshStampOffset * 0.5f, targetTerrain.terrainData.size.y, 0f);
        paintMat.SetTexture("_BrushTex", brushTexture);
        paintMat.SetVector("_BrushParams", brushParams);
        paintMat.SetTexture("_CloneTex", sampleContext.sourceRenderTexture);
        TerrainPaintUtility.SetupTerrainToolMaterialProperties(targetContext, targetXform, paintMat);
        Graphics.Blit(targetContext.sourceRenderTexture, targetContext.destinationRenderTexture, paintMat, 0);
    }
    private void PaintHeightmap(Terrain sampleTerrain, Terrain targetTerrain, BrushTransform sampleXform, BrushTransform targetXform, float brushStrength, float meshStampOffset)
    {
        PaintContext sampleContext = TerrainPaintUtility.BeginPaintHeightmap(sampleTerrain, sampleXform.GetBrushXYBounds());
        PaintContext targetContext = TerrainPaintUtility.BeginPaintHeightmap(targetTerrain, targetXform.GetBrushXYBounds());
        ApplyHeightmap(sampleContext, targetContext, targetXform, targetTerrain, brushTexture, brushStrength, meshStampOffset);
        TerrainPaintUtility.EndPaintHeightmap(targetContext, "Terrain Paint - Clone Brush (Heightmap)");
        TerrainPaintUtility.ReleaseContextResources(sampleContext);
    }
    private void PaintAlphamap(Terrain sampleTerrain, Terrain targetTerrain, BrushTransform sampleXform, BrushTransform targetXform, Material mat)
    {
        Rect sampleRect = sampleXform.GetBrushXYBounds();
        Rect targetRect = targetXform.GetBrushXYBounds();
        int numSampleTerrainLayers = sampleTerrain.terrainData.terrainLayers.Length;
        for (int i = 0; i < numSampleTerrainLayers; ++i)
        {
            TerrainLayer layer = sampleTerrain.terrainData.terrainLayers[i];
            if (layer == null)
            {
                continue;
            }
            PaintContext sampleContext = TerrainPaintUtility.BeginPaintTexture(sampleTerrain, sampleRect, layer);
            int layerIndex = TerrainPaintUtility.FindTerrainLayerIndex(sampleTerrain, layer);
            Texture2D layerTexture = TerrainPaintUtility.GetTerrainAlphaMapChecked(sampleTerrain, layerIndex >> 2);
            PaintContext targetContext = PaintContext.CreateFromBounds(targetTerrain, targetRect, layerTexture.width, layerTexture.height);
            targetContext.CreateRenderTargets(RenderTextureFormat.R8);
            targetContext.GatherAlphamap(layer, true);
            sampleContext.sourceRenderTexture.filterMode = FilterMode.Point;
            mat.SetTexture("_CloneTex", sampleContext.sourceRenderTexture);
            Graphics.Blit(targetContext.sourceRenderTexture, targetContext.destinationRenderTexture, mat, 0);
            targetContext.ScatterAlphamap("Terrain Paint - Clone Brush (Texture)");
            targetContext.Cleanup();
        }
    }
    public void CloneTool(Terrain terrain, Vector3 location, MovementBehavior movementBehavior, float meshStampOffset, bool paintAlphamap, bool paintHeightmap, Vector2 textureCoord, Texture brushTexture, float brushStrength, float brushSize, float brushRotation)
    {
        Material mat = new Material(Shader.Find("TerrainToolSamples/CloneBrush"));
        if (isPainting && sampleLocation.terrain != null)
        {
            Vector2 sampleUV = TerrainUVFromBrushLocation(sampleLocation.terrain, sampleLocation.pos);
            BrushTransform sampleBrushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, sampleUV, brushSize, 1);
            BrushTransform targetBrushXform = TerrainPaintUtility.CalculateBrushTransform(terrain, textureCoord, brushSize, 1);
            Vector4 brushParams = new Vector4(brushStrength, meshStampOffset * 0.5f, terrain.terrainData.size.y, 0f);
            mat.SetTexture("_BrushTex", brushTexture);
            mat.SetVector("_BrushParams", brushParams);
            if (paintAlphamap)
            {
                PaintAlphamap(terrain, terrain, sampleBrushXform, targetBrushXform, mat);
            }
            if (paintHeightmap)
            {
                PaintHeightmap(terrain, terrain, sampleBrushXform, targetBrushXform, brushStrength, meshStampOffset);
            }
        }
    }
}

