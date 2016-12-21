// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/GridShader"
{
	Properties
	{
		_BaseColor ("Color", Color) = (1, 1, 1, 1)
		_Width ("Transparent Width", Range(0.0, 1.0)) = 0.2
	}
	SubShader
	{
		Tags 
		{
			"Queue" = "Transparent"
		}

		Pass
		{
			ZWrite Off
        	Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct vertInput {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};  
			 
			struct vertOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			half4 _BaseColor;
			float _Width;
			
			vertOutput vert(vertInput input) {
				vertOutput o;
				o.pos = mul(UNITY_MATRIX_MVP, input.pos);
				o.uv = input.uv;
				return o;
			}
			 
			half4 frag(vertOutput input) : COLOR {
				if (input.uv.x < _Width || input.uv.x > 1 - _Width || input.uv.y < _Width || input.uv.y > 1 - _Width) {
					return (0, 0, 0, 0);
				} else {
					return _BaseColor;
				}

			}
			ENDCG
		}
	}
}
