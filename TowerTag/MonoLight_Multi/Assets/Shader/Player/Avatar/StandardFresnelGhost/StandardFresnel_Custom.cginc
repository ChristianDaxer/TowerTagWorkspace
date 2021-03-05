// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// ------------------------------------------------------------------
//  Base forward pass (directional light, emission, lightmaps, ...)

	// Micha changed
	half _Blend, _ColorBlend;
	#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
		half _FresnelExponent, _MinAlpha;
		sampler2D _NoiseTex;
		float _NoiseScale;
		float _NoiseAlphpaFactor;
		float _DispFactor;
	#endif

	struct VertexInput_2
	{
		float4 vertex	: POSITION;
		half3 normal	: NORMAL;
		float2 uv0		: TEXCOORD0;
		float2 uv1		: TEXCOORD1;
		float4 color	: COLOR;

		#if defined(DYNAMICLIGHTMAP_ON) || defined(UNITY_PASS_META)
			float2 uv2		: TEXCOORD2;
		#endif
		
		#ifdef _TANGENT_TO_WORLD
			half4 tangent	: TANGENT;
		#endif
		
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};


	float4 TexCoords_2(VertexInput_2 v)
	{
		float4 texcoord;
		texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
		texcoord.zw = TRANSFORM_TEX(((_UVSec == 0) ? v.uv0 : v.uv1), _DetailAlbedoMap);
		return texcoord;
	}

	inline half4 VertexGIForward_2(VertexInput_2 v, float3 posWorld, half3 normalWorld)
	{
		half4 ambientOrLightmapUV = 0;
		
		// Static lightmaps
		#ifdef LIGHTMAP_ON
			ambientOrLightmapUV.xy = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
			ambientOrLightmapUV.zw = 0;

		// Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
		#elif UNITY_SHOULD_SAMPLE_SH
			#ifdef VERTEXLIGHT_ON
					// Approximated illumination from non-important point lights
					ambientOrLightmapUV.rgb = Shade4PointLights(
						unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
						unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
						unity_4LightAtten0, posWorld, normalWorld);
			#endif

			ambientOrLightmapUV.rgb = ShadeSHPerVertex(normalWorld, ambientOrLightmapUV.rgb);

		#endif

		#ifdef DYNAMICLIGHTMAP_ON
			ambientOrLightmapUV.zw = v.uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
		#endif

		return ambientOrLightmapUV;
	}

	struct VertexOutputForwardBase_2
	{
		UNITY_POSITION(pos);
		float4 tex								: TEXCOORD0;
		half3 eyeVec							: TEXCOORD1;
		float4 tangentToWorldAndPackedData[3]   : TEXCOORD2;    // [3x3:tangentToWorld | 1x3:viewDirForParallax or worldPos]
		half4 ambientOrLightmapUV				: TEXCOORD5;    // SH or Lightmap UV
		UNITY_SHADOW_COORDS(6)
		UNITY_FOG_COORDS(7)

		// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
		#if UNITY_REQUIRE_FRAG_WORLDPOS && !UNITY_PACK_WORLDPOS_WITH_TANGENT
			float3 posWorld						: TEXCOORD8;
		#endif

		UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO

		// Micha Changed
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			float3 normal : NORMAL;
			float3 viewDir : NORMAL1;
			float3 objPos : NORMAL2;
		#endif
	};

	VertexOutputForwardBase_2 vertForwardBase_2(VertexInput_2 v)
	{
		UNITY_SETUP_INSTANCE_ID(v);
		VertexOutputForwardBase_2 o;
		UNITY_INITIALIZE_OUTPUT(VertexOutputForwardBase_2, o);
		UNITY_TRANSFER_INSTANCE_ID(v, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		float4 posWorld = mul(unity_ObjectToWorld, v.vertex);

		#if UNITY_REQUIRE_FRAG_WORLDPOS
			#if UNITY_PACK_WORLDPOS_WITH_TANGENT
				o.tangentToWorldAndPackedData[0].w = posWorld.x;
				o.tangentToWorldAndPackedData[1].w = posWorld.y;
				o.tangentToWorldAndPackedData[2].w = posWorld.z;

			#else
				o.posWorld = posWorld.xyz;

			#endif
		#endif


		// Micha
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			fixed4 disp = tex2Dlod(_NoiseTex, float4(v.vertex.r, v.vertex.g - _Time.x * 0.1 - 0.01 *  _SinTime.y, 0, 0))
						+ tex2Dlod(_NoiseTex, float4(v.vertex.r, v.vertex.g + _Time.y * 0.1 + 0.01 *  _SinTime.z, 0, 0));

			#ifdef _DISPLACEMENTVARIANT_VERTEXNORMAL
				float4 vertex = v.vertex + float4(v.normal, 0) * disp.r * _DispFactor * (1 - _Blend);

			#elif _DISPLACEMENTVARIANT_VERTEXPOSOBJ
				float4 vertex = v.vertex + float4(v.vertex.x, _SinTime.x, v.vertex.z, 0) * disp.r * _DispFactor * (1 - _Blend);

			#elif _DISPLACEMENTVARIANT_VERTEXCOLOR
				float4 vertex = v.vertex + float4((v.color.rgb * 2 - 1), 0) * disp.r * _DispFactor * (1 - _Blend);

			#else 
				float4 vertex = v.vertex;

			#endif

			o.pos = UnityObjectToClipPos(vertex);
			o.objPos = vertex;

		#else
			o.pos = UnityObjectToClipPos(v.vertex);

		#endif
		
		o.tex = TexCoords_2(v);
		o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
		float3 normalWorld = UnityObjectToWorldNormal(v.normal);
		
		#ifdef _TANGENT_TO_WORLD
			float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

			float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
			o.tangentToWorldAndPackedData[0].xyz = tangentToWorld[0];
			o.tangentToWorldAndPackedData[1].xyz = tangentToWorld[1];
			o.tangentToWorldAndPackedData[2].xyz = tangentToWorld[2];

		#else
			o.tangentToWorldAndPackedData[0].xyz = 0;
			o.tangentToWorldAndPackedData[1].xyz = 0;
			o.tangentToWorldAndPackedData[2].xyz = normalWorld;

		#endif

		//We need this for shadow receving
		UNITY_TRANSFER_SHADOW(o, v.uv1);
		o.ambientOrLightmapUV = VertexGIForward_2(v, posWorld, normalWorld);

		#ifdef _PARALLAXMAP
			TANGENT_SPACE_ROTATION;
			half3 viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
			o.tangentToWorldAndPackedData[0].w = viewDirForParallax.x;
			o.tangentToWorldAndPackedData[1].w = viewDirForParallax.y;
			o.tangentToWorldAndPackedData[2].w = viewDirForParallax.z;

		#endif

		UNITY_TRANSFER_FOG(o, o.pos);

		// Micha changed
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.viewDir = UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex));
		#endif

		return o;
	}


	half4 fragForwardBaseInternal_2(VertexOutputForwardBase_2 i) : SV_Target
	{
		UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

		// *** Micha changed *************
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			fixed blendFactor = lerp(1, tex2D(_MainTex, i.tex).a, _Blend);

		#else
			fixed blendFactor = tex2D(_MainTex, i.tex).a;

		#endif

		_Color.rgb = lerp(fixed3(1, 1, 1), _Color.rgb, blendFactor);

		// *******************************

		FRAGMENT_SETUP(s)

		UNITY_SETUP_INSTANCE_ID(i);
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

		UnityLight mainLight = MainLight();
		UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld);

		half occlusion = Occlusion(i.tex.xy);
		UnityGI gi = FragmentGI(s, occlusion, i.ambientOrLightmapUV, atten, mainLight);

		half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, gi.light, gi.indirect);
		c.rgb += Emission(i.tex.xy);

		// ***** Micha changed ******
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON

			half fresnel = 1 - saturate(dot(normalize(i.normal), normalize(i.viewDir)));
			fresnel = pow(fresnel, _FresnelExponent);
		
			half alpha = max(_MinAlpha, fresnel * s.alpha);
			c.rgb = lerp(lerp(_Color.rgb, c.rgb, (1 - (fresnel * s.alpha)) * _ColorBlend), c.rgb, _Blend);

			// Pseudo UV-Projection
			float2 uv = i.objPos.zy;

			float alphaFactor = (tex2D(_NoiseTex, ((uv + float2(0.3, 0.3) * _SinTime.y * 0.5)) * _NoiseScale).r
								+ tex2D(_NoiseTex, ((uv - float2(0.5, 0.25) * _SinTime.z * 0.2)) * _NoiseScale).r) * _NoiseAlphpaFactor;

			s.alpha = lerp(alpha * alphaFactor, 1, _Blend);

		#endif
		// ***********************************

		UNITY_APPLY_FOG(i.fogCoord, c.rgb);
		return OutputForward(c, s.alpha);
	}



