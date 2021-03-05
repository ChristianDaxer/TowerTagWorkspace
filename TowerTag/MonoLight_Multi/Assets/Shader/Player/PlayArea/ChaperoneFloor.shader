Shader "_OwnShader/Unlit/ChaperoneFloor"
{
	Properties
	{
		_Color("_TintColor", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_ColorMultiplier("Color Multiplier", float ) = 1.5
		_AnimParameter("Animation Parameter", Vector) = (0,0,0,0)

		[Header(Stencil Mask)]
		_Stencil("Stencil ID", Float) = 50
		_StencilComp("Stencil Comparison", Float) = 6
		_StencilOp("Stencil Operation", Float) = 2
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
		LOD 100

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
		}

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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			float4 _AnimParameter;
			float _ColorMultiplier;

			const float TWOPi = 2 * UNITY_PI;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float sinXAbs = _SinTime.x * 0.5 + 0.5;

				// tex Animation
				float2 uvScale = float2(1,(1 + (i.uv2.y) * 1.5 * (sinXAbs)));
				float2 uvOffset = float2((_SinTime.x * sin(i.uv.y * TWOPi * _AnimParameter.x * (sinXAbs)) * _AnimParameter.z) , _Time.x * _AnimParameter.y);
				float2 uvOffset2 = (uvOffset * 0.2) + float2(_SinTime.x * 0.1, _SinTime.x * _AnimParameter.w);

				// wall
				fixed4 texColor = tex2D(_MainTex, (i.uv * uvScale + uvOffset)) + tex2D(_MainTex, (i.uv + uvOffset2));
				float wallAlpha = texColor.r;

				// color ("premultiply Alpha" kind of Color fade)
				fixed4 col = _Color * _ColorMultiplier;
				col.a *= wallAlpha;
				return col;
			}

			ENDCG
		}
	}
}

