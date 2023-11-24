using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsFifth.Sources
{
    class Camera
    {
        private float _width;
        private float _height;
        public Vector3 position;
        private Vector3 _orientation = new Vector3(0.0f, 0.0f, -1.0f);
        private Vector3 _up = new Vector3(0.0f, 1.0f, 0.0f);
        private float _sensitivity = 0.3f;
        private Vector2 _mouse_position;
        public float yaw = -90.0f;
        public float pitch = 0.0f;
        public bool firstMove = true;
        public int projectionType = 1;

        public Camera(float width, float height, Vector3 pos)
        {
            _width = width;
            _height = height;
            position = pos;
        }

        public void SetModel(ref Shader shader, Vector3 vec)
        {
            Matrix4 model = Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0, 0, 0)), Matrix4.Identity);
            GL.UniformMatrix4(shader.GetUniformLocation("model"), false, ref model);
        }

        public void SetView(ref Shader shader)
        {
            Matrix4 view = Matrix4.LookAt(position, position + _orientation, _up);
            GL.UniformMatrix4(GL.GetUniformLocation(shader.GetProgram(), "view"), false, ref view);
        }

        public void SetProjection(ref Shader shader, float FOVdeg, float nearPlane, float farPlane)
        {
            Matrix4 projection = projectionType == 1 ? 
                Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOVdeg), _width / _height, nearPlane, farPlane)
                : Matrix4.CreateOrthographicOffCenter(-5f, 5f, -5f, 5f, -5f, 5f);
            GL.UniformMatrix4(shader.GetUniformLocation("projection"), false, ref projection);
        }

        public void SetPosition(ref Shader shader)
        {
            GL.Uniform3(shader.GetUniformLocation("viewPos"), ref position);
        }

        public void KeyboardMove(ref KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                position -= new Vector3(0, 0, 0.1f);
            if (e.KeyCode == Keys.S)
                position += new Vector3(0, 0, 0.1f);
            if (e.KeyCode == Keys.A)
                position -= new Vector3(0.1f, 0, 0);
            if (e.KeyCode == Keys.D)
                position += new Vector3(0.1f, 0, 0);
            if (e.KeyCode == Keys.Space)
                position += new Vector3(0, 0.1f, 0);
            if (e.KeyCode == Keys.C)
                position -= new Vector3(0, 0.1f, 0);
        }

        public void MouseMove(ref MouseEventArgs e)
        {
            if (firstMove)
            {
                _mouse_position = new Vector2(e.X, e.Y);
                firstMove = false;
            }
            else
            {
                float deltaX = e.X - _mouse_position.X;
                float deltaY = e.Y - _mouse_position.Y;
                _mouse_position = new Vector2(e.X, e.Y);

                yaw += deltaX * _sensitivity;
                pitch -= deltaY * _sensitivity;
                if (pitch > 89.0f)
                    pitch = 89.0f;
                if (pitch < -89.0f)
                    pitch = -89.0f;
            }
            UpdateOrientation();
        }

        public void UpdateOrientation()
        {
            _orientation.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            _orientation.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            _orientation.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));
            _orientation = Vector3.Normalize(_orientation);
        }
    }
}
