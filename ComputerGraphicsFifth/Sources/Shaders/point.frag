#version 330

in vec3 Normal;
in vec3 crntPos;
in vec2 texCoord;

struct Material {
    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
    float shininess;
}; 

struct Light {
    vec3 position;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;

    float linear;
    float quadratic;
};

out vec4 outColor;

uniform Material material;
uniform Light light;

uniform sampler2D tex0;
uniform vec3 viewPos;

void main(){
    vec3 ambient = light.ambient * material.ambient;

    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(light.position - crntPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = light.diffuse * (diff * material.diffuse);

    vec3 viewDir = normalize(viewPos - crntPos);
    vec3 reflectDir = reflect(-lightDir, norm);  
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess * 128);
    vec3 specular = light.specular * (spec * material.specular);

    float dist = length(light.position - crntPos);
    float attenuation = 1.0 / (1.0 + light.linear * dist + light.quadratic * pow(dist, 2));

    outColor = vec4(ambient * attenuation + diffuse * attenuation, 1.0) * texture(tex0, texCoord) + vec4(specular * attenuation, 0);
};