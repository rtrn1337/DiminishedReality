Shader "Custom/MobileSpecBumpedWithCameraGrain"
{Properties {
		_Color ("Shadow Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_NoiseTex("Noise Texture", 3D) = "white" {}
	_NoiseIntensity ("Depth", Range(0,1)) = 0.0
	_NoiseSpeed("Noise Speed", VECTOR) = (30.0, 20.0, 0, 0)
}
SubShader { 
	Tags { "RenderType"="Opaque" }
	LOD 250
	
CGPROGRAM
#pragma surface surf MobileBlinnPhong exclude_path:prepass nolightmap noforwardadd halfasview novertexlights

inline fixed4 LightingMobileBlinnPhong (SurfaceOutput s, fixed3 lightDir, fixed3 halfDir, fixed atten)
{
	fixed diff = max (0, dot (s.Normal, lightDir));
	fixed nh = max (0, dot (s.Normal, halfDir));
	fixed spec = pow (nh, s.Specular*128) * s.Gloss;
	
	fixed4 c;
	c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten*2);
	c.a = 0.0;
	return c;
}

sampler2D _MainTex;
sampler2D _BumpMap;
half _Shininess;
float4 _Color;

sampler3D _NoiseTex;
float4 _NoiseTex_ST;

float _NoiseIntensity;
float4 _NoiseSpeed;

struct Input {
	float2 uv_MainTex;
	float4 screenPos : TEXCOORD1;
};

void surf (Input IN, inout SurfaceOutput o) {

	float scale = 3;
	float2 nuv = scale * (IN.screenPos.xy/IN.screenPos.z);
	nuv.xy += float2(sin(_Time.y * _NoiseSpeed.x), cos(_Time.y * _NoiseSpeed.y));
	float3 nuv3d = float3(nuv, _NoiseIntensity);
	
	fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
    float4 noiseColor = tex3D(_NoiseTex, nuv3d);
	float3 mainColor =tex.rgb*_Color;
	o.Albedo =lerp(mainColor, noiseColor, 0.25);
	o.Gloss = tex.a;
	o.Alpha = tex.a;
	o.Specular = _Shininess;
	o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG
}

FallBack "Mobile/VertexLit"
}