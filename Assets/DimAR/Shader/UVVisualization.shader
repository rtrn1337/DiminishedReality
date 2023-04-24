    Shader "UV Visualization" {
        Properties {
            _MainTex ("Base (RGB)", 2D) = "white" {}
        }
        SubShader {
            Tags { "RenderType"="Opaque" }
            LOD 200
           
            CGPROGRAM
            #pragma surface surf Lambert
     
            sampler2D _MainTex;
     
            struct Input {
                float2 uv_MainTex;
            };
     
            void surf (Input IN, inout SurfaceOutput o) {
                o.Emission = float3(IN.uv_MainTex.rg, 0);
                o.Alpha = 0;
            }
            ENDCG
        }
    }
     
