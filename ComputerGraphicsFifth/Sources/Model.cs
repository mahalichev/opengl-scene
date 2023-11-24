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
    class Model
    {
        private int _VBO, _VAO;
        private float[] _vertices;
        private Texture _texture;

        public string type;

        private Vector3 _ambient;
        private Vector3 _diffuse;
        private Vector3 _specular;
        private float _shininess;

        public Model(int index, TextureUnit unit)
        {
            _texture = new Texture(index, unit);
            switch (index)
            {
                case 0:
                    UpdateMaterial("Silver");
                    break;
                case 1:
                    UpdateMaterial("Ceramic");
                    break;
                case 2: // Glass
                    UpdateMaterial("Glass");
                    break;
                case 3: //Glass
                    UpdateMaterial("Glass");
                    break;
                case 4:
                    UpdateMaterial("Coffee");
                    break;
                case 5:
                    UpdateMaterial("Neutral");
                    break;
                case 6:
                    UpdateMaterial("White Rubber");
                    break;
                default:
                    break;
            }
        }

        public void UpdateMaterial(string material)
        {
            type = material;
            switch (material)
            {
                case "Neutral":
                    _ambient = new Vector3(0.2f, 0.2f, 0.2f);
                    _diffuse = new Vector3(0.8f, 0.8f, 0.8f);
                    _specular = new Vector3(0f, 0f, 0f);
                    _shininess = 0f;
                    break;
                case "Silver":
                    _ambient = new Vector3(0.19225f, 0.19225f, 0.19225f);
                    _diffuse = new Vector3(0.50754f, 0.50754f, 0.50754f);
                    _specular = new Vector3(0.508273f, 0.508273f, 0.508273f);
                    _shininess = 0.4f;
                    break;
                case "Pearl":
                    //////
                    break;
                case "Glass":
                    _ambient = new Vector3(0.0f, 0.0f, 0.0f);
                    _diffuse = new Vector3(0.588235f, 0.670588f, 0.729412f);
                    _specular = new Vector3(0.9f, 0.9f, 0.9f);
                    _shininess = 0.75f;
                    break;
                case "Coffee":
                    _ambient = new Vector3(0f, 0f, 0f);
                    _diffuse = new Vector3(1f, 1f, 1f);
                    _specular = new Vector3(0f, 0f, 0f);
                    _shininess = 0.1f;
                    break;
                case "Ceramic":
                case "White Plastic":
                    _ambient = new Vector3(0f, 0f, 0f);
                    _diffuse = new Vector3(0.55f, 0.55f, 0.55f);
                    _specular = new Vector3(0.7f, 0.7f, 0.7f);
                    _shininess = 0.25f;
                    break;
                case "White Rubber":
                    _ambient = new Vector3(0.05f, 0.05f, 0.05f);
                    _diffuse = new Vector3(0.5f, 0.5f, 0.5f);
                    _specular = new Vector3(0.7f, 0.7f, 0.7f);
                    _shininess = 0.078125f;
                    break;
                case "Chrome":
                    _ambient = new Vector3(0.25f, 0.25f, 0.25f);
                    _diffuse = new Vector3(0.4f, 0.4f, 0.4f);
                    _specular = new Vector3(0.774597f, 0.774597f, 0.774597f);
                    _shininess = 0.6f;
                    break;
                case "Copper":
                    _ambient = new Vector3(0.19125f, 0.0735f, 0.0225f);
                    _diffuse = new Vector3(0.7038f, 0.27048f, 0.0828f);
                    _specular = new Vector3(0.256777f, 0.137622f, 0.086014f);
                    _shininess = 0.1f;
                    break;
                case "Polished Copper":
                    _ambient = new Vector3(0.2295f, 0.08825f, 0.0275f);
                    _diffuse = new Vector3(0.5508f, 0.2118f, 0.066f);
                    _specular = new Vector3(0.580594f, 0.223257f, 0.0695701f);
                    _shininess = 0.4f;
                    break;
                case "Gold":
                    _ambient = new Vector3(0.24725f, 0.1995f, 0.0745f);
                    _diffuse = new Vector3(0.75164f, 0.60648f, 0.22648f);
                    _specular = new Vector3(0.628281f, 0.555802f, 0.366065f);
                    _shininess = 0.4f;
                    break;
                case "Polished Gold":
                    _ambient = new Vector3(0.24725f, 0.2245f, 0.0645f);
                    _diffuse = new Vector3(0.34615f, 0.3143f, 0.0903f);
                    _specular = new Vector3(0.797357f, 0.723991f, 0.208006f);
                    _shininess = 0.65f;
                    break;
                case "Polished Silver":
                    _ambient = new Vector3(0.23125f, 0.23125f, 0.23125f);
                    _diffuse = new Vector3(0.2775f, 0.2775f, 0.2775f);
                    _specular = new Vector3(0.773911f, 0.773911f, 0.773911f);
                    _shininess = 0.7f;
                    break;
                case "Black Plastic":
                    _ambient = new Vector3(0f, 0f, 0f);
                    _diffuse = new Vector3(0.01f, 0.01f, 0.01f);
                    _specular = new Vector3(0.5f, 0.5f, 0.5f);
                    _shininess = 0.25f;
                    break;
                case "Brass":
                    _ambient = new Vector3(0.329412f, 0.223529f, 0.027451f);
                    _diffuse = new Vector3(0.780392f, 0.568627f, 0.113725f);
                    _specular = new Vector3(0.992157f, 0.941176f, 0.807843f);
                    _shininess = 0.21794843f;
                    break;
                case "Pewter":
                    _ambient = new Vector3(0.105882f, 0.058824f, 0.113725f);
                    _diffuse = new Vector3(0.427451f, 0.470588f, 0.541176f);
                    _specular = new Vector3(0.333333f, 0.333333f, 0.521569f);
                    _shininess = 0.07692304f;
                    break;
                default:
                    break;
            }
        }

        public void UpdateBuffers(ref Shader shader)
        {
            _VAO = GL.GenVertexArray();
            _VBO = GL.GenBuffer();

            GL.BindVertexArray(_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            int vertexCoordLocation = shader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(vertexCoordLocation);
            GL.VertexAttribPointer(vertexCoordLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            int normalCoordLocation = shader.GetAttribLocation("normal");
            GL.EnableVertexAttribArray(normalCoordLocation);
            GL.VertexAttribPointer(normalCoordLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
            int aTexCoordLocation = shader.GetAttribLocation("aTex");
            GL.EnableVertexAttribArray(aTexCoordLocation);
            GL.VertexAttribPointer(aTexCoordLocation, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        public void SetVertices(ref List<float> vertices)
        {
            _vertices = new float[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
                _vertices[i] = vertices[i];
        }

        public ref float[] GetVertices() => ref _vertices;

        public void Draw(ref Shader shader, BeginMode type)
        {
            if (shader.type != "default")
            {
                GL.Uniform3(shader.GetUniformLocation("material.ambient"), ref _ambient);
                GL.Uniform3(shader.GetUniformLocation("material.diffuse"), ref _diffuse);
                GL.Uniform3(shader.GetUniformLocation("material.specular"), ref _specular);
                GL.Uniform1(shader.GetUniformLocation("material.shininess"), _shininess);
            }

            _texture.Bind();
            GL.BindVertexArray(_VAO);
            GL.DrawArrays(type, 0, _vertices.Length / 8);
            GL.BindVertexArray(0);
            _texture.Unbind();
        }
    }
}
