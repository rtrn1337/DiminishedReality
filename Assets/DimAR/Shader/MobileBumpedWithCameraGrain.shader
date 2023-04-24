Shader "Custom/MobileBumpedWithCameraGrain"
{
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_NoiseTex("Noise Texture", 3D) = "white" {}
    _NoiseIntensity ("Depth", Range(0,1)) = 0.0
    _NoiseSpeed("Noise Speed", VECTOR) = (30.0, 20.0, 0, 0)
}

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 250

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;
sampler2D _BumpMap;

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
	float4 noiseColor = tex3D(_NoiseTex, nuv3d);
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex); 
	o.Albedo =  lerp(c.rgb, noiseColor, 0.25);;
	o.Alpha = c.a;
	o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}
ENDCG  
}

FallBack "Mobile/Diffuse"
}