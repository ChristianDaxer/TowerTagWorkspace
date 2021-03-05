// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ForceField"
{
	Properties
	{
		_NoiseMaskPower("Noise Mask Power", Range( 0 , 10)) = 1
		_Noise01("Noise 01", 2D) = "white" {}
		_Noise01Tiling("Noise 01 Tiling", Float) = 1
		_Noise01ScrollSpeed("Noise 01 Scroll Speed", Float) = 0.25
		[Toggle(_NOISE02ENABLED_ON)] _Noise02Enabled("Noise 02 Enabled", Float) = 0
		_Noise02("Noise 02", 2D) = "white" {}
		_Noise02Tiling("Noise 02 Tiling", Float) = 1
		_Noise02ScrollSpeed("Noise 02 Scroll Speed", Float) = 0.25
		[Toggle(_NOISEDISTORTIONENABLED_ON)] _NoiseDistortionEnabled("Noise Distortion Enabled", Float) = 1
		_NoiseDistortion("Noise Distortion", 2D) = "white" {}
		_NoiseDistortionPower("Noise Distortion Power", Range( 0 , 2)) = 0.5
		_NoiseDistortionTiling("Noise Distortion Tiling", Float) = 0.5
		_EmissionColor("Emission Color", Color) = (0,0,0,0)
		_FresnelOffset("Fresnel Offset", Float) = 0
		_Opacity("Opacity", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma shader_feature_local _NOISEDISTORTIONENABLED_ON
		#pragma shader_feature_local _NOISE02ENABLED_ON
		#pragma only_renderers d3d11_9x d3d11 glcore gles3 metal 
		#pragma surface surf Unlit alpha:fade keepalpha noshadow exclude_path:deferred noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			half3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			half ASEVFace : VFACE;
		};

		uniform half4 _EmissionColor;
		uniform sampler2D _Noise01;
		uniform float _Noise01Tiling;
		uniform float _Noise01ScrollSpeed;
		uniform sampler2D _NoiseDistortion;
		uniform float _NoiseDistortionTiling;
		uniform float _NoiseDistortionPower;
		uniform sampler2D _Noise02;
		uniform float _Noise02Tiling;
		uniform float _Noise02ScrollSpeed;
		uniform float _NoiseMaskPower;
		uniform half _FresnelOffset;
		uniform half _Opacity;


		inline float4 TriplanarSampling395( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			half3 ase_worldNormal = WorldNormalVector( i, half3( 0, 0, 1 ) );
			half3 temp_output_419_0 = abs( ase_worldNormal );
			half3 temp_output_423_0 = ( temp_output_419_0 * temp_output_419_0 );
			float3 ase_worldPos = i.worldPos;
			float4 temp_cast_1 = (0.0).xxxx;
			float4 triplanar395 = TriplanarSampling395( _NoiseDistortion, ase_worldPos, ase_worldNormal, 1.0, _NoiseDistortionTiling, 1.0, 0 );
			#ifdef _NOISEDISTORTIONENABLED_ON
				float4 staticSwitch407 = ( triplanar395 * _NoiseDistortionPower );
			#else
				float4 staticSwitch407 = temp_cast_1;
			#endif
			half4 break415 = ( half4( ( ase_worldPos * _Noise01Tiling ) , 0.0 ) + ( _Time.y * _Noise01ScrollSpeed ) + staticSwitch407 );
			half2 appendResult427 = (half2(break415.y , break415.z));
			half2 appendResult422 = (half2(break415.z , break415.x));
			half2 appendResult424 = (half2(break415.x , break415.y));
			half3 weightedBlendVar436 = temp_output_423_0;
			half weightedBlend436 = ( weightedBlendVar436.x*tex2D( _Noise01, appendResult427 ).r + weightedBlendVar436.y*tex2D( _Noise01, appendResult422 ).r + weightedBlendVar436.z*tex2D( _Noise01, appendResult424 ).r );
			half4 break412 = ( half4( ( ase_worldPos * _Noise02Tiling ) , 0.0 ) + ( _Time.y * _Noise02ScrollSpeed ) + staticSwitch407 );
			half2 appendResult416 = (half2(break412.y , break412.z));
			half2 appendResult417 = (half2(break412.z , break412.x));
			half2 appendResult418 = (half2(break412.x , break412.y));
			half3 weightedBlendVar433 = temp_output_423_0;
			half weightedBlend433 = ( weightedBlendVar433.x*tex2D( _Noise02, appendResult416 ).r + weightedBlendVar433.y*tex2D( _Noise02, appendResult417 ).r + weightedBlendVar433.z*tex2D( _Noise02, appendResult418 ).r );
			#ifdef _NOISE02ENABLED_ON
				float staticSwitch435 = weightedBlend433;
			#else
				float staticSwitch435 = 1.0;
			#endif
			o.Emission = ( _EmissionColor * ( weightedBlend436 * staticSwitch435 * _NoiseMaskPower ) ).rgb;
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			half dotResult440 = dot( ase_worldNormal , ( i.ASEVFace * ase_worldViewDir ) );
			half clampResult445 = clamp( dotResult440 , 0.0 , 1.0 );
			o.Alpha = ( ( ( 1.0 - clampResult445 ) + _FresnelOffset ) * _Opacity );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;-1404;2560;1383;-933.6747;722.7501;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;392;-2204.011,-124.7491;Float;False;Property;_NoiseDistortionTiling;Noise Distortion Tiling;11;0;Create;True;0;0;0;False;0;False;0.5;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;393;-2188.191,-317.076;Float;True;Property;_NoiseDistortion;Noise Distortion;9;0;Create;True;0;0;0;False;0;False;None;3c4ea3a033cf4ed09936c7bd439486a4;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;394;-1766.605,-73.49313;Float;False;Property;_NoiseDistortionPower;Noise Distortion Power;10;0;Create;True;0;0;0;False;0;False;0.5;1.069;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;395;-1851.084,-267.7451;Inherit;True;Spherical;World;False;Top Texture 0;_TopTexture0;white;0;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;396;-1293.878,150.1805;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;397;-1436.81,-62.18402;Float;False;Constant;_Float3;Float 3;23;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;398;-1320.785,536.2369;Float;False;Property;_Noise02ScrollSpeed;Noise 02 Scroll Speed;7;0;Create;True;0;0;0;False;0;False;0.25;0.125;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;399;-1293.459,295.199;Float;False;Property;_Noise02Tiling;Noise 02 Tiling;6;0;Create;True;0;0;0;False;0;False;1;0.125;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;400;-1435.605,-164.492;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TimeNode;401;-1293.025,391.636;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;403;-974.9821,449.136;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;402;-1369.756,-821.7343;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TimeNode;404;-1331.194,-540.611;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;408;-1355.321,-391.014;Float;False;Property;_Noise01ScrollSpeed;Noise 01 Scroll Speed;3;0;Create;True;0;0;0;False;0;False;0.25;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;407;-1219.711,-125.883;Float;False;Property;_NoiseDistortionEnabled;Noise Distortion Enabled;8;0;Create;True;0;0;0;False;0;False;0;1;1;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;405;-1331.627,-634.4471;Float;False;Property;_Noise01Tiling;Noise 01 Tiling;2;0;Create;True;0;0;0;False;0;False;1;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;406;-978.9341,207.391;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;409;-1013.152,-483.111;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;410;-1017.105,-724.855;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;411;-776.1041,324.0479;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FaceVariableNode;443;1499.114,572.0248;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;414;-814.2731,-608.198;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;439;1439.114,402.0248;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BreakToComponentsNode;412;-597.5059,325.7249;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WorldNormalVector;413;112.2941,-68.81712;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;438;1476.114,198.0248;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;444;1704.114,397.0248;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;417;-99.67004,346.836;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.AbsOpNode;419;335.9401,-67.54903;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;420;-175.2382,599.202;Float;True;Property;_Noise02;Noise 02;5;0;Create;True;0;0;0;False;0;False;None;a2bcabfdbdea49dda2cbe9fe713af97a;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.BreakToComponentsNode;415;-642.4862,-615.0301;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;416;-101.0511,192.503;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;418;-98.29114,499.7931;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;422;-132.8411,-584.4111;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;427;-139.2191,-739.7441;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;421;169.438,557.055;Inherit;True;Property;_TextureSample1;Texture Sample 1;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;423;503.6431,-72.74992;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;426;-213.4061,-333.046;Float;True;Property;_Noise01;Noise 01;1;0;Create;True;0;0;0;False;0;False;None;c9b256f565c646a38aa183bd548de557;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;425;164.239,156.6539;Inherit;True;Property;_TextureSample2;Texture Sample 2;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;428;164.238,351.6529;Inherit;True;Property;_TextureSample0;Texture Sample 0;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;424;-136.4591,-432.455;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;440;1864.114,244.0248;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SummedBlendNode;433;777.8418,343.8541;Inherit;False;5;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;432;807.8838,488.18;Float;False;Constant;_Float8;Float 8;24;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;430;131.269,-375.1929;Inherit;True;Property;_TextureSample4;Texture Sample 4;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;429;126.0701,-580.5931;Inherit;True;Property;_TextureSample5;Texture Sample 5;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;445;2032.114,425.0248;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;431;126.071,-775.5931;Inherit;True;Property;_TextureSample3;Texture Sample 3;15;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;435;1020.532,397.5719;Float;False;Property;_Noise02Enabled;Noise 02 Enabled;4;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SummedBlendNode;436;739.6709,-588.393;Inherit;False;5;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;434;821.9658,-41.92113;Float;False;Property;_NoiseMaskPower;Noise Mask Power;0;0;Create;True;0;0;0;False;0;False;1;3.07;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;446;2247.114,366.0248;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;450;2114.114,601.0248;Inherit;False;Property;_FresnelOffset;Fresnel Offset;13;0;Create;False;0;0;0;False;0;False;0;0.25;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;449;2424.114,461.0248;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;437;1303.747,-230.666;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;447;1681.114,-628.9752;Inherit;False;Property;_EmissionColor;Emission Color;12;0;Create;False;0;0;0;False;0;False;0,0,0,0;0,0.9791293,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;451;2519.675,567.2499;Inherit;False;Property;_Opacity;Opacity;14;0;Create;False;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;448;1954.114,-366.9752;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;452;2714.675,405.2499;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2703.978,-293.8422;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;ForceField;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;5;d3d11_9x;d3d11;glcore;gles3;metal;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;395;0;393;0
WireConnection;395;3;392;0
WireConnection;400;0;395;0
WireConnection;400;1;394;0
WireConnection;403;0;401;2
WireConnection;403;1;398;0
WireConnection;407;1;397;0
WireConnection;407;0;400;0
WireConnection;406;0;396;0
WireConnection;406;1;399;0
WireConnection;409;0;404;2
WireConnection;409;1;408;0
WireConnection;410;0;402;0
WireConnection;410;1;405;0
WireConnection;411;0;406;0
WireConnection;411;1;403;0
WireConnection;411;2;407;0
WireConnection;414;0;410;0
WireConnection;414;1;409;0
WireConnection;414;2;407;0
WireConnection;412;0;411;0
WireConnection;444;0;443;0
WireConnection;444;1;439;0
WireConnection;417;0;412;2
WireConnection;417;1;412;0
WireConnection;419;0;413;0
WireConnection;415;0;414;0
WireConnection;416;0;412;1
WireConnection;416;1;412;2
WireConnection;418;0;412;0
WireConnection;418;1;412;1
WireConnection;422;0;415;2
WireConnection;422;1;415;0
WireConnection;427;0;415;1
WireConnection;427;1;415;2
WireConnection;421;0;420;0
WireConnection;421;1;418;0
WireConnection;423;0;419;0
WireConnection;423;1;419;0
WireConnection;425;0;420;0
WireConnection;425;1;416;0
WireConnection;428;0;420;0
WireConnection;428;1;417;0
WireConnection;424;0;415;0
WireConnection;424;1;415;1
WireConnection;440;0;438;0
WireConnection;440;1;444;0
WireConnection;433;0;423;0
WireConnection;433;1;425;1
WireConnection;433;2;428;1
WireConnection;433;3;421;1
WireConnection;430;0;426;0
WireConnection;430;1;424;0
WireConnection;429;0;426;0
WireConnection;429;1;422;0
WireConnection;445;0;440;0
WireConnection;431;0;426;0
WireConnection;431;1;427;0
WireConnection;435;1;432;0
WireConnection;435;0;433;0
WireConnection;436;0;423;0
WireConnection;436;1;431;1
WireConnection;436;2;429;1
WireConnection;436;3;430;1
WireConnection;446;0;445;0
WireConnection;449;0;446;0
WireConnection;449;1;450;0
WireConnection;437;0;436;0
WireConnection;437;1;435;0
WireConnection;437;2;434;0
WireConnection;448;0;447;0
WireConnection;448;1;437;0
WireConnection;452;0;449;0
WireConnection;452;1;451;0
WireConnection;0;2;448;0
WireConnection;0;9;452;0
ASEEND*/
//CHKSM=E4E252923F5C6F0D327165055BF112CE7C0D263B