Shader "Custom/TopMaterial"
{
	Properties{
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB)", 2D) = "white"
	}
		SubShader{
			Tags { "Queue" = "Overlay+1" }
			ZTest Always
			Pass {
				SetTexture[_MainTex] {
					constantColor[_Color]
					Combine texture * constant
				}
			}
	}
}
