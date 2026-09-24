Shader "FullScreen/FovDarken"
{
    HLSLINCLUDE

    #pragma vertex Vert
    #pragma fragment CustomPostProcess
    #pragma target 4.5

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    TEXTURE2D(_FovMask);
    SAMPLER(sampler_FovMask);

    float4 _FovParams; 
    float4 _DarkColor; 
    float4 _VisForward; // xy = направление взгляда (нормализованное)
    float4 _VisAware;   // x = aware radius, y = view radius, z = cos(half angle)

    float4 CustomPostProcess(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 positionWS = GetAbsolutePositionWS(posInput.positionWS);

        if (depth == UNITY_RAW_FAR_CLIP_VALUE) return float4(0, 0, 0, 0);

        float2 delta = positionWS.xz - _FovParams.xy;
        float dist = length(delta);
        
        float2 uv = delta / (_FovParams.z * 2.0) + 0.5;

        float mask = 0;
        if (uv.x >= 0 && uv.x <= 1 && uv.y >= 0 && uv.y <= 1)
        {
            mask = SAMPLE_TEXTURE2D_LOD(_FovMask, sampler_FovMask, uv, 0).r;
        }

        // Вычисляем конус видимости
        float2 dir = delta / max(dist, 0.0001);
        float dotDir = dot(dir, _VisForward.xy);
        
        float awareRadius = _VisAware.x;
        float viewRadius = _VisAware.y;
        float cosAngle = _VisAware.z;

        float inCone = smoothstep(cosAngle - 0.05, cosAngle + 0.05, dotDir);
        float distFade = 1.0 - smoothstep(viewRadius * 0.8, viewRadius, dist);
        float coneVis = inCone * distFade;

        float awareVis = 1.0 - smoothstep(awareRadius * 0.8, awareRadius, dist);
        
        float rangeVis = max(coneVis, awareVis);
        mask = mask * rangeVis;

        float3 finalDark = _DarkColor.rgb;
        
        // Alpha Blending
        float alpha = 1.0 - mask;
        return float4(finalDark, alpha * _DarkColor.a);
    }
    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Fov Darken Pass"
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            ENDHLSL
        }
    }
    Fallback Off
}
