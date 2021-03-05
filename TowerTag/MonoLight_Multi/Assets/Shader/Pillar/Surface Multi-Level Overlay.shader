// Upgrade NOTE: upgraded instancing buffer '_OwnShaderMultiLevelOverlay' to new syntax.

// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "_OwnShader/Multi-Level Overlay"
{
	Properties
	{
		_LayersRGB("Layers (RGB)", 2D) = "white" {}
		[HDR]_TintColor("TintColor", Color) = (1,0,0,0)
		_LayerBlendFactors("Layer Blend Factors", Vector) = (0,0,0,0)
		_UVOffset("UVOffset", Vector) = (0,0,0,0)
		_TilingCracks("TilingCracks", Vector) = (0.7,0.7,0,0)
		_MetallicGlossMap("Metallic and Smoothness", 2D) = "white" {}
		[Normal]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("BumpScale", Float) = 1
		[Toggle(_PERVERTEXNORMAL_ON)] _PerVertexNormal("PerVertexNormal", Float) = 0
		_Albedo("Albedo", 2D) = "white" {}
		_Smoothnessfactor("Smoothnessfactor", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature_local _PERVERTEXNORMAL_ON
		#include "Assets/Shader/CombinedMeshUtils.hlsl"
		#include "Assets/Shader/CombinedMeshUtils/CombinedMeshTransform.hlsl"
		#pragma only_renderers d3d11_9x d3d11 glcore gles3 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _BumpMap;
		uniform half _BumpScale;
		uniform sampler2D _Albedo;
		uniform sampler2D _LayersRGB;
		uniform float2 _TilingCracks;
		uniform sampler2D _MetallicGlossMap;
		uniform float _Smoothnessfactor;

		UNITY_INSTANCING_BUFFER_START(_OwnShaderMultiLevelOverlay)
			UNITY_DEFINE_INSTANCED_PROP(half4, _BumpMap_ST)
#define _BumpMap_ST_arr _OwnShaderMultiLevelOverlay
			UNITY_DEFINE_INSTANCED_PROP(half4, _Albedo_ST)
#define _Albedo_ST_arr _OwnShaderMultiLevelOverlay
			UNITY_DEFINE_INSTANCED_PROP(float4, _TintColor)
#define _TintColor_arr _OwnShaderMultiLevelOverlay
			UNITY_DEFINE_INSTANCED_PROP(half4, _MetallicGlossMap_ST)
#define _MetallicGlossMap_ST_arr _OwnShaderMultiLevelOverlay
			UNITY_DEFINE_INSTANCED_PROP(half3, _LayerBlendFactors)
#define _LayerBlendFactors_arr _OwnShaderMultiLevelOverlay
			UNITY_DEFINE_INSTANCED_PROP(half2, _UVOffset)
#define _UVOffset_arr _OwnShaderMultiLevelOverlay
		UNITY_INSTANCING_BUFFER_END(_OwnShaderMultiLevelOverlay)

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			half4 _BumpMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_BumpMap_ST_arr, _BumpMap_ST);
			float2 uv_BumpMap = i.uv_texcoord * _BumpMap_ST_Instance.xy + _BumpMap_ST_Instance.zw;
			#ifdef _PERVERTEXNORMAL_ON
				half3 staticSwitch35_g7 = half3(0,0,1);
			#else
				half3 staticSwitch35_g7 = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap ), _BumpScale );
			#endif
			o.Normal = staticSwitch35_g7;
			half4 _Albedo_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_Albedo_ST_arr, _Albedo_ST);
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST_Instance.xy + _Albedo_ST_Instance.zw;
			o.Albedo = tex2D( _Albedo, uv_Albedo ).rgb;
			half mulTime7_g4 = _Time.y * 2.0;
			half2 _UVOffset_Instance = UNITY_ACCESS_INSTANCED_PROP(_UVOffset_arr, _UVOffset);
			float2 uv_TexCoord3_g4 = i.uv_texcoord * _TilingCracks + _UVOffset_Instance;
			half4 tex2DNode5_g4 = tex2D( _LayersRGB, uv_TexCoord3_g4 );
			half3 _LayerBlendFactors_Instance = UNITY_ACCESS_INSTANCED_PROP(_LayerBlendFactors_arr, _LayerBlendFactors);
			half dotResult15_g4 = dot( tex2DNode5_g4 , half4( _LayerBlendFactors_Instance , 0.0 ) );
			float4 _TintColor_Instance = UNITY_ACCESS_INSTANCED_PROP(_TintColor_arr, _TintColor);
			o.Emission = ( max( abs( sin( ( mulTime7_g4 + ( 10.0 * tex2DNode5_g4.b ) ) ) ) , 0.25 ) * dotResult15_g4 * _TintColor_Instance ).rgb;
			half4 _MetallicGlossMap_ST_Instance = UNITY_ACCESS_INSTANCED_PROP(_MetallicGlossMap_ST_arr, _MetallicGlossMap_ST);
			float2 uv_MetallicGlossMap = i.uv_texcoord * _MetallicGlossMap_ST_Instance.xy + _MetallicGlossMap_ST_Instance.zw;
			half4 break161 = tex2D( _MetallicGlossMap, uv_MetallicGlossMap );
			o.Metallic = break161;
			o.Smoothness = ( break161.a * _Smoothnessfactor );
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
2562;428;1920;853;-1372.68;1824.618;1;True;True
Node;AmplifyShaderEditor.FunctionNode;160;2055.262,-1700.902;Inherit;False;MetallicOutput;10;;2;addb0945cb0e34341aeb85fb9329cb89;0;0;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;161;2303.262,-1676.902;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;156;2169.678,-1358.763;Float;False;Property;_Smoothnessfactor;Smoothnessfactor;17;0;Create;True;0;0;0;False;0;False;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;169;2037.081,-1154.321;Inherit;False;SampleDistributedLocalPosition;-1;;3;8dc65b1de90dcb74da223e5da90c9eea;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.FunctionNode;158;2069.256,-1803.732;Inherit;False;MultiLevelOverlay;0;;4;1d3c3c6c6c12f634fb4f5364e7e744ce;0;0;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;162;2457.262,-1584.902;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;170;2582.081,-2036.321;Inherit;False;Constant;_Color0;Color 0;5;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;2149.257,-2252.746;Inherit;True;Property;_Albedo;Albedo;16;0;Create;True;0;0;0;False;0;False;-1;None;201de2387ec918843bf832691e90f164;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;171;2212.262,-1997.902;Inherit;False;BumpOutput;12;;7;4e78a794c49a70d4b84fa746d324b530;0;0;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2654.567,-1859.047;Half;False;True;-1;2;ASEMaterialInspector;0;0;Standard;_OwnShader/Multi-Level Overlay;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;4;d3d11_9x;d3d11;glcore;gles3;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;2;Include;;True;d99a4602d54a13a40b2b35c6ac3c99a8;Custom;Include;;True;840a30e678702e24996e2e0ffd546d5c;Custom;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;161;0;160;0
WireConnection;162;0;161;3
WireConnection;162;1;156;0
WireConnection;0;0;2;0
WireConnection;0;1;171;0
WireConnection;0;2;158;0
WireConnection;0;3;161;0
WireConnection;0;4;162;0
ASEEND*/
//CHKSM=EC5DCC51D24AC0387FA2918246D80820CC2C5292