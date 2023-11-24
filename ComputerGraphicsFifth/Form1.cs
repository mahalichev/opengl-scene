using System;
using System.Windows.Forms;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

using ComputerGraphicsFifth.Sources;

namespace ComputerGraphicsFifth
{
    struct Kettle
    {
        public Model Container;
        public Model CoffeeFoam;
        public Model ContainerBottom;
        public Model Lid;
        public Model[] Legs;
        public Model[] ReversedLegs;
        public Model[] Circles;
        public Model[] ReversedCircles;
        public Model[] Sticks;
        public Model[] ReversedSticks;
    };

    struct Cup
    {
        public Model Container;
        public Model Handle;
        public Model CoffeeFoam;
    };

    struct Spoon
    {
        public Model Container;
        public Model Stick;
    }


    public partial class Form1 : Form
    {


        private const string _vertexPath = "../../Sources/Shaders/vertex.vert";
        private const string _fragmentPath = "../../Sources/Shaders/fragment.frag";
        private const string _reversedPath = "../../Sources/Shaders/reversed.vert";
        private const string _pointPath = "../../Sources/Shaders/point.frag";
        private const string _directionalPath = "../../Sources/Shaders/directional.frag";
        private const string _spotlightPath = "../../Sources/Shaders/spotlight.frag";

        private Kettle _kettle;
        private Cup _cup;
        private Spoon[] _spoon;
        private Model _floor;
        private Model[] _plate;
        private ModelBuilder _modelBuilder;

        private Model _illuminant;
        private Light _light;

        private Shader _defaultShader;
        private Shader _lightingShader;
        private Shader _reversedShader;

        private Camera _camera;
        private float _width, _height, _FOV = 45f;
        private bool _mouse_captured = false;

        // For tests
        private int _VAO;

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
        }

