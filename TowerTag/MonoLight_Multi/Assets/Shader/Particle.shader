Shader "Unlit/Particle"
{
Properties {
    _TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Particle Texture", 2D) = "white" {}
    _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		_TestLight ("TestLight", Vector) = (0,0,0,10)
		[HDR] _TestLightColor("TestLight Color", Color) = (0.5,0.5,0.5,0.5)
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
    Blend SrcAlpha OneMinusSrcAlpha
    ColorMask RGB
    Cull Off Lighting Off ZWrite Off

    SubShader {
        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _TintColor;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD2;
                #endif

				float3 worldPos : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 _MainTex_ST;

			float4 _TestLight;
			float3 _TestLightColor;
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                #ifdef SOFTPARTICLES_ON
					o.projPos = ComputeScreenPos (o.vertex);
					COMPUTE_EYEDEPTH(o.projPos.z);
                #endif
                o.color = v.color * _TintColor;
                o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

				// view depedent transparancy
				float3 position_ws = mul(unity_ObjectToWorld, v.vertex);
				float3 viewDirection = normalize(_WorldSpaceCameraPos - position_ws);
				float3 normal_ws = UnityObjectToWorldNormal(v.normal);

				float viewDependentFade = abs(dot(normal_ws, viewDirection));

				o.color.a *= viewDependentFade;

				o.worldPos = position_ws;
				// simple vertex lights
				

				float fakeLightAtten = 1.0 - saturate(distance(position_ws, _TestLight.xyz) / _TestLight.w);
				o.color.rgb += o.color.rgb * _TestLightColor * fakeLightAtten;
				//
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            fixed4 frag (v2f i) : SV_Target
            {
				//float fakeLightAtten = 1.0 - saturate(distance(i.worldPos, _TestLight.xyz) / _TestLight.w);
				//i.color.rgb += i.color.rgb * _TestLightColor * fakeLightAtten;

                #ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ-partZ));
                i.color.a *= fade;
                #endif

                fixed4 col = 2.0f * i.color * tex2D(_MainTex, i.texcoord);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
}
