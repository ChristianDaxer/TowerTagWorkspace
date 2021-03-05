// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HoloRays"
{
	Properties
	{
		_BeamColor1("BeamColor1", Color) = (0,1,0.9896784,0)
		_BeamColor2("BeamColor2", Color) = (0,1,0.4814487,0)
		_Speed("Speed", Range( -2 , 2)) = -0.2
		_Intensity("Intensity", Float) = 2
		_NoiseScale("NoiseScale", Float) = 5
		_NoisePower("NoisePower", Float) = 7
		_NoiseMul("NoiseMul", Float) = 1
		_BeamAdd("BeamAdd", Range( 0 , 2)) = 0.18
		_BeamMul("BeamMul", Range( 0 , 2)) = 0.2
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		Blend One One , One One
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float _NoiseScale;
		uniform float _Speed;
		uniform float _NoiseMul;
		uniform float _NoisePower;
		uniform float4 _BeamColor1;
		uniform float _BeamMul;
		uniform float _BeamAdd;
		uniform float4 _BeamColor2;
		uniform float _Intensity;


		float2 voronoihash48( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi48( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash48( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			return (F2 + F1) * 0.5;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float time48 = 20.33;
			float mulTime22 = _Time.y * _Speed;
			float2 coords48 = ( i.uv_texcoord + ( 1.0 - ( mulTime22 * 0.1 ) ) ) * _NoiseScale;
			float2 id48 = 0;
			float2 uv48 = 0;
			float voroi48 = voronoi48( coords48, time48, id48, uv48, 0 );
			float temp_output_136_0 = ( 1.0 - i.uv_texcoord.y );
			float mulTime104 = _Time.y * 2.0;
			float2 temp_cast_0 = (round( ( ( i.uv_texcoord.x + 0.218 ) * 16.0 ) )).xx;
			float dotResult4_g1 = dot( temp_cast_0 , float2( 12.9898,78.233 ) );
			float lerpResult10_g1 = lerp( 0.0 , 1.0 , frac( ( sin( dotResult4_g1 ) * 43758.55 ) ));
			float temp_output_78_0 = lerpResult10_g1;
			float4 lerpResult45 = lerp( _BeamColor1 , _BeamColor2 , ( temp_output_78_0 + 0.2 ));
			o.Emission = ( ( pow( ( ( 1.0 - voroi48 ) * _NoiseMul ) , _NoisePower ) * ( saturate( ( ( temp_output_136_0 - 0.7 ) * 0.3 ) ) + ( saturate( ( ( ( temp_output_136_0 - 0.3 ) * 0.15 ) * _BeamColor1 ) ) + ( ( ( ( ( sin( ( mulTime104 + ( ( temp_output_78_0 * 5.0 ) + -3.0 ) ) ) * 0.4 ) + 0.6 ) * ( ( ( sin( ( ( ( i.uv_texcoord.x + -0.0165 ) * 3.141593 ) * 32.0 ) ) * _BeamMul ) - _BeamAdd ) * 10.0 ) ) * ( 1.0 - i.uv_texcoord.y ) ) * lerpResult45 ) ) ) ) * _Intensity ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
-1523;549;1535;446;2896.108;1254.913;1;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;70;-4874.811,-1008.759;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;81;-4552.977,-1008.127;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.218;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;-4367.338,-1017.196;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;16;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;99;-3241.751,-1004.957;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RoundOpNode;80;-4207.524,-1019.982;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;78;-3988.076,-1041.853;Inherit;False;Random Range;-1;;1;7b754edb8aebbfb4a9ace907af661cfc;0;3;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;75;-2858.025,-1041.844;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-0.0165;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;74;-2656.705,-1038.624;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3.141593;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;106;-3240.55,-1241.655;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;117;-3063.567,-1265.522;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;104;-3230.399,-1374.096;Inherit;False;1;0;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;-2485.903,-1037.429;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;32;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;153;-2484.18,-953.625;Inherit;False;Property;_BeamMul;BeamMul;9;0;Create;True;0;0;0;False;0;False;0.2;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-3353.194,-1663.508;Inherit;False;Property;_Speed;Speed;3;0;Create;True;0;0;0;False;0;False;-0.2;-0.2;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;118;-2753.02,-1327.632;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;71;-2304.649,-1040.923;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-2155.052,-1062.456;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;152;-2224.18,-947.6249;Inherit;False;Property;_BeamAdd;BeamAdd;8;0;Create;True;0;0;0;False;0;False;0.18;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;132;-808.8073,-1374.233;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SinOpNode;103;-2417.996,-1410.279;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;22;-2945.969,-1673.504;Inherit;False;1;0;FLOAT;-0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;52;-2628.773,-1735.74;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;101;-1996.936,-1469.797;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.4;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;136;-562.5358,-1311.897;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;111;-1991.628,-1070.323;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.18;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-1850.436,-1029.477;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;10;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;102;-1725.265,-1328.28;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;134;-426.5961,-1197.534;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;53;-2617.09,-1939.146;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;58;-2293.43,-1807.58;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-1660.726,-339.1686;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;54;-2018.629,-1876.69;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;-240.5682,-1083.185;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.15;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;44;-1691.834,-732.2135;Inherit;False;Property;_BeamColor1;BeamColor1;1;0;Create;True;0;0;0;False;0;False;0,1,0.9896784,0;0,1,0.9896784,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;46;-1695.56,-542.6317;Inherit;False;Property;_BeamColor2;BeamColor2;2;0;Create;True;0;0;0;False;0;False;0,1,0.4814487,0;0.9150943,0.05789623,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-1641.106,-1065.4;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;83;-1506.477,-900.9014;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-1701.423,-1448.763;Inherit;False;Property;_NoiseScale;NoiseScale;5;0;Create;True;0;0;0;False;0;False;5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;45;-1242.45,-628.1889;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;144;-116.0951,-929.0811;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.VoronoiNode;48;-1465.192,-1472.384;Inherit;False;0;0;1;3;1;False;40;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;20.33;False;2;FLOAT;10.28;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;146;-276.6703,-1292.917;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.7;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;-1239.712,-952.7689;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;57;-1250.102,-1354.648;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-1280.271,-1256.503;Inherit;False;Property;_NoiseMul;NoiseMul;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-840.9121,-647.0273;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;137;46.83133,-897.7518;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;-55.53564,-1199.807;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.3;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;150;-1056.271,-1372.503;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;138;202.8826,-827.387;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;148;110.3154,-1083.421;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;109;-1187.423,-1204.763;Inherit;False;Property;_NoisePower;NoisePower;6;0;Create;True;0;0;0;False;0;False;7;7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;56;-1004.085,-1282.647;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;149;451.5559,-901.7801;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;49;281.89,-595.3873;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;85;269.3349,-452.4829;Inherit;False;Property;_Intensity;Intensity;4;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;501.6197,-577.0408;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;909.8077,-734.5383;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;HoloRays;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;2;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;4;1;False;-1;1;False;-1;4;1;False;-1;1;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;81;0;70;1
WireConnection;79;0;81;0
WireConnection;80;0;79;0
WireConnection;78;1;80;0
WireConnection;75;0;99;1
WireConnection;74;0;75;0
WireConnection;106;0;78;0
WireConnection;117;0;106;0
WireConnection;72;0;74;0
WireConnection;118;0;104;0
WireConnection;118;1;117;0
WireConnection;71;0;72;0
WireConnection;110;0;71;0
WireConnection;110;1;153;0
WireConnection;103;0;118;0
WireConnection;22;0;69;0
WireConnection;52;0;22;0
WireConnection;101;0;103;0
WireConnection;136;0;132;2
WireConnection;111;0;110;0
WireConnection;111;1;152;0
WireConnection;77;0;111;0
WireConnection;102;0;101;0
WireConnection;134;0;136;0
WireConnection;58;0;52;0
WireConnection;47;0;78;0
WireConnection;54;0;53;0
WireConnection;54;1;58;0
WireConnection;139;0;134;0
WireConnection;29;0;102;0
WireConnection;29;1;77;0
WireConnection;83;0;99;2
WireConnection;45;0;44;0
WireConnection;45;1;46;0
WireConnection;45;2;47;0
WireConnection;144;0;139;0
WireConnection;144;1;44;0
WireConnection;48;0;54;0
WireConnection;48;2;108;0
WireConnection;146;0;136;0
WireConnection;40;0;29;0
WireConnection;40;1;83;0
WireConnection;57;0;48;0
WireConnection;43;0;40;0
WireConnection;43;1;45;0
WireConnection;137;0;144;0
WireConnection;147;0;146;0
WireConnection;150;0;57;0
WireConnection;150;1;151;0
WireConnection;138;0;137;0
WireConnection;138;1;43;0
WireConnection;148;0;147;0
WireConnection;56;0;150;0
WireConnection;56;1;109;0
WireConnection;149;0;148;0
WireConnection;149;1;138;0
WireConnection;49;0;56;0
WireConnection;49;1;149;0
WireConnection;84;0;49;0
WireConnection;84;1;85;0
WireConnection;0;2;84;0
ASEEND*/
//CHKSM=CDC90FA43DFA2D62E2DCBF5EA9DE0A646AB22CCB