#version 330

in vec3 Normal;
in vec2 texCoord;

out vec4 outColor;
uniform sampler2D tex0;
uniform vec3 lightColor;
uniform vec3 lightIntensity;

void main(){
    vec4 coloredTexture = vec4(lightColor, 1.0) * texture(tex0, texCoord);
    outColor = vec4(lightIntensity, 1.0) * coloredTexture;
};