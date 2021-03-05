// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "_OwnShader/RopeIntersectionProjector_Animated" 
{
	Properties 
	{
        _TintColor ("Tint Color", Color) = (1,1,1,1)
		_ColorStrength ("Color strength", Float) = 1.0
		_EmissionStrength("Emission strength", Float) = 1.0
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "black" {}

		// *** Animation & Colorramp ***
		[Header(Animation And Colorramp)]
		_NoiseTex("Noise", 2D) = "white" {}
		_ColorRamp("Color Ramp", 2D) = "white" {}

		_Frequency("Frequency", float) = 1
		_DisplacementStrength("Displace strength 1", float) = 1
		_DisplacementSpeed("Displacement Speed", float) = 1

		_RotationSpeed("Rotation Speed", float) = 1
		_OffsetAngle("OffsetAngle", float) = 1
	}

	Category 
	{

		//Tags { "Queue"="Transparent"  "IgnoreProjector"="True"  "RenderType"="Transparent" }
		Tags { "Queue"="Geometry+1"  "IgnoreProjector"="True"  "RenderType"="Transparent" }
		
		// alpha blending
		//Blend SrcAlpha OneMinusSrcAlpha
		
		// additive blending
		Blend SrcAlpha One
		
		Cull Off 
		ZWrite Off
		Fog { Mode Off}
		Offset -1, -1

		SubShader 
		{
			Pass 
			{
				Name "BASE"
				//Tags { "LightMode" = "Always" }
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct appdata_t {
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 texcoord: TEXCOORD0;
					fixed4 color : COLOR;
				};

				struct v2f {
					float4 vertex : POSITION;
					float4 uvFalloff : TEXCOORD0;
					float4 uvMainTex : TEXCOORD1;
					float2 texcoord : TEXCOORD2;
				};

				float4x4 unity_Projector;
				float4x4 unity_ProjectorClip;
				sampler2D _MainTex;
				float _ColorStrength;
				fixed4 _TintColor;
				float4 _MainTex_ST;

				float _EmissionStrength;

				// *** Animation & Colorramp ***
				sampler2D _ColorRamp;
				float4 _ColorRamp_ST;
				float _Frequency;
				float _DisplacementStrength;
				float _OffsetAngle;
				float _RotationSpeed;
				float _DisplacementSpeed;
				sampler2D _NoiseTex;

				float2 CalculateUVOffset(float2 uv, float lengthFactor)
				{
					// calculate vector in unit circle
					float2 vectorInUnitCircle = (uv - 0.5) * 2;
					// calculate angle
					float angle = atan2(vectorInUnitCircle.x, vectorInUnitCircle.y) * (_Frequency / UNITY_TWO_PI);

					// rotational animation
					float rotation = _SinTime.x * _RotationSpeed;
					// get displacement from tilable Noisetexture
					float disp = tex2D(_NoiseTex, float2(angle + rotation, _Time.x * _DisplacementSpeed));

					// offset UV
					return vectorInUnitCircle * disp /* length(vectorInUnitCircle)*/ * _DisplacementStrength * lengthFactor;
				}

				fixed4 CalculateColor(float2 uv)
				{
					// calculate vector in unit circle
					float2 vectorInUnitCircle = (uv - 0.5) * 2;
					return tex2D(_ColorRamp, float2(length(vectorInUnitCircle), 0) * _ColorRamp_ST.xy + _ColorRamp_ST.zw);
				}
				//******************



				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uvMainTex = mul (unity_Projector, v.vertex);
					o.uvFalloff = mul (unity_ProjectorClip, v.vertex);
					o.texcoord = TRANSFORM_TEX( o.uvMainTex.xyz,_MainTex);

					#if UNITY_UV_STARTS_AT_TOP
					float scale = -1.0;
					#else
					float scale = 1.0;
					#endif
					return o;
				}

				half4 frag( v2f i ) : COLOR
				{
					fixed4 mask = tex2D(_MainTex, saturate(i.texcoord)) * CalculateColor(saturate(i.texcoord));
					float2 offsetUVs = i.texcoord + CalculateUVOffset(i.texcoord, mask.a);

					fixed4 tex = tex2D(_MainTex, saturate(offsetUVs))  * CalculateColor(saturate(offsetUVs));
					fixed4 res = lerp(0, tex * _TintColor * _ColorStrength, 1 - abs(i.uvFalloff.z)) + float4(_EmissionStrength, _EmissionStrength, _EmissionStrength, 0);
					res.a = saturate(res.a);
					res *= res.a;
					return res;
				}
				ENDCG
			}
		}
	}
}

