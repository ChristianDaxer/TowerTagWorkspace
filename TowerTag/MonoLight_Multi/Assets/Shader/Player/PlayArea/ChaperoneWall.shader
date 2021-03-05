// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "_OwnShader/Unlit/ChaperoneWall"
{
	Properties
	{
		_maxClipDist("Max. Clip Size", float) = 0.15
		_ClipDistScale("Clip Size Multi", float) = 1
		_DeltaHeight("Alpha Height", float) = 1
		_Color("_TintColor", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}

		_Frequency("Frequency", float) = 1
		_Distortion("Distortion", float) = 0
		_Speed("Speed", float) = 0
		_RadialContrast("contrast of radial visuals", float) = 0.125
		_ColorMultiplier("Color Multiplier", float ) = 1.5

		// Alpha Ramp
		_AlphaRampStart("Alpha Ramp Start (rel. to current AlphaHeight)", float) = 0
		_AlphaRampEnd("Alpha Ramp End (rel. to current AlphaHeight)", float) = 1

		_AnimParameter("Animation Parameter", Vector) = (0,0,0,0)

		[HideInInspector]
		_ControllerWorldPosition("ControllerWorldPosition", Vector) = (0,0,0,0)
		[HideInInspector]
		_HMDWorldPosition("HMDWorldPosition", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD10;
				float3 worldPos : COLOR; 
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _DeltaHeight;
			fixed4 _Color;
			float4 _ControllerWorldPosition;
			float4 _HMDWorldPosition;
			float _maxClipDist;
			float _ClipDistScale;
			float4 _AnimParameter;
			float _Frequency;
			float _Distortion;
			float _Speed;
			float _AlphaRampStart, _AlphaRampEnd;
			float _RadialContrast;
			float _ColorMultiplier;
			float _StencilMinAlpha;

			const float TWOPi = 2 * UNITY_PI;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			float map(float fromMin, float fromMax, float toMin, float toMax, float value)
			{
				return lerp(toMin, toMax, max((value - fromMin),0) / max((fromMax- fromMin), 0));
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float sinXAbs = _SinTime.x * 0.5 + 0.5;

				// min distance to clipping objects
				float dist = min(distance(i.worldPos, _ControllerWorldPosition), distance(i.worldPos, _HMDWorldPosition));

				// tex Animation
				float2 uvScale = float2(1,(1 + (i.uv2.y) * 1.5 * (sinXAbs)));
				float2 uvOffset = float2((_SinTime.x * sin(i.uv.y * TWOPi * _AnimParameter.x * (sinXAbs)) * _AnimParameter.z) , _Time.x * _AnimParameter.y);
				float2 uvOffset2 = (uvOffset * 0.2) + float2(_SinTime.x * 0.1, _SinTime.x * _AnimParameter.w);

				// wall
				fixed4 texColor = tex2D(_MainTex, (i.uv * uvScale + uvOffset)) + tex2D(_MainTex, (i.uv + uvOffset2));
				float fadeValue = map(_AlphaRampStart * _DeltaHeight, _AlphaRampEnd * _DeltaHeight, 1, 0, i.uv2.y);
				float wallAlpha = texColor.r * max(fadeValue, 0);

				// radial
				float distortionValue = tex2D(_MainTex, (i.uv2 * uvScale * 3  + uvOffset));
				float radialFactor =  (sin((_Time.z * _Speed) + (dist * (_Frequency) + ( distortionValue * _Distortion)))* _RadialContrast + max(_RadialContrast,(1 -  (_RadialContrast))));
				float distAlpha = map(0, _maxClipDist, 1, 0, dist * _ClipDistScale * (1 - _DeltaHeight)) * radialFactor;

				// color ("premultiply Alpha" kind of Color fade)
				fixed4 col = _Color * max(fadeValue, distAlpha) * _ColorMultiplier;
				col.a *= max(wallAlpha, distAlpha);
				
				return col;
			}

			ENDCG
		}
	}
}
