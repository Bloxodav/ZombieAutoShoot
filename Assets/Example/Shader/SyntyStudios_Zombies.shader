Shader "SyntyStudios/Zombies" {
	Properties {
		_Texture ("Texture", 2D) = "white" {}
		_Blood ("Blood", 2D) = "white" {}
		_BloodColor ("BloodColor", Vector) = (0.6470588,0.2569204,0.2569204,0)
		_BloodAmount ("BloodAmount", Range(0, 1)) = 0
		_Spec ("Spec", Vector) = (0,0,0,0)
		_Smoothness ("Smoothness", Range(0, 1)) = 0
		_Emissive ("Emissive", 2D) = "white" {}
		_EmissiveColor ("Emissive Color", Vector) = (0,0,0,0)
		[HideInInspector] _texcoord2 ("", 2D) = "white" {}
		[HideInInspector] _texcoord ("", 2D) = "white" {}
		[HideInInspector] __dirty ("", Float) = 1
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}