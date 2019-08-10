//Comments and suggestions to business@runswimfly.com

Shader "RunSwimFlyTools/WaterDisplay"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.2
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        Cull Off ZWrite Off

        Blend SrcAlpha OneMinusSrcAlpha
        Offset -1, -1 
                
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _HeightTex;
        float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height
        uniform float3 _Size;
        uniform float _Resolution;
        uniform float _Opacity;
        


        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float Border(float2 coord)
        {
            const float borderSub = 0.95;
            const float borderMult = 100.0;

            float2 displacement = abs(coord - float2(0.5,0.5)) * 2.0;
            return clamp((max(displacement.x,displacement.y)-borderSub)*borderMult,0.0,1.0);
        }
        
        float Height(float2 coord)
        {

            return UnpackHeightmap(tex2Dlod(_HeightTex, float4(coord,0.0,0.0)))*2.0*_Size.y + 
            tex2Dlod(_MainTex, float4(coord,0.0,0.0)).r + Border(coord) * 2.0;   
        }
      
        void vert (inout appdata_full v)//, out Input o) 
        {
           float height = Height(v.texcoord.xy);

           v.vertex.y += 1.0 + Height(v.texcoord.xy);
           
           float heightX = Height(v.texcoord.xy + float2(_MainTex_TexelSize.x,0.0));
           float heightZ = Height(v.texcoord.xy + float2(0.0,_MainTex_TexelSize.y));
           
           float slopeX = (heightX-height)*_Resolution/_Size.x;
           float slopeZ = (heightZ-height)*_Resolution/_Size.z;
           
           v.normal = normalize(float3(slopeX,1.0,slopeZ));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 c = tex2D(_MainTex, IN.uv_MainTex);
            fixed3 col = lerp(fixed3(0.2,0.5,0.8),fixed3(0.1,0.3,1.0),min(c.r*0.175,1.0)); //deep water is a slightly deeper blue
            
            //create a dull white border around water to simulate (kind of) the effect of foam but mostly just 
            //make the border between water and terrain look a little better
            float foam = max(c.g,1.0-5.0*c.r);
            float interp = min(foam,1.0);
            col = lerp(col,fixed3(0.8,0.8,0.8),interp);
            
           // float interp = max(foam,sediment*0.8);
            o.Metallic = lerp(_Metallic,0.0,interp);;
            o.Smoothness = lerp(_Glossiness,0.1,interp);
            
            
            //draw a white outline around simulation area
            float displaceFactor = Border(IN.uv_MainTex);
            o.Albedo = col*(1.0-displaceFactor) + fixed3(1.0,1.0,1.0)*displaceFactor;
            
            o.Alpha = clamp(max(displaceFactor,sqrt(c.r*0.75)),c.r>0.05?0.15:0.0,_Opacity);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
