// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#warning Upgrade NOTE: unity_Scale shader variable was removed; replaced 'unity_Scale.w' with '1.0'
// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "VertexLit"
{
	SubShader
	{
		Tags{ "Queue" = "Geometry" "RenderType" = "Opaque" }

		Pass
	{
		Tags{ "LightMode" = "ForwardBase" }

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest


		struct vInput
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 color : COLOR;
	};

	struct vOutput
	{
		float4 pos :SV_POSITION;
		float4 color :TEXCOORD0;
	};


	vOutput vert(vInput v)
	{
		vOutput o;

		o.pos = UnityObjectToClipPos(v.vertex);


		half vertexLitColor = max(0.0, dot(mul((half3x3)unity_ObjectToWorld, v.normal * 1.0), _WorldSpaceLightPos0.xyz));

		o.color = v.color * vertexLitColor;

		return o;
	}

	float4 frag(vOutput i) : COLOR
	{
		return i.color;
	}

		ENDCG

	} //Pass
	} //SubShader
} //Shader