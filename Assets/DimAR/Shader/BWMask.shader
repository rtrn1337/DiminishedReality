Shader "Custom/BWMask" {

	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"  }
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION; 
			};
			
         struct fragment_output
            {
                half4 color : SV_Target;
            };

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
		 fragment_output frag (v2f i)
            {
                fragment_output o;
                o.color = half4(1,1,1,1);
                return o;
            }
			ENDCG
		}
	}
}