// ------------------------------------------------------------------
//  Additive forward pass (one light per pass)

	struct VertexOutputForwardAdd_2
	{
		UNITY_POSITION(pos);
		float4 tex								: TEXCOORD0;
		half3 eyeVec							: TEXCOORD1;
		float4 tangentToWorldAndLightDir[3]		: TEXCOORD2;    // [3x3:tangentToWorld | 1x3:lightDir]
		float3 posWorld							: TEXCOORD5;
		UNITY_SHADOW_COORDS(6)
		UNITY_FOG_COORDS(7)

		// next ones would not fit into SM2.0 limits, but they are always for SM3.0+
		#if defined(_PARALLAXMAP)
				half3 viewDirForParallax            : TEXCOORD8;
		#endif

		// Micha Changed
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			float3 normal : NORMAL;
			float3 viewDir : NORMAL1;
			float2 objPos : NORMAL2;
		#endif

		UNITY_VERTEX_OUTPUT_STEREO
	};

	VertexOutputForwardAdd_2 vertForwardAdd_2(VertexInput_2 v)
	{
		UNITY_SETUP_INSTANCE_ID(v);
		VertexOutputForwardAdd_2 o;
		UNITY_INITIALIZE_OUTPUT(VertexOutputForwardAdd_2, o);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

		float4 posWorld = mul(unity_ObjectToWorld, v.vertex);

		// Micha changed
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			
			fixed4 disp = tex2Dlod(_NoiseTex, float4(v.vertex.r, v.vertex.g - _Time.y * 0.1 - 0.01 *  _SinTime.y, 0, 0))
						+ tex2Dlod(_NoiseTex, float4(v.vertex.r, v.vertex.g + _Time.x * 0.1 + 0.01 *  _SinTime.z, 0, 0));

			#ifdef _DISPLACEMENTVARIANT_VERTEXNORMAL
				float4 vertex = v.vertex + float4(v.normal, 0) * disp.r * _DispFactor * (1 - _Blend) * 0.7;

			#elif _DISPLACEMENTVARIANT_VERTEXPOSOBJ
				float4 vertex = v.vertex + float4(v.vertex.x, _SinTime.x, v.vertex.z, 0) * disp.r * _DispFactor * (1 - _Blend) * 0.7;

			#elif _DISPLACEMENTVARIANT_VERTEXCOLOR
				float4 vertex = v.vertex + float4((v.color.rgb * 2 - 1), 0) * disp.r * _DispFactor * (1 - _Blend) * 0.7;

			#else 
				float4 vertex = v.vertex;

			#endif
			
			o.pos = UnityObjectToClipPos(vertex);
			o.objPos = vertex.zy + float2(0.1, -0.1);

		#else
			o.pos = UnityObjectToClipPos(v.vertex);

		#endif

		o.tex = TexCoords_2(v);
		o.eyeVec = NormalizePerVertexNormal(posWorld.xyz - _WorldSpaceCameraPos);
		o.posWorld = posWorld.xyz;
		float3 normalWorld = UnityObjectToWorldNormal(v.normal);

		#ifdef _TANGENT_TO_WORLD
			float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);

			float3x3 tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
			o.tangentToWorldAndLightDir[0].xyz = tangentToWorld[0];
			o.tangentToWorldAndLightDir[1].xyz = tangentToWorld[1];
			o.tangentToWorldAndLightDir[2].xyz = tangentToWorld[2];

		#else
			o.tangentToWorldAndLightDir[0].xyz = 0;
			o.tangentToWorldAndLightDir[1].xyz = 0;
			o.tangentToWorldAndLightDir[2].xyz = normalWorld;

		#endif
		
		//We need this for shadow receiving
		UNITY_TRANSFER_SHADOW(o, v.uv1);

		float3 lightDir = _WorldSpaceLightPos0.xyz - posWorld.xyz * _WorldSpaceLightPos0.w;

		#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = NormalizePerVertexNormal(lightDir);
		#endif

		o.tangentToWorldAndLightDir[0].w = lightDir.x;
		o.tangentToWorldAndLightDir[1].w = lightDir.y;
		o.tangentToWorldAndLightDir[2].w = lightDir.z;

		#ifdef _PARALLAXMAP
			TANGENT_SPACE_ROTATION;
			o.viewDirForParallax = mul(rotation, ObjSpaceViewDir(v.vertex));
		#endif

		// Micha changed
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			o.normal = UnityObjectToWorldNormal(v.normal);
			o.viewDir = UnityWorldSpaceViewDir(mul(unity_ObjectToWorld, v.vertex));
		#endif

		UNITY_TRANSFER_FOG(o, o.pos);
		return o;
	}

	half4 fragForwardAddInternal_2(VertexOutputForwardAdd_2 i) : SV_Target
	{
		UNITY_APPLY_DITHER_CROSSFADE(i.pos.xy);

		FRAGMENT_SETUP_FWDADD(s)

		UNITY_LIGHT_ATTENUATION(atten, i, s.posWorld)
		UnityLight light = AdditiveLight(IN_LIGHTDIR_FWDADD(i), atten);
		UnityIndirect noIndirect = ZeroIndirect();

		half4 c = UNITY_BRDF_PBS(s.diffColor, s.specColor, s.oneMinusReflectivity, s.smoothness, s.normalWorld, -s.eyeVec, light, noIndirect);

		// ***** Micha changed ******
		#if _ALPHABLEND_ON || _ALPHAPREMULTIPLY_ON || _ALPHATEST_ON
			
			half fresnel = 1 - saturate(dot(normalize(i.normal), normalize(i.viewDir)));
			fresnel = pow(fresnel, _FresnelExponent);
			
			half alpha = max(_MinAlpha, fresnel * s.alpha);
			c.rgb = lerp(lerp(_Color.rgb, c.rgb, (1 - (fresnel * s.alpha)) * _ColorBlend), c.rgb, _Blend);
			
			float alphaFactor = (tex2D(_NoiseTex, ((i.objPos - float2(0.3, 0.3) * _SinTime.y * 0.5)) * _NoiseScale).r
								+ tex2D(_NoiseTex, ((i.objPos + float2(0.25, 0.2) * _SinTime.z * 0.2)) * _NoiseScale).r) * _NoiseAlphpaFactor;

			s.alpha = lerp(alpha * alphaFactor, 1, _Blend);
		#endif
		// ***********************************

		UNITY_APPLY_FOG_COLOR(i.fogCoord, c.rgb, half4(0, 0, 0, 0)); // fog towards black in additive pass
		return OutputForward(c, s.alpha);
	}