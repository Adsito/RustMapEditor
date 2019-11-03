    Shader "TerrainTools/SplatmapSmooth" {

    Properties { _MainTex ("Texture", any) = "" {} }

    SubShader {

        ZTest Always Cull Off ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height

            sampler2D _BrushTex;

            float4 _BrushParams;
            #define BRUSH_STRENGTH      (_BrushParams[0])

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

        ENDCG


        Pass
        {
            Name "Blur Splatmap"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment SmoothAlpha

            float4 SmoothAlpha(v2f i) : SV_Target
            {
                float brushStrength = BRUSH_STRENGTH * UnpackHeightmap(tex2D(_BrushTex, i.uv));

				float4 oldAlpha = tex2D(_MainTex, i.uv);

				const float2 coords [4] = { {-1,0}, { 1,0}, {0, -1}, { 0, 1} };

                float hc = tex2D(_MainTex, i.uv);
				float hl = tex2D(_MainTex, i.uv + coords[0] * _MainTex_TexelSize.xy);
				float hr = tex2D(_MainTex, i.uv + coords[1] * _MainTex_TexelSize.xy);
				float ht = tex2D(_MainTex, i.uv + coords[2] * _MainTex_TexelSize.xy);
				float hb = tex2D(_MainTex, i.uv + coords[3] * _MainTex_TexelSize.xy);

				float targetAlpha = 0.25f * ( hl + hr + ht + hb );

				return lerp(oldAlpha, targetAlpha, brushStrength);
            }
            ENDCG
        }
    }
    Fallback Off
}
