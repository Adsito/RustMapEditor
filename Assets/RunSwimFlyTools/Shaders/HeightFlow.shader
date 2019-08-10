//Comments and suggestions to business@runswimfly.com

    Shader "RunSwimFlyTools/HeightFlow" {
     
    Properties { _MainTex ("Texture", any) = "" {} }

    SubShader {

        ZTest Always Cull Off ZWrite Off 

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height
            
            int _MaskStencil;
            sampler2D _MaskTex;
            int _TerrainAdd;

            sampler2D _BrushTex;
            sampler2D _HeightFlow;

            float4 _BrushParams;
            #define BRUSH_STRENGTH      (_BrushParams[0])
            #define HEAT_RETAIN        (_BrushParams[1])
            #define RAIN_AMOUNT         (_BrushParams[2])
            
            float _Viscosity;

            float2 _WaterShift;
            float2 _WaterVel;

            float _HeightScale;
            float _Extent;
            float _DepositMult;
            
            struct appdata_t {
                float4 vertex : POSITION;
                float2 pcUV : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 pcUV : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pcUV = v.pcUV;
                return o;
            }
            
            struct fragOutput {
                float4 height : SV_Target0;
                float4 waterSoil : SV_Target1;
            };   
        ENDCG


        Pass    // 11 Erode
        {
            Name "Height Flow"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment HeightFlow
            
            void Transfer(float2 loc, float height, float4 water, inout float4 waterNew, float strength, float2 velMult)
            {
                float heightNeighbour = UnpackHeightmap(tex2D(_MainTex, loc));
                float4 waterNeighbour = tex2D(_HeightFlow, loc);
                
                //This hack reduces unsightly instability at the border between water and land.  
                float heightClamped = clamp(min(water.r,waterNeighbour.r)*0.8,0.0,1.0);
                
                float slip = (waterNeighbour.r - water.r) + (heightNeighbour - height)*_HeightScale*2.0; 
                waterNew.ba += velMult * slip  * strength * heightClamped * (1.0-_Viscosity*0.98) * 0.15;
                
                float flow = dot(waterNeighbour.ba,velMult);
                float waterDiff = (slip + flow)*0.125;
                waterDiff = min(waterNeighbour.r,max(-water.r,waterDiff));
                waterNew.r += (waterDiff * strength)*(1.0-_Viscosity*0.75); 
            }
            
            fragOutput HeightFlow(v2f i)
            {
                fragOutput OUT;
                
                float2 brushUV = PaintContextUVToBrushUV(((i.pcUV + _WaterShift)*(_Extent)) - float2(0.5,0.5)*(_Extent-1.0));
                float2 heightmapUV = PaintContextUVToHeightmapUV(i.pcUV);

                // out of bounds multiplier
                float oob = all(saturate(brushUV) == brushUV) ? 1.0f : 0.0f;
                
                float modifier = tex2D(_MaskTex, heightmapUV);
                float intensity = (_MaskStencil==0)?1.0 : modifier*(_MaskStencil-1) + (2-_MaskStencil) * (1.0f-modifier);

                float height = UnpackHeightmap(tex2D(_MainTex, heightmapUV));
                float4 water = tex2D(_HeightFlow, heightmapUV);
                
                float xoffset = _MainTex_TexelSize.x;
                float yoffset = _MainTex_TexelSize.y;
                float xyoffset = sqrt(pow(xoffset,2.0) + pow(yoffset,2.0)); 
                
                float xStrength = 1.0/xoffset;
                float factor = pow(xoffset/yoffset,0.75); //fudge factor to create roughly symetrical waves and erosion when simulation cells are not square 
                float yStrength = factor/(yoffset); 
                float xystrength = sqrt(factor)/xyoffset;
 
                float total = (xStrength + yStrength) * 2.0 + xystrength * 4.0;
                xystrength /= total;
                xStrength/=total;
                yStrength/=total;
                
                float4 waterNew = float4(0.0,0.0,0.0,0.0);
                Transfer(heightmapUV + float2(-xoffset, 0.0F), height, water, waterNew, xStrength, float2(-1.0,0.0));
                Transfer(heightmapUV + float2( xoffset, 0.0F), height, water, waterNew, xStrength, float2(1.0,0.0));
                Transfer(heightmapUV + float2(0.0f, -yoffset), height, water, waterNew, yStrength, float2(0.0,-1.0));
                Transfer(heightmapUV + float2( 0.0f, yoffset), height, water, waterNew, yStrength, float2(0.0,1.0));
                Transfer(heightmapUV + float2( -xoffset, -yoffset), height, water, waterNew, xystrength, float2(-0.707,-0.707));
                Transfer(heightmapUV + float2( -xoffset, yoffset), height, water, waterNew, xystrength, float2(-0.707,0.707));
                Transfer(heightmapUV + float2( xoffset, -yoffset), height, water, waterNew, xystrength, float2(0.707,-0.707));
                Transfer(heightmapUV + float2( xoffset, yoffset), height, water, waterNew, xystrength, float2(0.707,0.707));
                 
                float2 displacement = abs(heightmapUV - float2(0.5,0.5)) * 2.0;
                float displaceFactor = max(0.0,max(displacement.x,displacement.y)-0.8)*5.0;
 
                float brushShape = oob * UnpackHeightmap(tex2D(_BrushTex, brushUV));

                waterNew += water; 
                waterNew.g = waterNew.r * (1.0-HEAT_RETAIN) + water.g;
                float sub = abs(waterNew.g) > 0.1?waterNew.g:0.0;
                waterNew.g -= sub;
                
                waterNew.r -= sub;
                
                
                waterNew.ba *= (0.991 + clamp(waterNew.r*0.125,0.0,1.0)*0.007);
                //Damp shallow water more.
                
                float len = length (waterNew.ba);
                float2 normal = (len == 0.0) ? float2 (0.0, 0.0) : waterNew.ba / len;
                waterNew.ba = normal*min(max(0.0,waterNew.r*40.0),len);
  
                float erodedHeight = height;
                
 
                OUT.height = PackHeightmap(erodedHeight+(sub)/(_HeightScale*2.0));
                
                //Limit water level to zero as it approaches edge of simulation area
                waterNew.r*=1.0-pow(displaceFactor,10.0);
                waterNew.r = max(waterNew.r,0.0);
              //  waterNew.g = 1.0;
                OUT.waterSoil = waterNew + float4(_TerrainAdd==1?BRUSH_STRENGTH:0.0,_TerrainAdd==0?-BRUSH_STRENGTH*intensity:0.0,_WaterVel.x,_WaterVel.y) * brushShape * RAIN_AMOUNT;
                return OUT;
            }
            ENDCG
        }
    }
    Fallback Off
}
