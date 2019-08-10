//Comments and suggestions to business@runswimfly.com

    Shader "RunSwimFlyTools/MaskedTexture" {

    Properties { _MainTex ("Texture", any) = "" {} }

    SubShader {

        ZTest Always Cull Off ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height

            int _MaskStencil;
            
            sampler2D _BrushTex;
            sampler2D _HeightTex;
            sampler2D _NormalTex;
            sampler2D _MaskTex;
            
            float4 _BrushParams;
            #define BRUSH_STRENGTH      (_BrushParams[0])
            #define HEIGHT_MAX_POINT         (_BrushParams[1])
            #define HEIGHT_MAX_WIDTH         (_BrushParams[2])
            #define HEIGHT_FALLOFF_WIDTH     (_BrushParams[3])
            
            float4 _SlopeParams;
            #define SLOPE_MAX_POINT         (_SlopeParams[0])
            #define SLOPE_MAX_WIDTH         (_SlopeParams[1])
            #define SLOPE_FALLOFF_WIDTH     (_SlopeParams[2])
            #define MAX_VALUE               (_SlopeParams[3])
            
            float _FeatureSize;
            float _AspectRatio;
            
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

        ENDCG


        Pass    
        {
            Name "Masked Texture"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment MaskedTexture
            
            float Intensity(float input, float maxPoint, float maxWidth, float falloffWidth)
            {
                float distFromMax = abs(input - maxPoint);
                float distFromPlateau = distFromMax - maxWidth;
                return distFromPlateau<0.0?1.0:max(0.0,(falloffWidth-distFromPlateau)/falloffWidth);
            }
            
            float RelativeHeight(float2 loc, float height)
            {
                return height - UnpackHeightmap(tex2D(_HeightTex, loc))*2.0;
            }
            
            float4 MaskedTexture(v2f i) : SV_Target
            {
                const float PI = 3.14159;

                float2 brushUV = PaintContextUVToBrushUV(i.pcUV);
                float2 heightmapUV = PaintContextUVToHeightmapUV(i.pcUV);
                
                float3 normal = UnpackNormal(tex2D(_NormalTex, heightmapUV));   
                float slope = acos(normal.y)*2.0/PI;
                
                float intensity = MAX_VALUE*Intensity(slope,SLOPE_MAX_POINT,SLOPE_MAX_WIDTH,SLOPE_FALLOFF_WIDTH);
                
                float height = UnpackHeightmap(tex2D(_HeightTex, heightmapUV)) * 2.0;
                
                float xoffset = _MainTex_TexelSize.x *_FeatureSize;
                float yoffset = _MainTex_TexelSize.y * _AspectRatio * _FeatureSize;
  
                if (_FeatureSize>0.0)
                {
                    float relativeHeight = RelativeHeight(heightmapUV + float2(-xoffset, 0.0F), height);
                    relativeHeight += RelativeHeight(heightmapUV + float2( xoffset, 0.0F), height);
                    relativeHeight += RelativeHeight(heightmapUV + float2(0.0f, -yoffset), height);
                    relativeHeight += RelativeHeight(heightmapUV + float2( 0.0f, yoffset), height);
                    relativeHeight += RelativeHeight(heightmapUV + float2( -xoffset, -yoffset)*0.707, height);;
                    relativeHeight += RelativeHeight(heightmapUV + float2( -xoffset, yoffset)*0.707, height);
                    relativeHeight += RelativeHeight(heightmapUV + float2( xoffset, -yoffset)*0.707, height);
                    relativeHeight += RelativeHeight(heightmapUV + float2( xoffset, yoffset)*0.707, height);
                    
                  //  relativeHeight = RelativeHeight(heightmapUV + float2( xyoffset, xyoffset)*10.0, height);
                    height = relativeHeight/_FeatureSize;
                }
                
                intensity *= Intensity(height,HEIGHT_MAX_POINT,HEIGHT_MAX_WIDTH,HEIGHT_FALLOFF_WIDTH);
                
                float currentAlpha = tex2D(_MainTex, heightmapUV).r;

                float modifier = tex2D(_MaskTex, heightmapUV);
                intensity *= (_MaskStencil==0)?1.0 : (modifier+currentAlpha)*(_MaskStencil-1) + (2-_MaskStencil) * (1.0f-modifier);

                float oob = all(saturate(brushUV) == brushUV) ? 1.0f : 0.0f;
                float brushStrength = ((BRUSH_STRENGTH <= 1.0)?(oob * BRUSH_STRENGTH * UnpackHeightmap(tex2D(_BrushTex, brushUV))):1.0) * intensity;
               
                 return min(currentAlpha + brushStrength,max(intensity,currentAlpha));
                
            }
            ENDCG
        }
    }
    Fallback Off
}
