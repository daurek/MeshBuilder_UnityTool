﻿Shader "Custom/Vertex Colored Diffuse" {
     
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
     
    CGPROGRAM
    #pragma surface surf Standard fullforwardshadows vertex:vert 
	#pragma target 3.0
     
    struct Input 
	{
        float4 vertColor;
    };
     
    void vert (inout appdata_full v, out Input o) 
	{
        UNITY_INITIALIZE_OUTPUT(Input, o);
        o.vertColor = v.color;
    }
     
    void surf (Input IN, inout SurfaceOutputStandard o)
	{
        o.Albedo = IN.vertColor.rgb;
    }
    ENDCG
    }
     
    Fallback "Diffuse"
}
