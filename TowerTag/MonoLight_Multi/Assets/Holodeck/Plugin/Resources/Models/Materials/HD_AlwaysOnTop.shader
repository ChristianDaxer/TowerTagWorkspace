// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


 Shader "HolodeckVR/AlwaysOnTop" {
     Properties {
    _MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
    Blend SrcAlpha One
    Cull Off Lighting Off ZTest Off ZWrite Off Fog { Color (0,0,0,0) }
    
    BindChannels {
        Bind "Color", color
        Bind "Vertex", vertex
        Bind "TexCoord", texcoord
    }
    
    SubShader {
        Pass {
            SetTexture [_MainTex] {
                combine texture
            }
        }
    }
}
 }
