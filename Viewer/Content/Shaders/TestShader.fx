float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor = float4(1, 1, 1, 1);
float AmbientIntensity = 0.4;

float4x4 WorldInverseTranspose;

float3 DiffuseLightDirection = float3(1, 0, 0);
float4 DiffuseColor = float4(1, 1, 1, 1);
float DiffuseIntensity = 1.0;

texture ModelTexture;
sampler2D textureSampler = sampler_state {
    Texture = (ModelTexture);
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float3 Position : SV_POSITION;    
    float3 Normal : NORMAL0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate: TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(float4(input.Position, 1), World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    float3 normal = mul(input.Normal, WorldInverseTranspose);
    float lightIntensity = dot(normal.xyz, DiffuseLightDirection);
    output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);
	output.Color.w = 1;
    output.TextureCoordinate = input.TextureCoordinate;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET0
{
    // float4 textureColor = tex2D(textureSampler, input.TextureCoordinate);
    //if(textureColor.a == 0)
    //       clip(-1);
    //textureColor.a = 0.5f;
    //return textureColor;
     return saturate(input.Color);// + AmbientColor * AmbientIntensity) * float4(input.TextureCoordinate.x, input.TextureCoordinate.y,0,1);
}

technique Diffuse
{
    pass Pass1
    {
        //AlphaBlendEnable = TRUE;
        VertexShader = compile vs_4_0 VertexShaderFunction();
        PixelShader = compile ps_4_0 PixelShaderFunction();
    }
}