Shader "Custom/InvisibleShadowCaster"{

SubShader { 
            Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
            UsePass "VertexLit/SHADOWCASTER" } 

FallBack off 

} 