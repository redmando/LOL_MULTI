// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/TerrainEngine/Details/WavingDoublePass" {
Properties {
    _WavingTint ("Fade Color", Color) = (.7,.6,.5, 0)
    _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    _WaveAndDistance ("Wave and distance", Vector) = (12, 3.6, 1, 1)
    _Cutoff ("Cutoff", float) = 0.5
}

SubShader {
    Tags {
        "Queue" = "Geometry+200"
        "IgnoreProjector"="True"
        "RenderType"="Grass"
        "DisableBatching"="True"
    }
    Cull Off
    LOD 200
    ColorMask RGB

CGPROGRAM
#pragma surface surf Lambert vertex:WavingGrassVert addshadow exclude_path:deferred
#include "TerrainEngine.cginc"
#include "../../FogOfWarMath.cginc"
#pragma multi_compile __ FoWColor FoWAnimatedFog

sampler2D _MainTex;
fixed _Cutoff;

struct Input {
    float2 uv_MainTex;
    fixed4 color : COLOR;
	float3 worldPos;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * IN.color;

	fixed4 FoW = FoWIntensity(IN.worldPos);

    o.Albedo = c.rgb * FoW.rgb;
    o.Alpha = c.a;
    clip (o.Alpha - _Cutoff);
    o.Alpha *= IN.color.a;
}
ENDCG
}

    SubShader {
        Tags {
            "Queue" = "Geometry+200"
            "IgnoreProjector"="True"
            "RenderType"="Grass"
        }
        Cull Off
        LOD 200
        ColorMask RGB

        Pass {
            Tags { "LightMode" = "Vertex" }
            Material {
                Diffuse (1,1,1,1)
                Ambient (1,1,1,1)
            }
            Lighting On
            ColorMaterial AmbientAndDiffuse
            AlphaTest Greater [_Cutoff]
            SetTexture [_MainTex] { combine texture * primary DOUBLE, texture }
        }
        Pass {
            Tags { "LightMode" = "VertexLMRGBM" }
            AlphaTest Greater [_Cutoff]
            BindChannels {
                Bind "Vertex", vertex
                Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
                Bind "texcoord", texcoord1 // main uses 1st uv
            }
            SetTexture [unity_Lightmap] {
                matrix [unity_LightmapMatrix]
                combine texture * texture alpha DOUBLE
            }
            SetTexture [_MainTex] { combine texture * previous QUAD, texture }
        }
    }

    Fallback Off
}
