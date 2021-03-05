// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "_OwnShader/SurfaceFresnel_NoiseAlpha_Unlit_TwoFaced" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGBA)", 2D) = "white" {}
		_NoiseTex ("Noise Texture (RGBA)", 2D) = "white" {}

		[Header(Fresnel)]
		_FresnelExponent( "FresnelExponent", Range(0.0001, 6) ) = 1
		_MinAlpha("MinAlpha", Range(0,1)) = 1

		[Header(Tesselation)]
		_Tess("TesselationFactor", Vector) = (1,1,1,1)
		_EdgeLength("EdgeLength", Range(2, 100)) = 15
		_PhongTess("Phong Tesselation Strength", Range(0,2)) = 0

		[Header(VertexDisplacement (Noise Tex (RG)))]
		[KeywordEnum(vertexNormal, vertexPosObj, vertexColor, dispVector)] _DisplacementVariant("Displace along", Int) = 0

		_DispVector("Displacement Direction (XYZ)", Vector) = (0, 0, 0, 0)
		_DispScale("Displacement TexScale (XY: T1 ZW: T2)", Vector) = (1, 1, 1, 1)
		_DispOffset("Displacement Offset", float) = -0.5
		_DispFactor("Displacement Strength", Float) = 0
		_DisplacementSpeed("Displace Speed X:T1 Y:SinT1 Z:T2 W: SinT2", Vector) = (0.1, 0.01, 0.1, 0.01)

		[Header(Noise Alpha (Noise Tex (BA)))]
		_NoiseAlphaDirection("Noise Alpha Movement XY: SinTime ZW:Time", Vector) = (2.65, 5.15, 1.1, 2.05)
		_NoiseAlphaFactor("Noise Alpha Multiplicator", Float) = 1

		[Header(Additive PreMultiply)]
		[Toggle] _UseAdditive("Use Aditive Multiplier", int) = 0
		_AdditiveFactor("Additive Multiplier", float) = 1

		// ***** UI-Stencil (only needed when Shader is used in UI) *****
		[Header(When used in UI)]
		[HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask("Color Mask", Float) = 15
		// ***************************************************************

	}
	SubShader {
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		LOD 200
		Lighting Off
		Cull Off
		
		// ***** UI-Stencil (only needed when Shader is used in UI) *****
		ColorMask[_ColorMask]

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
		// ***************************************************************
		

		CGPROGRAM

		#pragma shader_feature _DISPLACEMENTVARIANT_VERTEXNORMAL _DISPLACEMENTVARIANT_VERTEXPOSOBJ _DISPLACEMENTVARIANT_VERTEXCOLOR _DISPLACEMENTVARIANT_DISPVECTOR
		#pragma shader_feature _USEADDITIVE_ON

		// Unlit
		#pragma surface surf NoLighting fullforwardshadows vertex:vert tessellate:tess tessphong:_PhongTess alpha:fade nolightmap noforwardadd nofog

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 5.0

		#include "Tessellation.cginc"

		sampler2D _MainTex;
		sampler2D _NoiseTex;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_NoiseTex;
			float3 worldNormal;
			float3 viewDir;
		};

		struct appdata
		{
			float4 vertex : POSITION;
			float4 color : Color;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
		};

		fixed4 _Color, _FresnelColor;
		float4 _Tess;
		float _DispFactor;
		half _FresnelExponent, _MinAlpha;
		float _PhongTess;
		float _EdgeLength;
		float _NoiseAlphaFactor;
		float4 _DisplacementSpeed;
		float4 _DispScale;
		float4 _DispVector;
		float _DispOffset;
		float4 _NoiseAlphaDirection;
		float _AdditiveFactor;

		float4 tess(appdata v0, appdata v1, appdata v2)
		{
			return _Tess * UnityEdgeLengthBasedTess(v0.vertex, v1.vertex, v2.vertex, _EdgeLength);
		}

		void vert(inout appdata v)
		{

			float c = (tex2Dlod(_NoiseTex, float4(v.vertex.x * _DispScale.x, _DispScale.y * v.vertex.y - _Time.x * _DisplacementSpeed.x - _DisplacementSpeed.y *  _SinTime.y ,0,0)).r
							+ tex2Dlod(_NoiseTex, float4(v.vertex.x * _DispScale.z, _DispScale.w * v.vertex.y + _Time.y * _DisplacementSpeed.z + _DisplacementSpeed.w *  _SinTime.z ,0,0)).g) + (2 * _DispOffset);

			#ifdef _DISPLACEMENTVARIANT_VERTEXNORMAL
				v.vertex =  v.vertex + float4(v.normal, 0) * c * _DispFactor ;
			#elif _DISPLACEMENTVARIANT_VERTEXPOSOBJ
				v.vertex = v.vertex + float4(v.vertex.x, _SinTime.x,  v.vertex.z, 0) * c * _DispFactor;
			#elif _DISPLACEMENTVARIANT_VERTEXCOLOR
				v.vertex = v.vertex + float4(((v.color.rgb * 2) - 1),0) * c * _DispFactor;
			#elif _DISPLACEMENTVARIANT_DISPVECTOR
				v.vertex =  v.vertex + float4((_DispVector.xyz), 0) * c * _DispFactor;
			#endif

		}


		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutput o) 
		{
			half fresnel = 1 - saturate(dot(normalize(IN.worldNormal), normalize(IN.viewDir)));
			fresnel = pow(fresnel, _FresnelExponent);

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = lerp(_FresnelColor, c.rgb, 1 - fresnel);

			float2 uv = IN.uv_NoiseTex;
			float alphaFactor =  (tex2D(_NoiseTex, ((uv + _NoiseAlphaDirection.xy * _SinTime.x))).b 
								+ tex2D(_NoiseTex, ((uv - _NoiseAlphaDirection.zw * _Time.x))).a) * _NoiseAlphaFactor;

			half alpha = max(_MinAlpha, fresnel);
			o.Alpha = alpha * alphaFactor * c.a;
		}

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			#ifdef _USEADDITIVE_ON
				return fixed4(s.Albedo * _AdditiveFactor * s.Alpha, s.Alpha);
			#else
				return fixed4(s.Albedo, s.Alpha);
			#endif
		}

		ENDCG
	}
	//CustomEditor "ShaderGUI"
	FallBack "Diffuse"
}
