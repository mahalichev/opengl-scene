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
    vec3 realPos = vec3(model * vec4(position.x, -(position.y + 1.6), position.z, 1.0));
    gl_Position = projection * view * vec4(realPos, 1.0);
    Normal = mat3(transpose(inverse(model))) * vec3(normal);
    texCoord = aTex;
}