/*using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Timers;

using OpenTK.Graphics.OpenGL;
using OpenTK;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace cursovik
{
    public partial class Form1 : Form
    {
        Shader mainShader;
        Shader linesShader;
        Shader lampaShader;

        float lastX = 0, lastY = 0;
        float yaw = -90.0f;
        float pitch = 0.0f;

        bool firstMouse = true;

        bool isMouseClick = false;

        Vector3 cameraPos = new Vector3(1.0f, 1.0f, 3.0f);
        Vector3 cameraFront = new Vector3(2f, 1f, 1.0f);
        Vector3 cameraUp = new Vector3(0.0f, 1.0f, 3.0f);

        bool[] keys = new bool[1024];

        float angleX = 0.0f;
        float angleY = 0.0f;
        float angleZ = 0.0f;

        float translateX = 0.0f;
        float translateY = 0.0f;
        float translateZ = 0.0f;

        float scaleX = 1.0f;
        float scaleY = 1.0f;
        float scaleZ = 1.0f;

        bool showHiddenLines = false;
        bool isPerspective = true;

        Vector3 lightPos = new Vector3(0.5f, 0.5f, 3.0f);
        Vector3 lightIntensity = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);

        Vector3 MatAmbient = new Vector3(0.5f, 0.0f, 0.0f);
        Vector3 MatDiffuse = new Vector3(0.5f, 0.0f, 0.0f);
        Vector3 MatSpecular = new Vector3(0.7f, 0.6f, 0.6f);
        float MatShininess = 0.25f;

        private static System.Timers.Timer timer;
        int time = 0;

        string shaderType = "point";

        const string directedFragmentShaderSource = @"
            #version 330 core
            struct Material {
                vec3 ambient;
                vec3 diffuse;
                vec3 specular;
                float shininess;
            };
            struct Light {
                vec3 direction;
                vec3 ambient;
                vec3 diffuse;
                vec3 specular;
            };

            in vec3 Normal;
            in vec3 FragPos;
            
            out vec4 color;

            uniform vec3 viewPos;

            uniform Material material;
            uniform Light light; 

            void main()
            {
                vec3 ambient  = light.ambient * material.ambient;
                
                vec3 norm = normalize(Normal);
                vec3 lightDir = normalize(-light.direction);
                float diff = max(dot(norm, lightDir), 0.0);
                vec3 diffuse  = light.diffuse * diff * material.diffuse;         

                vec3 viewDir = normalize(viewPos - FragPos);
                vec3 reflectDir = reflect(-lightDir, norm);
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
                vec3 specular = light.specular * (spec * material.specular); 
                
                vec3 result = ambient + diffuse + specular;
                color = vec4(result, 1.0f);
            };";

        const string pointFragmentShaderSource = @"
            #version 330 core
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

                float constant;
                float linear;
                float quadratic;
            };

            in vec3 Normal;
            in vec3 FragPos;
            
            out vec4 color;
               
            uniform vec3 viewPos;

            uniform Material material;
            uniform Light light; 

            void main()
            {
                vec3 ambient  = light.ambient * material.ambient;
                
                vec3 norm = normalize(Normal);
                vec3 lightDir = normalize(light.position - FragPos);
                float diff = max(dot(norm, lightDir), 0.0);
                vec3 diffuse  = light.diffuse * diff * material.diffuse;         

                vec3 viewDir = normalize(viewPos - FragPos);
                vec3 reflectDir = reflect(-lightDir, norm);
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
                vec3 specular = light.specular* spec * material.specular; 

                float distance    = length(light.position - FragPos);
                float attenuation = 1.0 / (light.constant + light.linear * distance + 
    		    light.quadratic * (distance * distance));
                ambient  *= attenuation; 
                diffuse  *= attenuation;
                specular *= attenuation;
                
                vec3 result = ambient + diffuse + specular;
                color = vec4(result, 1.0f);
            };";


        const string spotlightFragmentShaderSource = @"
            #version 330 core
            struct Material {
                vec3 ambient;
                vec3 diffuse;
                vec3 specular;
                float shininess;
            };
            struct Light {
                vec3  position;
                vec3  direction;
                float cutOff;
                float outerCutOff;

                vec3 ambient;
                vec3 diffuse;
                vec3 specular;

            };

            in vec3 Normal;
            in vec3 FragPos;
            
            out vec4 color;
               
            uniform vec3 viewPos;

            uniform Material material;
            uniform Light light; 

            void main()
            {
                vec3 ambient  = light.ambient * material.ambient;
                
                vec3 norm = normalize(Normal);
                vec3 lightDir = normalize(light.position - FragPos);
                float diff = max(dot(norm, lightDir), 0.0);
                vec3 diffuse  = light.diffuse * diff * material.diffuse;         

                vec3 viewDir = normalize(viewPos - FragPos);
                vec3 reflectDir = reflect(-lightDir, norm);
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
                vec3 specular = light.specular * (spec * material.specular); 

                float theta     = dot(lightDir, normalize(-light.direction));
                float epsilon   = light.cutOff - light.outerCutOff;
                float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0);
                
                diffuse *=  intensity;
                specular *= intensity;

                vec3 result = ambient + diffuse + specular;
                color = vec4(result, 1.0f);
            };";


        const string vertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 position;
            layout (location = 1) in vec3 normal;
            out vec3 FragPos;  
            out vec3 Normal;
           
            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            void main()
            {
                gl_Position = projection * view * model * vec4(position, 1.0);
                FragPos = vec3(model * vec4(position, 1.0f));
                Normal = mat3(transpose(inverse(model))) * normal;
            }";

        public Form1()
        {
            InitializeComponent();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {


            // код фрагментного шейдера


            const string defaultVertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 position;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            void main()
            {
                gl_Position = projection * view * vec4(position, 1.0);
            }";


            const string defaultFragmentShaderSource = @"
            #version 330 core
            
            out vec4 color;
            uniform vec3 ourColor;
            void main()
            {
                color = vec4(ourColor, 1.0f);
            };";


            const string lampatVertexShaderSource = @"
            #version 330 core
            layout (location = 0) in vec3 position;
            
            out vec3 outColor;            

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            uniform vec3 ourColor;

            void main()
            {
                gl_Position = projection * view * model * vec4(position, 1.0f);
                outColor = ourColor;
            }";


            const string lampaFragmentShaderSource = @"
            #version 330 core
            in  vec3 outColor;
            out vec4 color;

            void main()
            {
                color = vec4(outColor, 1.0f);;
            }";

            mainShader = new Shader(vertexShaderSource, pointFragmentShaderSource);
            linesShader = new Shader(defaultVertexShaderSource, defaultFragmentShaderSource);
            lampaShader = new Shader(lampatVertexShaderSource, lampaFragmentShaderSource);

            timer = new System.Timers.Timer(100);
            timer.Elapsed += TimeEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            do_movement();
            GL.ClearColor(System.Drawing.Color.Black);
            GL.Enable(EnableCap.DepthTest);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + cameraFront, cameraUp);
            Matrix4 projection;
            if (isPerspective)
                projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), Width / Height, 0.1f, 100.0f);
            else
                projection = Matrix4.CreateOrthographicOffCenter(-5.0f, 5.0f, -5.0f, 5.0f, -5.0f, 5.0f);
            Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(angleX));
            model = Matrix4.Mult(model, Matrix4.CreateRotationY(MathHelper.DegreesToRadians(angleY)));
            model = Matrix4.Mult(model, Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(angleZ)));
            model = Matrix4.Mult(model, Matrix4.CreateTranslation(translateX, translateY, translateZ));
            model = Matrix4.Mult(model, Matrix4.CreateScale(scaleX, scaleY, scaleZ));
            lighting(view, projection);
            drawObject(view, projection, model);
            drawCoordinateLines(view, projection);
            glControl1.SwapBuffers();
        }
        public void lighting(Matrix4 view, Matrix4 projection)
        {
            switch (shaderType)
            {
                case "point":
                    mainShader = new Shader(vertexShaderSource, pointFragmentShaderSource);
                    mainShader.Use();
                    mainShader.SetAttribute("light.position", lightPos);
                    mainShader.SetFloat("light.constant", 1.0f);
                    mainShader.SetFloat("light.linear", 0.22f);
                    mainShader.SetFloat("light.quadratic", 0.2f);
                    drawLampa(view, projection);
                    break;
                case "directed":
                    mainShader = new Shader(vertexShaderSource, directedFragmentShaderSource);
                    mainShader.Use();
                    mainShader.SetAttribute("light.direction", new Vector3(-0.1f, 1.0f, -1.0f));
                    break;
                case "spotlight":
                    mainShader = new Shader(vertexShaderSource, spotlightFragmentShaderSource);
                    mainShader.Use();
                    mainShader.SetAttribute("light.position", cameraPos);
                    mainShader.SetAttribute("light.direction", cameraFront);
                    mainShader.SetFloat("light.cutOff", (float)Math.Cos(MathHelper.DegreesToRadians(12.5)));
                    mainShader.SetFloat("light.outerCutOff", (float)Math.Cos(MathHelper.DegreesToRadians(20.5)));
                    break;
            }
            mainShader.Use();
            mainShader.SetAttribute("light.ambient", new Vector3(0.2f, 0.2f, 0.2f));
            mainShader.SetAttribute("light.diffuse", lightIntensity * lightColor);
            mainShader.SetAttribute("light.specular", new Vector3(1.0f, 1.0f, 1.0f));
        }

        public float[] createRectangle3D(float length, float width, float height)
        {
            float[] verticles = new float[144];
            createRectangle2D(ref verticles, length, width, height, "down", 0);
            createRectangle2D(ref verticles, length, width, height, "up", 24);
            createRectangle2D(ref verticles, length, width, height, "left", 48);
            createRectangle2D(ref verticles, length, width, height, "right", 72);
            createRectangle2D(ref verticles, length, width, height, "font", 96);
            createRectangle2D(ref verticles, length, width, height, "backside", 120);
            return verticles;
        }

        public void createRectangle2D(ref float[] vert, float length, float width, float height, string side, int offset)
        {
            switch (side)
            {
                case "down":
                    createPoint(ref vert, 0, 0, 0, 5, -1.0f, offset);
                    createPoint(ref vert, length, 0, 0, 5, -1.0f, offset + 6);
                    createPoint(ref vert, length, width, 0, 5, -1.0f, offset + 12);
                    createPoint(ref vert, 0, width, 0, 5, -1.0f, offset + 18);
                    break;
                case "up":
                    createPoint(ref vert, 0, 0, height, 5, 1.0f, offset);
                    createPoint(ref vert, length, 0, height, 5, 1.0f, offset + 6);
                    createPoint(ref vert, length, width, height, 5, 1.0f, offset + 12);
                    createPoint(ref vert, 0, width, height, 5, 1.0f, offset + 18);
                    break;
                case "left":
                    createPoint(ref vert, 0, width, 0, 3, -1.0f, offset);
                    createPoint(ref vert, 0, 0, 0, 3, -1.0f, offset + 6);
                    createPoint(ref vert, 0, 0, height, 3, -1.0f, offset + 12);
                    createPoint(ref vert, 0, width, height, 3, -1.0f, offset + 18);
                    break;
                case "right":
                    createPoint(ref vert, length, width, 0, 3, 1.0f, offset);
                    createPoint(ref vert, length, 0, 0, 3, 1.0f, offset + 6);
                    createPoint(ref vert, length, 0, height, 3, 1.0f, offset + 12);
                    createPoint(ref vert, length, width, height, 3, 1.0f, offset + 18);
                    break;
                case "font":
                    createPoint(ref vert, 0, 0, 0, 4, -1.0f, offset);
                    createPoint(ref vert, length, 0, 0, 4, -1.0f, offset + 6);
                    createPoint(ref vert, length, 0, height, 4, -1.0f, offset + 12);
                    createPoint(ref vert, 0, 0, height, 4, -1.0f, offset + 18);
                    break;
                case "backside":
                    createPoint(ref vert, 0, width, 0, 4, 1.0f, offset);
                    createPoint(ref vert, length, width, 0, 4, 1.0f, offset + 6);
                    createPoint(ref vert, length, width, height, 4, 1.0f, offset + 12);
                    createPoint(ref vert, 0, width, height, 4, 1.0f, offset + 18);
                    break;
            }
        }

        public void createPoint(ref float[] vert, float x, float y, float z, int side, float sideValue, int offset)
        {
            vert[offset] = x;
            vert[offset + 1] = y;
            vert[offset + 2] = z;
            vert[offset + side] = sideValue;
        }

        public void drawObject(Matrix4 view, Matrix4 projection, Matrix4 model)
        {
            mainShader.Use();
            mainShader.SetMatrix(view, "view");
            mainShader.SetMatrix(projection, "projection");
            mainShader.SetAttribute("viewPos", cameraPos);
            mainShader.SetAttribute("material.ambient", MatAmbient);
            mainShader.SetAttribute("material.diffuse", MatDiffuse);
            mainShader.SetAttribute("material.specular", MatSpecular);
            mainShader.SetFloat("material.shininess", MatShininess);




            float[] verticalVertices = createRectangle3D(0.25f, 0.25f, 1.25f);
            MatFigure vertical = new MatFigure(mainShader, verticalVertices);
            float[] halfVerticalVertices = createRectangle3D(0.25f, 0.25f, 0.75f);
            MatFigure halfVertical = new MatFigure(mainShader, halfVerticalVertices);
            float[] horizontalVertices = createRectangle3D(0.25f, 1.25f, 0.25f);
            MatFigure horizontal = new MatFigure(mainShader, horizontalVertices);
            float[] squareVertices = createRectangle3D(0.25f, 0.25f, 0.25f);
            MatFigure square = new MatFigure(mainShader, squareVertices);

            // draw H
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.25f, 0.0f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.5f, 0.0f, 0.5f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.75f, 0.0f, 0.0f)), model), "model");
            vertical.draw(mainShader);

            //draw E
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.05f, 0.0f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.3f, 0.0f, 1.0f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.3f, 0.0f, 0.5f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.3f, 0.0f, 0.0f)), model), "model");
            square.draw(mainShader);

            //draw L
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.6f, 0.25f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.6f, 0.5f, 0.0f)), model), "model");
            square.draw(mainShader);

            //draw P
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.6f, 0.8f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.6f, 1.05f, 1.0f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.6f, 1.05f, 0.5f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.6f, 1.3f, 0.5f)), model), "model");
            halfVertical.draw(mainShader);

            // draw H
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.3f, 1.55f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.05f, 1.55f, 0.5f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.8f, 1.55f, 0.0f)), model), "model");
            vertical.draw(mainShader);

            //draw E
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.5f, 1.55f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.25f, 1.55f, 1.0f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.25f, 1.55f, 0.5f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.25f, 1.55f, 0.0f)), model), "model");
            square.draw(mainShader);

            //draw L
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.0f, 1.3f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.0f, 1.05f, 0.0f)), model), "model");
            square.draw(mainShader);

            //draw P
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.0f, 0.75f, 0.0f)), model), "model");
            vertical.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.0f, 0.5f, 1.0f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.0f, 0.5f, 0.5f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.0f, 0.25f, 0.5f)), model), "model");
            halfVertical.draw(mainShader);

            // draw H
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.25f, 0.25f, 1.25f)), model), "model");
            horizontal.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.5f, 0.75f, 1.25f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.75f, 0.25f, 1.25f)), model), "model");
            horizontal.draw(mainShader);

            //draw E
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.05f, 0.25f, 1.25f)), model), "model");
            horizontal.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.3f, 1.25f, 1.25f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.3f, 0.75f, 1.25f)), model), "model");
            square.draw(mainShader);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(1.3f, 0.25f, 1.25f)), model), "model");
            square.draw(mainShader);

            float[] mainSquareVertices = createRectangle3D(1.3f, 1.3f, 1.25f);
            MatFigure mainSquare = new MatFigure(mainShader, mainSquareVertices);
            mainShader.SetMatrix(Matrix4.Mult(Matrix4.CreateTranslation(new Vector3(0.25f, 0.25f, 0.0f)), model), "model");
            mainShader.SetAttribute("material.diffuse", new Vector3(0.85f, 0.83f, 0.82f));
            mainSquare.draw(mainShader);

        }

        public void drawCoordinateLines(Matrix4 view, Matrix4 projection)
        {
            linesShader.Use();
            linesShader.SetMatrix(view, "view");
            linesShader.SetMatrix(projection, "projection");
            linesShader.SetAttribute("ourColor", new Vector3(0.0f, 1.0f, 0.0f));
            Figure lineX = new Figure(linesShader, new float[] { -4.0f, 0.0f, 0.0f, 4.0f, 0.0f, 0.0f });
            lineX.draw(linesShader, BeginMode.Lines);
            linesShader.SetAttribute("ourColor", new Vector3(1.0f, 0.0f, 0.0f));
            Figure lineY = new Figure(linesShader, new float[] { 0.0f, -4.0f, 0.0f, 0.0f, 4.0f, 0.0f });
            lineY.draw(linesShader, BeginMode.Lines);
            linesShader.SetAttribute("ourColor", new Vector3(0.0f, 0.0f, 1.0f));
            Figure lineZ = new Figure(linesShader, new float[] { 0.0f, 0.0f, -4.0f, 0.0f, 0.0f, 4.0f });
            lineZ.draw(linesShader, BeginMode.Lines);
        }


        public void drawLampa(Matrix4 view, Matrix4 projection)
        {
            Matrix4 model = Matrix4.Identity;
            lampaShader.Use();
            lampaShader.SetMatrix(view, "view");
            lampaShader.SetMatrix(projection, "projection");
            lampaShader.SetAttribute("ourColor", lightColor * lightIntensity);

            float[] cubeVert = createRectangle3D(0.15f, 0.15f, 0.15f);

            MatFigure lampa = new MatFigure(lampaShader, cubeVert);
            lampaShader.SetMatrix(Matrix4.CreateTranslation(lightPos), "model");
            lampa.draw(lampaShader);
        }

        private void TimeEvent(Object source, ElapsedEventArgs e)
        {
            glControl1.Invalidate();
            time++;
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}",
                          e.SignalTime);
        }

        private void glControl1_KeyDown(object sender, KeyEventArgs e)
        {
            keys[e.KeyValue] = true;
        }

        private void glControl1_KeyUp(object sender, KeyEventArgs e)
        {
            keys[e.KeyValue] = false;
        }

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseClick = true;
            lastX = e.X;
            lastY = e.Y;
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseClick = false;
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseClick)
                return;
            if (firstMouse)
            {
                lastX = e.X;
                lastY = e.Y;
                firstMouse = false;
            }
            float xoffset = e.X - lastX;
            float yoffset = lastY - e.Y;
            lastX = e.X;
            lastY = e.Y;

            float sensitivity = 0.05f;
            xoffset *= sensitivity;
            yoffset *= sensitivity;

            yaw += xoffset;
            pitch += yoffset;

            if (pitch > 89.0f)
                pitch = 89.0f;
            if (pitch < -89.0f)
                pitch = -89.0f;

            Vector3 front;
            front.X = (float)(Math.Cos(MathHelper.DegreesToRadians(pitch)) * Math.Cos(MathHelper.DegreesToRadians(yaw)));
            front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = (float)(Math.Cos(MathHelper.DegreesToRadians(pitch)) * Math.Sin(MathHelper.DegreesToRadians(yaw)));
            cameraFront = Vector3.Normalize(front);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            lightPos.X = (float)trackBar1.Value / 100;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            lightPos.Y = (float)trackBar2.Value / 100;
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            lightPos.Z = (float)trackBar3.Value / 100;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            float value = (float)trackBar4.Value / 100;
            lightIntensity = new Vector3(value, value, value);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                lightColor.X = colorDialog1.Color.R / 255;
                lightColor.Y = colorDialog1.Color.G / 255;
                lightColor.Z = colorDialog1.Color.B / 255;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            shaderType = comboBox1.Text;
            switch (shaderType)
            {
                case "directed":
                    panel1.Visible = false;
                    break;
                case "point":
                    panel1.Visible = true;
                    break;
                case "spotlight":
                    panel1.Visible = false;
                    break;
            }
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            translateX = (float)trackBar5.Value / 100;
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            translateY = (float)trackBar6.Value / 100;
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            translateZ = (float)trackBar7.Value / 100;
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            angleX = trackBar8.Value;
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            angleY = trackBar9.Value;
        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            angleZ = trackBar10.Value;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            isPerspective = !isPerspective;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox2.Text)
            {
                case "emerald":
                    MatAmbient = new Vector3(0.0215f, 0.1745f, 0.0215f);
                    MatDiffuse = new Vector3(0.07568f, 0.61424f, 0.07568f);
                    MatSpecular = new Vector3(0.633f, 0.727811f, 0.633f);
                    MatShininess = 0.6f;
                    break;
                case "pearl":
                    MatAmbient = new Vector3(0.25f, 0.20725f, 0.20725f);
                    MatDiffuse = new Vector3(1f, 0.829f, 0.829f);
                    MatSpecular = new Vector3(0.296648f, 0.296648f, 0.296648f);
                    MatShininess = 0.088f;
                    break;
                case "ruby":
                    MatAmbient = new Vector3(0.1745f, 0.01175f, 0.01175f);
                    MatDiffuse = new Vector3(0.61424f, 0.04136f, 0.04136f);
                    MatSpecular = new Vector3(0.727811f, 0.626959f, 0.626959f);
                    MatShininess = 0.6f;
                    break;
                case "gold":
                    MatAmbient = new Vector3(0.24725f, 0.1995f, 0.0745f);
                    MatDiffuse = new Vector3(0.75164f, 0.60648f, 0.22648f);
                    MatSpecular = new Vector3(0.628281f, 0.555802f, 0.366065f);
                    MatShininess = 0.4f;
                    break;
                case "bronze":
                    MatAmbient = new Vector3(0.2125f, 0.1275f, 0.054f);
                    MatDiffuse = new Vector3(0.714f, 0.4284f, 0.18144f);
                    MatSpecular = new Vector3(0.393548f, 0.271906f, 0.166721f);
                    MatShininess = 0.2f;
                    break;
                case "cyan plastic":
                    MatAmbient = new Vector3(0.0f, 0.1f, 0.06f);
                    MatDiffuse = new Vector3(0.0f, 0.50980392f, 0.50980392f);
                    MatSpecular = new Vector3(0.50196078f, 0.50196078f, 0.50196078f);
                    MatShininess = 0.25f;
                    break;
                case "red plastic":
                    MatAmbient = new Vector3(0.0f, 0.0f, 0.0f);
                    MatDiffuse = new Vector3(0.5f, 0.0f, 0.0f);
                    MatSpecular = new Vector3(0.7f, 0.6f, 0.6f);
                    MatShininess = 0.25f;
                    break;
                case "green plastic":
                    MatAmbient = new Vector3(0.0f, 0.0f, 0.0f);
                    MatDiffuse = new Vector3(0.1f, 0.35f, 0.1f);
                    MatSpecular = new Vector3(0.45f, 0.55f, 0.45f);
                    MatShininess = 0.25f;
                    break;
            }
        }

        public void do_movement()
        {
            float cameraSpeed = 0.05f;
            if (keys[87])
                cameraPos += cameraSpeed * cameraFront;
            if (keys[83])
                cameraPos -= cameraSpeed * cameraFront;
            if (keys[65])
                cameraPos -= Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed;
            if (keys[68])
                cameraPos += Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed;
            if (keys[32])
                cameraPos += cameraUp * cameraSpeed;
            if (keys[16])
                cameraPos -= cameraUp * cameraSpeed;
        }

    }

    public class Shader
    {
        int Program;
        public Shader(string vertexSource, string fragmentSource)
        {
            int vertex, fragment;
            vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vertexSource);
            GL.CompileShader(vertex);
            string info = GL.GetShaderInfoLog(vertex);
            if (info != null)
                Console.WriteLine(info);
            fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fragmentSource);
            GL.CompileShader(fragment);
            info = GL.GetShaderInfoLog(fragment);
            if (info != null)
                Console.WriteLine(info);
            Program = GL.CreateProgram();
            GL.AttachShader(Program, vertex);
            GL.AttachShader(Program, fragment);
            GL.LinkProgram(Program);
            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }

        public void Use() { GL.UseProgram(Program); }

        public int GetAttribLocation(string name)
        {
            return GL.GetAttribLocation(Program, name);
        }

        public void SetAttribute(string name, Vector3 value)
        {
            int handle = GL.GetUniformLocation(Program, name);
            GL.Uniform3(handle, value);
        }

        public void SetFloat(string name, float value)
        {
            int handle = GL.GetUniformLocation(Program, name);
            GL.Uniform1(handle, value);
        }

        public void SetInt(string name, int value)
        {
            int handle = GL.GetUniformLocation(Program, name);
            GL.Uniform1(handle, value);
        }

        public void SetMatrix(Matrix4 matrix, string name)
        {
            int modelLoc = GL.GetUniformLocation(Program, name);
            GL.UniformMatrix4(modelLoc, false, ref matrix);
        }

    };


    public class Texture
    {
        int Handle;
        public Texture(string path)
        {
            Handle = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
            Image<Rgba32> image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

            image.Mutate(x => x.Flip(FlipMode.Vertical));

            var pixels = new List<byte>(4 * image.Width * image.Height);

            for (int y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);

                for (int x = 0; x < image.Width; x++)
                {
                    pixels.Add(row[x].R);
                    pixels.Add(row[x].G);
                    pixels.Add(row[x].B);
                    pixels.Add(row[x].A);
                }
            }
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels.ToArray());
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Use()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }
    };

    public class MatFigure
    {
        int VBO, VAO;
        float[] vertices;

        public MatFigure(Shader shader, float[] vertices)
        {
            this.vertices = vertices;

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Position attribute
            int vertexCoordLocation = shader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(vertexCoordLocation);
            GL.VertexAttribPointer(vertexCoordLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Normal attribute
            int normalLoc = shader.GetAttribLocation("normal");
            GL.EnableVertexAttribArray(normalLoc);
            GL.VertexAttribPointer(normalLoc, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }


        public void draw(Shader shader)
        {
            shader.Use();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Quads, 0, vertices.Length / 6);
            GL.BindVertexArray(0);
        }
    }

    public class Figure
    {
        int VBO, VAO;
        float[] vertices;
        public Figure(Shader shader, float[] verticles)
        {
            vertices = verticles;

            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Position attribute
            int vertexCoordLocation = shader.GetAttribLocation("position");
            GL.EnableVertexAttribArray(vertexCoordLocation);
            GL.VertexAttribPointer(vertexCoordLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);

        }

        public void draw(Shader shader, BeginMode primitive)
        {
            shader.Use();
            GL.BindVertexArray(VAO);
            GL.DrawArrays(primitive, 0, vertices.Length / 3);
            GL.BindVertexArray(0);
        }
    }


} */