        private void SetNormals()
        {
            _modelBuilder.UpdateNormalCoords();

            _VAO = GL.GenVertexArray();
            int _VBO = GL.GenBuffer();

            GL.BindVertexArray(_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _modelBuilder.GetNormalCoords().Length * sizeof(float), _modelBuilder.GetNormalCoords(), BufferUsageHint.StaticDraw);

            int vertexCoordLocation = _lightingShader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(vertexCoordLocation);
            GL.VertexAttribPointer(vertexCoordLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            int normalCoordLocation = _lightingShader.GetAttribLocation("normal");
            GL.EnableVertexAttribArray(normalCoordLocation);
            GL.VertexAttribPointer(normalCoordLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
        }

        private void DrawNormals()
        {
            GL.BindVertexArray(_VAO);
            GL.DrawArrays(BeginMode.Lines, 0, _modelBuilder.GetNormalCoords().Length / 3);
            GL.BindVertexArray(0);
        }

        private void Draw()
        {
            if (_lightingShader.type != "directional")
            {
                _defaultShader.UseProgram();
                _camera.SetModel(ref _defaultShader, new Vector3(0, 0, 0));
                _camera.SetProjection(ref _defaultShader, _FOV, 0.7f, 100.0f);
                _camera.SetView(ref _defaultShader);
                DrawIlluminant(ref _illuminant, ref _defaultShader, ref _light);
                //DrawNormals();
                _defaultShader.DeactivateProgram();
            }

            _lightingShader.UseProgram();
            _light.Update(ref _lightingShader);
            _camera.SetPosition(ref _lightingShader);
            _camera.SetModel(ref _lightingShader, new Vector3(0, 0, 0));
            _camera.SetProjection(ref _lightingShader, _FOV, 0.7f, 100.0f);
            _camera.SetView(ref _lightingShader);
            GL.Uniform3(GL.GetUniformLocation(_lightingShader.GetProgram(), "lightPos"), _light.position);
            DrawCup(ref _cup, ref _lightingShader, BeginMode.Quads);
            for (var i = 0; i < 2; i++)
                DrawPlate(ref _plate[i], ref _lightingShader, BeginMode.Quads);
            for (var i = 0; i < 2; i++)
                DrawSpoon(ref _spoon[i], ref _lightingShader, BeginMode.Quads);
            DrawKettle(ref _kettle, ref _lightingShader, BeginMode.Quads);
            _lightingShader.DeactivateProgram();

            _reversedShader.UseProgram();
            _light.Update(ref _lightingShader);
            _camera.SetPosition(ref _lightingShader);
            _camera.SetModel(ref _reversedShader, new Vector3(0, 0, 0));
            _camera.SetProjection(ref _reversedShader, _FOV, 0.7f, 100.0f);
            _camera.SetView(ref _reversedShader);
            GL.Uniform3(GL.GetUniformLocation(_reversedShader.GetProgram(), "lightPos"), _light.position);
            DrawCup(ref _cup, ref _reversedShader, BeginMode.Quads);
            for (var i = 0; i < 2; i++)
                DrawPlate(ref _plate[i], ref _reversedShader, BeginMode.Quads);
            for (var i = 0; i < 2; i++)
                DrawSpoon(ref _spoon[i], ref _reversedShader, BeginMode.Quads);
            DrawKettle(ref _kettle, ref _reversedShader, BeginMode.Quads);
            DrawFloor(ref _floor, ref _lightingShader, BeginMode.Quads);
            _reversedShader.DeactivateProgram();
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.ClearColor(0f, 0f, 0f, 1);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushMatrix();
                Draw();
            GL.PopMatrix();

            glControl1.SwapBuffers();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            _width = (float)glControl1.Width;
            _height = (float)glControl1.Height;
            _camera = new Camera(_width, _height, new Vector3(0f, 1.0f, 5.0f));
            _light = new Light();

            comboBox1.SelectedIndex = 1;
            comboBox2.SelectedIndex = _camera.projectionType;
            comboBox3.SelectedIndex = 2;
            trackBar6.Value = (int)_camera.position.X * 10;
            trackBar5.Value = (int)_camera.position.Y * 10;
            trackBar4.Value = (int)_camera.position.Z * 10;
            trackBar8.Value = (int)_camera.yaw;
            trackBar7.Value = (int)_camera.pitch;
            trackBar9.Value = (int)_FOV * 10;

            trackBar1.Value = (int)_light.position.X * 10;
            trackBar2.Value = (int)_light.position.Y * 10;
            trackBar3.Value = (int)_light.position.Z * 10;
            trackBar16.Value = (int)_light.direction.X * 100;
            trackBar15.Value = (int)_light.direction.Y * 100;
            trackBar14.Value = (int)_light.direction.Z * 100;
            trackBar12.Value = (int)_light.color.X * 255;
            trackBar11.Value = (int)_light.color.Y * 255;
            trackBar10.Value = (int)_light.color.Z * 255;
            trackBar13.Value = (int)_light.intensity.X * 100;
            trackBar18.Value = (int)_light.innerAngle * 10;
            trackBar17.Value = (int)_light.outerAngle * 10;

            GL.Viewport(0, 0, (int)_width, (int)_height);

            GL.Enable(EnableCap.Texture2D);

            _defaultShader = new Shader("default", _vertexPath, _fragmentPath);
            _lightingShader = new Shader("directional", _vertexPath, _directionalPath);
            _reversedShader = new Shader("directional", _reversedPath, _directionalPath);

            _modelBuilder = new ModelBuilder();
            _kettle = new Kettle();
            _cup = new Cup();
            _plate = new Model[2];
            for (var i = 0; i < 2; i++)
                _plate[i] = new Model(1, TextureUnit.Texture1);
            _spoon = new Spoon[2];
            for (var i = 0; i < 2; i++)
                _spoon[i] = new Spoon();

            _modelBuilder.CalculateIlluminant(ref _illuminant, ref _light.position, ref _lightingShader);
            _modelBuilder.CalculateFloor(ref _floor, ref _lightingShader);

            _modelBuilder.CalculateKettle(ref _kettle, ref _lightingShader);
            _modelBuilder.CalculateCup(ref _cup, ref _lightingShader);
            _modelBuilder.CalculatePlate(ref _plate[0], ref _lightingShader);
            _modelBuilder.CalculateReversedPlate(ref _plate[1], ref _lightingShader);
            _modelBuilder.CalculateSpoon(ref _spoon[0], ref _lightingShader);
            _modelBuilder.CalculateReversedSpoon(ref _spoon[1], ref _lightingShader);

            for (int i = 0; i < comboBox4.Items.Count; i++)
                if (comboBox4.Items[i].ToString() == _cup.Container.type) comboBox4.SelectedIndex = i;

            for (int i = 0; i < comboBox5.Items.Count; i++)
                if (comboBox5.Items[i].ToString() == _kettle.Lid.type) comboBox5.SelectedIndex = i;

            // For testing normals
            //SetNormals();

            GL.Enable(EnableCap.DepthTest);
        }

        private void DrawFloor(ref Model floor, ref Shader shader, BeginMode mode)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.SrcColor);
            floor.Draw(ref shader, mode);
            GL.Disable(EnableCap.Blend);
        }

