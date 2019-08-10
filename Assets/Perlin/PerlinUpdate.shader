Shader "Perlin/Update"
{
    Properties
    { 
         _Factor("Factor",float) = 0.05
         _Speed("Speed", float) = 0.5
    }

    CGINCLUDE

    #include "UnityCustomRenderTexture.cginc"
    #include "ClassicNoise2D.cginc"

    float _Factor, _Speed;

    half4 frag(v2f_customrendertexture i) : SV_Target
    {
        float2 uv = i.globalTexcoord;
        half value = cnoise(uv* _Factor+_Time.x * _Speed);

        return half4(value, value, 0, 0);
    }

    ENDCG


    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            Name "Update"
            CGPROGRAM
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            ENDCG
        }
    }
}
