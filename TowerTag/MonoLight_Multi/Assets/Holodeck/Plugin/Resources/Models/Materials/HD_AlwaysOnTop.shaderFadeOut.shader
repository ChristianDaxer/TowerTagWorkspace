// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


 Shader "HolodeckVR/AlwaysOnTopFadeOut" {
         Properties {
         _MainTex ("Font Texture", 2D) = "white" {}
         _Color ("Text Color", Color) = (1,1,1,1)
         
		 _MinVisDistance("MinDistance",Float) = 0
         _MaxVisDistance("MaxDistance",Float) = 20
 
         _ColorMask ("Color Mask", Float) = 15
     }
 
     SubShader {
 
         Tags 
         {
             "Queue"="Transparent"
             "IgnoreProjector"="True"
             "RenderType"="Transparent"
             "PreviewType"="Plane"
         }
         

         
         Lighting Off 
         Cull Off 
         ZTest Off
         ZWrite Off 
         Blend SrcAlpha OneMinusSrcAlpha
         ColorMask [_ColorMask]

		         Pass
        {
           ZWrite Off
           ColorMask 0
        }
        Blend OneMinusDstColor OneMinusSrcAlpha //invert blending, so long as FG color is 1,1,1,1
        BlendOp Add
 
         Pass 
         {
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile_fog
 
             #include "UnityCG.cginc"
 

 
             struct v2f {
                 float4 vertex : SV_POSITION;
                 fixed4 color : COLOR;
                 float2 texcoord : TEXCOORD0;
             };
 
             sampler2D _MainTex;
             uniform float4 _MainTex_ST;
             uniform fixed4 _Color;
			 half        _MinVisDistance;
			 half        _MaxVisDistance;
             
             v2f vert (appdata_full v)
             {
                 v2f o;
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				 half3 viewDirW = _WorldSpaceCameraPos - mul((half4x4)unity_ObjectToWorld, v.vertex);
				 half viewDist = length(viewDirW);
				 half falloff = saturate((viewDist - _MinVisDistance) / (_MaxVisDistance - _MinVisDistance));
				
                 o.color = (v.color * _Color) - falloff;
 #ifdef UNITY_HALF_TEXEL_OFFSET
                 o.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
 #endif
                 return o;
             }

			 
 
             fixed4 frag (v2f i) : SV_Target
             {
                 fixed4 col = i.color;
                 col.a *= tex2D(_MainTex, i.texcoord).a;
                 clip (col.a - 0.01);
                 return col;
             }
             ENDCG 
         }
     }
 }
