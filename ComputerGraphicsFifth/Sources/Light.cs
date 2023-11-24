using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace ComputerGraphicsFifth.Sources
{
    class Light
    {
        public Vector3 position = new Vector3(1f, 1.5f, 1f);
        public Vector3 color = new Vector3(1f, 1f, 1f);
        public Vector3 intensity = new Vector3(1f, 1f, 1f);
        public Vector3 ambient = new Vector3(0.4f, 0.4f, 0.4f);
        public Vector3 diffuse = new Vector3(1.0f, 1.0f, 1.0f);
        public Vector3 specular = new Vector3(1.0f, 1.0f, 1.0f);
        public Vector3 direction = new Vector3(-1f, -1f, -1f);
        private float _linear = 0.22f;
        private float _quadratic = 0.2f;
        public float innerAngle = 10f;
        public float outerAngle = 15f;

        public void Update(ref Shader shader)
        {
            switch (shader.type)
            {
                case "point":
                    GL.Uniform1(shader.GetUniformLocation("light.linear"), _linear);
                    GL.Uniform1(shader.GetUniformLocation("light.quadratic"), _quadratic);
                    break;
                case "directional":
                    GL.Uniform3(shader.GetUniformLocation("light.direction"), ref direction);
                    break;
                case "spotlight":
                    GL.Uniform3(shader.GetUniformLocation("light.direction"), ref direction);
                    GL.Uniform1(shader.GetUniformLocation("light.innerAngle"), (float)Math.Cos(MathHelper.DegreesToRadians(innerAngle)));
                    GL.Uniform1(shader.GetUniformLocation("light.outerAngle"), (float)Math.Cos(MathHelper.DegreesToRadians(outerAngle)));
                    break;
            }
            GL.Uniform3(shader.GetUniformLocation("light.position"), ref position);
            GL.Uniform3(shader.GetUniformLocation("light.ambient"), ref ambient);
            GL.Uniform3(shader.GetUniformLocation("light.diffuse"), intensity * color);
            GL.Uniform3(shader.GetUniformLocation("light.specular"), ref specular);
        }

        public void SetConstants(float linear, float quadratic)
        {
            _linear = linear;
            _quadratic = quadratic;
        }
    }

}
