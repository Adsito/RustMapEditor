// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Nature/Terrain/Diffuse" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB)", 2D) = "white" {}
    [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
}
SubShader {
    Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
    LOD 200
	Offset -0.01,-0.01

CGPROGRAM
#pragma surface surf Lambert vertex:SplatmapVert addshadow fullforwardshadows alpha:fade
#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
#pragma multi_compile_local __ _ALPHATEST_ON

#define TERRAIN_BASE_PASS
#include "TerrainSplatmapCommon.cginc"

sampler2D _MainTex;
fixed4 _Color;

void surf (Input IN, inout SurfaceOutput o) {
    #ifdef _ALPHATEST_ON
        ClipHoles(IN.tc.xy);
    #endif
    fixed4 c = tex2D(_MainTex, IN.tc.xy) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG

UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
}

Fallback "Legacy Shaders/VertexLit"
}
