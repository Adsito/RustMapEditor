//Comments and suggestions to business@runswimfly.com
    
    Shader "RunSwimFlyTools/MaskedSharpen" {

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
            sampler2D _MaskTex;
 
            float4 _BrushParams;
            #define BRUSH_STRENGTH      (_BrushParams[0])
            #define BRUSH_FEATURESIZE   (_BrushParams[2])

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
            Name "Sharpen Height"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment MaskedSharpen

            float4 MaskedSharpen(v2f i) : SV_Target
            {
                float2 brushUV = PaintContextUVToBrushUV(i.pcUV);
                float2 heightmapUV = PaintContextUVToHeightmapUV(i.pcUV);
                
                float modifier = tex2D(_MaskTex, heightmapUV);
                float intensity = (_MaskStencil==0)?1.0 : modifier*(_MaskStencil-1) + (2-_MaskStencil) * (1.0f-modifier);

                // out of bounds multiplier
                float oob = all(saturate(brushUV) == brushUV) ? 1.0f : 0.0f;

                float height = UnpackHeightmap(tex2D(_MainTex, heightmapUV));
                float brushStrength = oob * BRUSH_STRENGTH * UnpackHeightmap(tex2D(_BrushTex, brushUV)) * intensity;

                float avg = 0.0F;
                float xoffset = _MainTex_TexelSize.x * BRUSH_FEATURESIZE;
                float yoffset = _MainTex_TexelSize.y * BRUSH_FEATURESIZE;

                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV));
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2( xoffset,  0      )));
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2(-xoffset,  0      )));
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2( xoffset,  yoffset))) * 0.75F;
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2(-xoffset,  yoffset))) * 0.75F;
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2( xoffset, -yoffset))) * 0.75F;
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2(-xoffset, -yoffset))) * 0.75F;
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2( 0,        yoffset)));
                avg += UnpackHeightmap(tex2D(_MainTex, heightmapUV + float2( 0,       -yoffset)));
                avg /= 8.0F;

                float contrast = 1.5F;
                float h = ((height - avg)) * contrast + avg;
                return PackHeightmap(lerp(height, h, brushStrength));
            }
            ENDCG
        }
    }
    Fallback Off
}
