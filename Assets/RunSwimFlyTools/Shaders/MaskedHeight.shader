    //Comments and suggestions to business@runswimfly.com
    
    Shader "RunSwimFlyTools/MaskedHeight" {

    Properties { _MainTex ("Texture", any) = "" {} }

    SubShader {

        ZTest Always Cull Off ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"

            sampler2D _MainTex;
            float4 _HeightTex_TexelSize;      // 1/width, 1/height, width, height
            
            int _MaskStencil;
            
            sampler2D _BrushTex;
            sampler2D _MaskTex;

            float4 _BrushParams;
            #define BRUSH_STRENGTH      (_BrushParams[0])
            
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
            Name "Masked Height"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment MaskedHeight
            
            float4 MaskedHeight(v2f i) : SV_Target
            {
                float2 brushUV = PaintContextUVToBrushUV(i.pcUV);
                float2 heightmapUV = PaintContextUVToHeightmapUV(i.pcUV);
                
                float modifier = tex2D(_MaskTex, heightmapUV);
                float intensity = (_MaskStencil==0)?1.0 : modifier*(_MaskStencil-1) + (2-_MaskStencil) * (1.0f-modifier);

                float height = UnpackHeightmap(tex2D(_MainTex, heightmapUV));
                
                float oob = all(saturate(brushUV) == brushUV) ? 1.0f : 0.0f;
                float brushStrength = oob * BRUSH_STRENGTH * UnpackHeightmap(tex2D(_BrushTex, brushUV)) * intensity;
                return PackHeightmap(height + brushStrength);
            }
            ENDCG
        }
    }
    Fallback Off
}