        private void DrawKettle(ref Kettle kettle, ref Shader shader, BeginMode mode) {
            kettle.ContainerBottom.Draw(ref shader, mode);
            kettle.CoffeeFoam.Draw(ref shader, mode);
            kettle.Lid.Draw(ref shader, mode);
            foreach (Model circle in kettle.Circles)
                circle.Draw(ref shader, mode);
            foreach (Model circle in kettle.ReversedCircles)
                circle.Draw(ref shader, mode);
            foreach (Model stick in kettle.Sticks)
                stick.Draw(ref shader, mode);
            foreach (Model stick in kettle.ReversedSticks)
                stick.Draw(ref shader, mode);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            kettle.Container.Draw(ref shader, mode);
            GL.Disable(EnableCap.Blend);
            foreach (Model leg in kettle.Legs)
                leg.Draw(ref shader, mode);
            foreach (Model leg in kettle.ReversedLegs)
                leg.Draw(ref shader, mode);
        }

        private void DrawCup(ref Cup cup, ref Shader shader, BeginMode mode)
        {
            cup.Container.Draw(ref shader, mode);
            cup.Handle.Draw(ref shader, mode);
            cup.CoffeeFoam.Draw(ref shader, mode);
        }

        private void DrawPlate(ref Model plate, ref Shader shader, BeginMode mode) {
            plate.Draw(ref shader, mode);
        }

        private void DrawSpoon(ref Spoon spoon, ref Shader shader, BeginMode mode)
        {
            spoon.Container.Draw(ref shader, mode);
            spoon.Stick.Draw(ref shader, mode);
        }

        private void DrawIlluminant(ref Model illuminant, ref Shader shader, ref Light light)
        {
            int id = GL.GetUniformLocation(shader.GetProgram(), "lightColor");
            GL.Uniform3(id, _light.color);
            id = GL.GetUniformLocation(shader.GetProgram(), "lightIntensity");
            GL.Uniform3(id, _light.intensity);
            illuminant.Draw(ref shader, BeginMode.Quads);
        }

        private void MouseWheelEvent(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _FOV -= e.Delta * 0.01f;
            if (_FOV >= 45.0f)
                _FOV = 45.0f;
            if (_FOV <= 1.0f)
                _FOV = 1.0f;
            trackBar9.Value = (int)_FOV * 10;
            glControl1.Invalidate(); 
        }

