using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace ComputerGraphicsFifth.Sources
{
    class Shader
    {
        private readonly int _program = 0;
        public string type = "default";
        public Shader(string shaderType, string vertexPath, string fragmentPath)
        {
            type = shaderType;
            int vertexID = CreateShader(ShaderType.VertexShader, vertexPath);
            int fragmentID = CreateShader(ShaderType.FragmentShader, fragmentPath);

            _program = GL.CreateProgram();
            GL.AttachShader(_program, vertexID);
            GL.AttachShader(_program, fragmentID);

            GL.LinkProgram(_program);
            GL.GetProgram(_program, GetProgramParameterName.LinkStatus, out int code);
            if (code != (int)All.True)
                Console.WriteLine(GL.GetProgramInfoLog(_program));
            GL.DeleteShader(vertexID);
            GL.DeleteShader(fragmentID);
        }

        private int CreateShader(ShaderType type, string path)
        {
            string source = File.ReadAllText(path);
            int shaderID = GL.CreateShader(type);
            GL.ShaderSource(shaderID, source);

            GL.CompileShader(shaderID);
            GL.GetShader(shaderID, ShaderParameter.CompileStatus, out int code);
            if (code != (int)All.True)
                Console.WriteLine(GL.GetShaderInfoLog(shaderID));

            return shaderID;
        }

        public int GetProgram() => _program;

        public void UseProgram() => GL.UseProgram(_program);

        public void DeactivateProgram() => GL.UseProgram(0);

        public int GetAttribLocation(string name) => GL.GetAttribLocation(_program, name);

        public int GetUniformLocation(string name) => GL.GetUniformLocation(_program, name);

        public void SetTime(float time, bool isMoving, bool isLightChanging)
        {
            if (isMoving)
                GL.Uniform1(GL.GetUniformLocation(_program, "deformationTime"), time);
            if (isLightChanging)
                GL.Uniform1(GL.GetUniformLocation(_program, "lightTime"), time);
        }
    }
}
