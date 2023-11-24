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
    vec3 direction;

    vec3 ambient;
    vec3 diffuse;
    vec3 specular;
};

out vec4 outColor;

uniform Material material;
uniform Light light;
uniform sampler2D tex0;
uniform vec3 viewPos;

void main(){
    // ambient
    vec3 ambient = light.ambient * material.ambient;

    // diffuse 
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(-light.direction);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse  = light.diffuse * diff * material.diffuse;

    // specular
    vec3 viewDir = normalize(viewPos - crntPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), (material.shininess * 128));
    vec3 specular = light.specular * (spec * material.specular);

    outColor = vec4(ambient + diffuse, 1.0) * texture(tex0, texCoord) + vec4(specular * light.diffuse, 0);
};