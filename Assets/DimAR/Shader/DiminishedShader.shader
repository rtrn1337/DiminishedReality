// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/DiminishedShader"
{
    Properties
    {
        _MainTex ("CameraInput", 2D) = "white" {}
        _NoiseTex("Noise Texture", 3D) = "white" {}
        _NoiseIntensity ("Depth", Range(0,1)) = 0.0
        _NoiseSpeed("Noise Speed", VECTOR) = (30.0, 20.0, 0, 0)  
    }
    
    SubShader
    {
        Pass
        {
            Offset 1, 1
            Tags
            {
                "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="ForwardBase"
            }
            Cull Off
            ZWrite Off
            Ztest LEss
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
                float4 mainColor = tex2Dproj(_MainTex, i.uv);  

                //Noise Calculation
                float scale = 3;
                float2 nuv = scale * (i.screenPos.xy/i.screenPos.w);
                nuv.xy += float2(sin(_Time.y * _NoiseSpeed.x), cos(_Time.y * _NoiseSpeed.y));
                float3 nuv3d = float3(nuv, _NoiseIntensity);
                //Noise Color
                float4 noiseColor = tex3D(_NoiseTex, nuv3d); 
             
                float4  finalColor = lerp(mainColor, noiseColor, 0.1);//
           
                return finalColor;
            }  
            ENDCG
        } 
        }
         
    FallBack "Diffuse"
}