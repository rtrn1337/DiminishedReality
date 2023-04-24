Shader "Custom/DiminishedLidarShader"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1) 
        _MainTex ("CameraInput", 2D) = "white" {}
        _NoiseTex("Noise Texture", 3D) = "white" {}
        _NoiseIntensity ("Depth", Range(0,1)) = 0.0
        _NoiseSpeed("Noise Speed", VECTOR) = (30.0, 20.0, 0, 0)
    }
    
    SubShader
    {
        //GrabPass { "_GrabTexture" }
        Pass
        {
            Offset 1, 1
            Tags
            {
                "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="ForwardBase"
            }
                  ZWrite Off
          Cull Back 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
 
            #include "UnityCG.cginc"
            float4x4 _ProjectionMatrix;
            
            struct v2f
            {
                half4 pos : SV_POSITION;
                half4 uv : TEXCOORD0; 
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            //Camera Grain Noise
            sampler3D _NoiseTex;
            float4 _NoiseTex_ST;

            float _NoiseIntensity;
            float4 _NoiseSpeed;
            half4 _Color;
            float _Alpha;
            v2f vert(appdata_base v)
            {
                v2f o;
            
                o.pos = UnityObjectToClipPos(v.vertex);
                //Multiply frame based ProjectionMatrix to uv
                float4 projClipPos = mul(_ProjectionMatrix, float4(v.vertex.xyz, 1.0));
                o.uv = ComputeScreenPos(projClipPos);
               //Noise Grain pos
                o.screenPos = ComputeScreenPos(o.pos);
              
                return o;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float scale = 3;
                float2 nuv = scale * (i.screenPos.xy/i.screenPos.w);
                nuv.xy += float2(sin(_Time.y * _NoiseSpeed.x), cos(_Time.y * _NoiseSpeed.y));
                float3 nuv3d = float3(nuv, _NoiseIntensity);
                float4 mainColor = tex2Dproj(_MainTex, i.uv);
             
                //Noise Color
                float4 noiseColor = tex3D(_NoiseTex, nuv3d);
                //final Color main lerp to Noise
                float4 finalColor = lerp(mainColor, noiseColor, 0.1); 
                return finalColor;
            }
            ENDCG
        } 
    }
    FallBack "Diffuse"
}