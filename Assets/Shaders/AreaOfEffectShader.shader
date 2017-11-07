// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/AreaOfEffectShader" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_RimValue("Rim value", Range(0, 1)) = 0.5
		_Radius("Radius", Range(0, 100)) = 10
	}
		SubShader{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }

		CGPROGRAM
#pragma surface surf Lambert alpha vertex vert fragment frag

#include "UnityCG.cginc"


			sampler2D _MainTex;
		fixed _RimValue;
		fixed _Radius;


			struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
		};


		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
			float3 worldNormal;
		};

		// Vertex modifier function
		//void vert(inout appdata_full v) {
		//	// Do whatever you want with the "vertex" property of v here
		//	v.vertex.z *= _Radius;
		//}
		v2f vert(appdata v)
		{
			v2f o;
			v.vertex.x += _Radius;
			o.vertex = UnityObjectToClipPos(v.vertex);
			/*o.uv = TRANSFORM_TEX(v.uv, _MainTex);*/
			return o;
		}
		//v2f vert(appdata_base v)
		//{
		//	v2f OUT;
		//	float3 norm = normalize(v.normal); //Unity 5 fix

		//	v.vertex.xyz += norm * _Radius;
		//	OUT.pos = mul(UNITY_MATRIX_MVP, v.vertex);

		//	OUT.normals = v.normal;
		//	return OUT;
		//}

		void surf(Input IN, inout SurfaceOutput o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
			float3 normal = normalize(IN.worldNormal);
			float3 dir = normalize(IN.viewDir);
			float val = 1 - (abs(dot(dir, normal)));
			float rim = val * val * _RimValue;
			o.Alpha = c.a * rim;
		}
		ENDCG
	}
		FallBack "Diffuse"
}