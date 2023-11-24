#version 330

layout (location = 0) in vec3 position;
layout (location = 1) in vec3 normal;
layout (location = 2) in vec2 aTex;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

out vec3 crntPos;
out vec3 Normal;
out vec2 texCoord;

void main(){
    crntPos = vec3(model * vec4(position, 1.0));
    gl_Position = projection * view * vec4(crntPos, 1.0);
    Normal = mat3(transpose(inverse(model))) * normal;
    texCoord = aTex;
}