        private void glControl1_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_mouse_captured)
                _camera.MouseMove(ref e);
            trackBar8.Value = (int)_camera.yaw - 360 * (int)(_camera.yaw / 180);
            trackBar7.Value = (int)_camera.pitch;
            glControl1.Invalidate();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            _light.position = new Vector3((float)trackBar1.Value / 10, _light.position.Y, _light.position.Z);
            _modelBuilder.CalculateCube(ref _illuminant, 0.2f, ref _light.position, ref _defaultShader);
            glControl1.Invalidate();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            _light.position = new Vector3(_light.position.X, (float)trackBar2.Value / 10, _light.position.Z);
            _modelBuilder.CalculateCube(ref _illuminant, 0.2f, ref _light.position, ref _defaultShader);
            glControl1.Invalidate();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            _light.position = new Vector3(_light.position.X, _light.position.Y, (float)trackBar3.Value / 10);
            _modelBuilder.CalculateCube(ref _illuminant, 0.2f, ref _light.position, ref _defaultShader);
            glControl1.Invalidate();
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            _camera.position = new Vector3((float)trackBar6.Value / 10, (float)trackBar5.Value / 10, (float)trackBar4.Value / 10);
            glControl1.Invalidate();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            _camera.position = new Vector3((float)trackBar6.Value / 10, (float)trackBar5.Value / 10, (float)trackBar4.Value / 10);
            glControl1.Invalidate();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            _camera.position = new Vector3((float)trackBar6.Value / 10, (float)trackBar5.Value / 10, (float)trackBar4.Value / 10);
            glControl1.Invalidate();
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            _camera.yaw = trackBar8.Value;
            _camera.UpdateOrientation();
            glControl1.Invalidate();
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            _camera.pitch = trackBar7.Value;
            _camera.UpdateOrientation();
            glControl1.Invalidate();
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            _FOV = (float)trackBar9.Value / 10;
            glControl1.Invalidate();
        }

        private void trackBar13_Scroll(object sender, EventArgs e)
        {
            _light.intensity = new Vector3((float)trackBar13.Value / 100f, (float)trackBar13.Value / 100f, (float)trackBar13.Value / 100f);
            glControl1.Invalidate();
        }

        private void trackBar12_Scroll(object sender, EventArgs e)
        {
            _light.color = new Vector3((float)trackBar12.Value / 255f, _light.color.Y, _light.color.Z);
            glControl1.Invalidate();
        }

        private void trackBar11_Scroll(object sender, EventArgs e)
        {
            _light.color = new Vector3(_light.color.X, (float)trackBar11.Value / 255f, _light.color.Z);
            glControl1.Invalidate();
        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            _light.color = new Vector3(_light.color.X, _light.color.Y, (float)trackBar10.Value / 255f);
            glControl1.Invalidate();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    _lightingShader = new Shader("point", _vertexPath, _pointPath);
                    _reversedShader = new Shader("point", _reversedPath, _pointPath);
                    break;
                case 1:
                    _lightingShader = new Shader("directional", _vertexPath, _directionalPath);
                    _reversedShader = new Shader("directional", _reversedPath, _directionalPath);
                    break;
                case 2:
                    _lightingShader = new Shader("spotlight", _vertexPath, _spotlightPath);
                    _reversedShader = new Shader("spotlight", _reversedPath, _spotlightPath);
                    break;
            }
            glControl1.Invalidate();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            _camera.projectionType = comboBox2.SelectedIndex;
            glControl1.Invalidate();
        }

        private void trackBar16_Scroll(object sender, EventArgs e)
        {
            _light.direction = new Vector3((float)trackBar16.Value / 100f, _light.direction.Y, _light.direction.Z);
            glControl1.Invalidate();
        }

        private void trackBar15_Scroll(object sender, EventArgs e)
        {
            _light.direction = new Vector3(_light.direction.X, (float)trackBar15.Value / 100f, _light.direction.Z);
            glControl1.Invalidate();
        }

        private void trackBar14_Scroll(object sender, EventArgs e)
        {
            _light.direction = new Vector3(_light.direction.X, _light.direction.Y, (float)trackBar14.Value / 100f);
            glControl1.Invalidate();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox3.SelectedIndex)
            {
                case 0:
                    _light.SetConstants(0.7f, 1.8f);
                    break;
                case 1:
                    _light.SetConstants(0.35f, 0.44f);
                    break;
                case 2:
                    _light.SetConstants(0.22f, 0.20f);
                    break;
                case 3:
                    _light.SetConstants(0.14f, 0.07f);
                    break;
                case 4:
                    _light.SetConstants(0.09f, 0.032f);
                    break;
                case 5:
                    _light.SetConstants(0.07f, 0.017f);
                    break;
                case 6:
                    _light.SetConstants(0.045f, 0.0075f);
                    break;
                case 7:
                    _light.SetConstants(0.027f, 0.0028f);
                    break;
                case 8:
                    _light.SetConstants(0.022f, 0.0019f);
                    break;
                case 9:
                    _light.SetConstants(0.014f, 0.0007f);
                    break;
                case 10:
                    _light.SetConstants(0.007f, 0.0002f);
                    break;
                case 11:
                    _light.SetConstants(0.0014f, 0.000007f);
                    break;
            }
            glControl1.Invalidate();
        }

        private void trackBar18_Scroll(object sender, EventArgs e)
        {
            if (trackBar18.Value >= trackBar17.Value)
                trackBar18.Value = trackBar17.Value - 1;
            _light.innerAngle = (float)trackBar18.Value / 10f;
            glControl1.Invalidate();
        }

        private void trackBar17_Scroll(object sender, EventArgs e)
        {
            if (trackBar17.Value <= trackBar18.Value)
                trackBar17.Value = trackBar18.Value + 1;
            _light.outerAngle = (float)trackBar17.Value / 10f;
            glControl1.Invalidate();
        }

        private void glControl1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _mouse_captured = true;
        }

        private void glControl1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            _mouse_captured = false;
            _camera.firstMove = true;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            _cup.Container.UpdateMaterial(comboBox4.Text);
            _cup.Handle.UpdateMaterial(comboBox4.Text);
            for (int i = 0; i < 2; i++)
                _plate[i].UpdateMaterial(comboBox4.Text);
            glControl1.Invalidate();
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            _kettle.Lid.UpdateMaterial(comboBox5.Text);
            for (int i = 0; i < 4; i++)
            {
                _kettle.Sticks[i].UpdateMaterial(comboBox5.Text);
                _kettle.ReversedSticks[i].UpdateMaterial(comboBox5.Text);
                _kettle.Legs[i].UpdateMaterial(comboBox5.Text);
                _kettle.ReversedLegs[i].UpdateMaterial(comboBox5.Text);
            }
            for (int i = 0; i < 2; i++)
            {
                _kettle.Circles[i].UpdateMaterial(comboBox5.Text);
                _kettle.ReversedCircles[i].UpdateMaterial(comboBox5.Text);
                _spoon[i].Container.UpdateMaterial(comboBox5.Text);
                _spoon[i].Stick.UpdateMaterial(comboBox5.Text);
            }
            glControl1.Invalidate();
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            _camera.KeyboardMove(ref e);
            if (e.KeyCode == Keys.W)
                trackBar4.Value -= trackBar4.Value > trackBar4.Minimum ? 1 : 0;
            if (e.KeyCode == Keys.S)
                trackBar4.Value += trackBar4.Value < trackBar4.Maximum ? 1 : 0;
            if (e.KeyCode == Keys.A)
                trackBar6.Value -= trackBar6.Value > trackBar6.Minimum ? 1 : 0;
            if (e.KeyCode == Keys.D)
                trackBar6.Value += trackBar6.Value < trackBar6.Maximum ? 1 : 0;
            if (e.KeyCode == Keys.Space)
                trackBar5.Value += trackBar5.Value < trackBar5.Maximum ? 1 : 0;
            if (e.KeyCode == Keys.C)
                trackBar5.Value -= trackBar5.Value > trackBar5.Minimum ? 1 : 0;
            glControl1.Invalidate();
        }
    }
}