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
    class ModelBuilder
    {
        private List<float> _normalCoordsList = new List<float>();
        private float[] _normalCoords;
        private int _rows = 150;
        private int _columns = 150;

        public Vector3 NormalCoords(Vector3 v1, Vector3 v2, Vector3 v3) => Vector3.Normalize(Vector3.Cross(v2 - v1, v3 - v1));

        public Vector2 ImageCoords(int i, int j, int rows, int columns) => new Vector2((float)j / (float)columns, 1.0f - (float)i / (float)rows);

        public Vector3 CylindricalCoords(float r, float theta1, float theta2, float dx, float dy, float dz)
        {
            float x, y, z;
            x = r * (float)Math.Cos(theta2) * (float)Math.Sin(theta1) + dx;
            y = r * (float)Math.Cos(theta1) + dy;
            z = r * (float)Math.Sin(theta2) * (float)Math.Sin(theta1) + dz;
            Vector3 coords = new Vector3(x, y, z);
            return coords;
        }

        public void BuildInCylindrical(ref List<float> vertices, int rows, int columns, float r, float height, int startTheta1, int endTheta1, int startTheta2, int endTheta2, float dx, float dy, float dz)
        {
            bool isCylinder = ((startTheta1 == 90) && (endTheta1 == 90)) ? true : false;
            float yStep = height / (float)columns;
            float theta1Step = (float)(endTheta1 - startTheta1) / (float)columns;
            float theta2Step = (float)(endTheta2 - startTheta2) / (float)rows;
            for (int i = 0; i < columns; i++)
            {
                float theta1 = MathHelper.DegreesToRadians(startTheta1 + theta1Step * i);
                for (int j = 0; j < rows; j++)
                {
                    Vector3[] points = new Vector3[4];
                    float theta2 = MathHelper.DegreesToRadians(startTheta2 + theta2Step * j);
                    points[0] = CylindricalCoords(r, theta1, theta2, dx, dy, dz);
                    points[1] = CylindricalCoords(r, theta1 + MathHelper.DegreesToRadians(theta1Step), theta2, dx, isCylinder ? dy + yStep : dy, dz);
                    points[2] = CylindricalCoords(r, theta1 + MathHelper.DegreesToRadians(theta1Step), theta2 + MathHelper.DegreesToRadians(theta2Step), dx, isCylinder ? dy + yStep : dy, dz);
                    points[3] = CylindricalCoords(r, theta1, theta2 + MathHelper.DegreesToRadians(theta2Step), dx, dy, dz);
                    Vector3 normal = NormalCoords(points[0], points[1], points[2]);
                    Vector2[] imagePoints = new Vector2[4];
                    imagePoints[0] = ImageCoords(i, j, rows, columns);
                    imagePoints[1] = ImageCoords(i + 1, j, rows, columns);
                    imagePoints[2] = ImageCoords(i + 1, j + 1, rows, columns);
                    imagePoints[3] = ImageCoords(i, j + 1, rows, columns);
                    for (int t = 0; t < 4; t++)
                    {
                        vertices.Add(points[t].X);
                        vertices.Add(points[t].Y);
                        vertices.Add(points[t].Z);
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        vertices.Add(imagePoints[t].X);
                        vertices.Add(imagePoints[t].Y);

                        // For testing normals
                        _normalCoordsList.Add(points[t].X);
                        _normalCoordsList.Add(points[t].Y);
                        _normalCoordsList.Add(points[t].Z);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(points[t].X + normal.X * 0.05f);
                        _normalCoordsList.Add(points[t].Y + normal.Y * 0.05f);
                        _normalCoordsList.Add(points[t].Z + normal.Z * 0.05f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                    }
                }
                if (isCylinder) dy += yStep;
            }
        }

        public void BuildFlatDX(ref List<float> vertices, int rows, int columns, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {

            float xStep = (p4.X - p1.X) / (float)columns;
            float yStep = (p2.Y - p1.Y) / (float)rows;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    Vector3[] points = new Vector3[4];
                    points[0] = new Vector3(p1.X + j * xStep, p1.Y + i * yStep, p1.Z);
                    points[1] = new Vector3(p1.X + j * xStep, p1.Y + (i + 1) * yStep, p1.Z);
                    points[2] = new Vector3(p1.X + (j + 1) * xStep, p1.Y + (i + 1) * yStep, p1.Z);
                    points[3] = new Vector3(p1.X + (j + 1) * xStep, p1.Y + i * yStep, p1.Z);
                    Vector3 normal = NormalCoords(points[0], points[1], points[2]);
                    Vector2[] imagePoints = new Vector2[4];
                    imagePoints[0] = ImageCoords(i, j, rows, columns);
                    imagePoints[1] = ImageCoords(i + 1, j, rows, columns);
                    imagePoints[2] = ImageCoords(i + 1, j + 1, rows, columns);
                    imagePoints[3] = ImageCoords(i, j + 1, rows, columns);
                    for (int t = 0; t < 4; t++)
                    {
                        vertices.Add(points[t].X);
                        vertices.Add(points[t].Y);
                        vertices.Add(points[t].Z);
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        vertices.Add(imagePoints[t].X);
                        vertices.Add(imagePoints[t].Y);

                        // For testing normals
                        _normalCoordsList.Add(points[t].X);
                        _normalCoordsList.Add(points[t].Y);
                        _normalCoordsList.Add(points[t].Z);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(points[t].X + normal.X * 0.05f);
                        _normalCoordsList.Add(points[t].Y + normal.Y * 0.05f);
                        _normalCoordsList.Add(points[t].Z + normal.Z * 0.05f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                    }
                }
        }

        public void BuildFlatDZ(ref List<float> vertices, int rows, int columns, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {

            float zStep = (p4.Z - p1.Z) / (float)columns;
            float yStep = (p2.Y - p1.Y) / (float)rows;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    Vector3[] points = new Vector3[4];
                    points[0] = new Vector3(p1.X, p1.Y + i * yStep, p1.Z + j * zStep);
                    points[1] = new Vector3(p1.X, p1.Y + (i + 1) * yStep, p1.Z + j * zStep);
                    points[2] = new Vector3(p1.X, p1.Y + (i + 1) * yStep, p1.Z + (j + 1) * zStep);
                    points[3] = new Vector3(p1.X, p1.Y + i * yStep, p1.Z + (j + 1) * zStep);
                    Vector3 normal = NormalCoords(points[0], points[1], points[2]);
                    Vector2[] imagePoints = new Vector2[4];
                    imagePoints[0] = ImageCoords(i, j, rows, columns);
                    imagePoints[1] = ImageCoords(i + 1, j, rows, columns);
                    imagePoints[2] = ImageCoords(i + 1, j + 1, rows, columns);
                    imagePoints[3] = ImageCoords(i, j + 1, rows, columns);
                    for (int t = 0; t < 4; t++)
                    {
                        vertices.Add(points[t].X);
                        vertices.Add(points[t].Y);
                        vertices.Add(points[t].Z);
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        vertices.Add(imagePoints[t].X);
                        vertices.Add(imagePoints[t].Y);

                        // For testing normals
                        _normalCoordsList.Add(points[t].X);
                        _normalCoordsList.Add(points[t].Y);
                        _normalCoordsList.Add(points[t].Z);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(points[t].X + normal.X * 0.05f);
                        _normalCoordsList.Add(points[t].Y + normal.Y * 0.05f);
                        _normalCoordsList.Add(points[t].Z + normal.Z * 0.05f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                    }
                }
        }

        public void BuildFlatDXDZ(ref List<float> vertices, int rows, int columns, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {

            float xStep = (p4.X - p1.X) / (float)columns;
            float zStep = (p2.Z - p1.Z) / (float)rows;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    Vector3[] points = new Vector3[4];
                    points[0] = new Vector3(p1.X + j * xStep, p1.Y, p1.Z + i * zStep);
                    points[1] = new Vector3(p1.X + j * xStep, p1.Y, p1.Z + (i + 1) * zStep);
                    points[2] = new Vector3(p1.X + (j + 1) * xStep, p1.Y, p1.Z + (i + 1) * zStep);
                    points[3] = new Vector3(p1.X + (j + 1) * xStep, p1.Y, p1.Z + i * zStep);
                    Vector3 normal = NormalCoords(points[0], points[1], points[2]);
                    Vector2[] imagePoints = new Vector2[4];
                    imagePoints[0] = ImageCoords(i, j, rows, columns);
                    imagePoints[1] = ImageCoords(i + 1, j, rows, columns);
                    imagePoints[2] = ImageCoords(i + 1, j + 1, rows, columns);
                    imagePoints[3] = ImageCoords(i, j + 1, rows, columns);
                    for (int t = 0; t < 4; t++)
                    {
                        vertices.Add(points[t].X);
                        vertices.Add(points[t].Y);
                        vertices.Add(points[t].Z);
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        vertices.Add(imagePoints[t].X);
                        vertices.Add(imagePoints[t].Y);

                        // For testing normals
                        _normalCoordsList.Add(points[t].X);
                        _normalCoordsList.Add(points[t].Y);
                        _normalCoordsList.Add(points[t].Z);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(points[t].X + normal.X * 0.05f);
                        _normalCoordsList.Add(points[t].Y + normal.Y * 0.05f);
                        _normalCoordsList.Add(points[t].Z + normal.Z * 0.05f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                    }
                }
        }

        public Vector3 CircleCoords(float r, float theta, float dx, float dy, float dz)
        {
            float x = r * (float)Math.Cos(theta) + dx;
            float z = r * (float)Math.Sin(theta) + dz;
            Vector3 coords = new Vector3(x, dy, z);
            return coords;
        }

        public void BuildCircle(ref List<float> vertices, bool isReversed, int rows, int columns, float r, float dx, float dy, float dz)
        {
            float angleStep = (float)360 / (float)columns;
            float rStep = r / (float)rows;
            angleStep = isReversed ? -angleStep : angleStep;
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                {
                    Vector3[] points = new Vector3[4];
                    float theta = MathHelper.DegreesToRadians(angleStep * j);
                    points[0] = CircleCoords(i* rStep, theta, dx, dy, dz);
                    points[1] = CircleCoords((i + 1) * rStep, theta, dx, dy, dz);
                    points[2] = CircleCoords((i + 1) * rStep, theta + MathHelper.DegreesToRadians(angleStep), dx, dy, dz);
                    points[3] = CircleCoords(i * rStep, theta + MathHelper.DegreesToRadians(angleStep), dx, dy, dz);
                    Vector3 normal = NormalCoords(points[0], points[1], points[2]);
                    Vector2[] imagePoints = new Vector2[4];
                    imagePoints[0] = ImageCoords(i, j, rows, columns);
                    imagePoints[1] = ImageCoords(i + 1, j, rows, columns);
                    imagePoints[2] = ImageCoords(i + 1, j + 1, rows, columns);
                    imagePoints[3] = ImageCoords(i, j + 1, rows, columns);
                    for (int t = 0; t < 4; t++)
                    {
                        vertices.Add(points[t].X);
                        vertices.Add(points[t].Y);
                        vertices.Add(points[t].Z);
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        vertices.Add(imagePoints[t].X);
                        vertices.Add(imagePoints[t].Y);

                        // For testing normals
                        _normalCoordsList.Add(points[t].X);
                        _normalCoordsList.Add(points[t].Y);
                        _normalCoordsList.Add(points[t].Z);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(points[t].X + normal.X * 0.05f);
                        _normalCoordsList.Add(points[t].Y + normal.Y * 0.05f);
                        _normalCoordsList.Add(points[t].Z + normal.Z * 0.05f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                    }
                }
        }

        public Vector3 TorusCoords(float R, float r, float theta1, float theta2, float dx, float dy, float dz)
        {
            float x = (R + r * (float)Math.Cos(theta2)) * (float)Math.Cos(theta1) + dx;
            float y = (R + r * (float)Math.Cos(theta2)) * (float)Math.Sin(theta1) + dy;
            float z = r * (float)Math.Sin(theta2) + dz;
            Vector3 coords = new Vector3(x, y, z);
            return coords;
        }

        public void BuildTorus(ref List<float> vertices, int rows, int columns, float R, float r, int startTheta1, int endTheta1, int startTheta2, int endTheta2, float dx, float dy, float dz)
        {
            float theta1Step = (float)(endTheta1 - startTheta1) / (float)rows;
            float theta2Step = (float)(endTheta2 - startTheta2) / (float)columns;
            for (int i = 0; i < rows; i++)
            {
                float theta1 = MathHelper.DegreesToRadians(startTheta1 + theta1Step * i);
                for (int j = 0; j < columns; j++)
                {
                    Vector3[] points = new Vector3[4];
                    float theta2 = MathHelper.DegreesToRadians(startTheta2 + theta2Step * j);
                    points[0] = TorusCoords(R, r, theta1, theta2, dx, dy, dz);
                    points[1] = TorusCoords(R, r, theta1 + MathHelper.DegreesToRadians(theta1Step), theta2, dx, dy, dz);
                    points[2] = TorusCoords(R, r, theta1 + MathHelper.DegreesToRadians(theta1Step), theta2 + MathHelper.DegreesToRadians(theta2Step), dx, dy, dz);
                    points[3] = TorusCoords(R, r, theta1, theta2 + MathHelper.DegreesToRadians(theta2Step), dx, dy, dz);
                    Vector3 normal = NormalCoords(points[0], points[1], points[2]);
                    Vector2[] imagePoints = new Vector2[4];
                    imagePoints[0] = ImageCoords(i, j, rows, columns);
                    imagePoints[1] = ImageCoords(i + 1, j, rows, columns);
                    imagePoints[2] = ImageCoords(i + 1, j + 1, rows, columns);
                    imagePoints[3] = ImageCoords(i, j + 1, rows, columns);
                    for (int t = 0; t < 4; t++)
                    {
                        vertices.Add(points[t].X);
                        vertices.Add(points[t].Y);
                        vertices.Add(points[t].Z);
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        vertices.Add(imagePoints[t].X);
                        vertices.Add(imagePoints[t].Y);

                        // For testing normals
                        _normalCoordsList.Add(points[t].X);
                        _normalCoordsList.Add(points[t].Y);
                        _normalCoordsList.Add(points[t].Z);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(points[t].X + normal.X * 0.05f);
                        _normalCoordsList.Add(points[t].Y + normal.Y * 0.05f);
                        _normalCoordsList.Add(points[t].Z + normal.Z * 0.05f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                    }
                }
            }
        }

        public Vector3 LegCoordsDX(float r, float theta, float dx, float dy, float dz)
        {
            float x, y, z;
            x = dx;
            y = r * (float)Math.Cos(theta) + dy;
            z = r * (float)Math.Sin(theta) + dz;
            Vector3 coords = new Vector3(x, y, z);
            return coords;
        }
        public Vector3 LegCoordsDZ(float r, float theta, float dx, float dy, float dz)
        {
            float x, y, z;
            x = r * (float)Math.Cos(theta) + dx;
            y = r * (float)Math.Sin(theta) + dy;
            z = dz;
            Vector3 coords = new Vector3(x, y, z);
            return coords;
        }

        private void BuildLeg(ref List<float> vertices, bool isDX, int rows, int columns, float r, float height, float dx, float dy, float dz)
        {
            float step = height / (float)rows;
            float thetaStep = (float)360 / (float)columns;
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Vector3[] points = new Vector3[4];
                    float theta = MathHelper.DegreesToRadians(thetaStep * j);
                    if (isDX)
                    {
                        points[0] = LegCoordsDX(r, theta, dx, dy, dz);
                        points[1] = LegCoordsDX(r, theta, dx + step, dy, dz);
                        points[2] = LegCoordsDX(r, theta + MathHelper.DegreesToRadians(thetaStep), dx + step, dy, dz);
                        points[3] = LegCoordsDX(r, theta + MathHelper.DegreesToRadians(thetaStep), dx, dy, dz);
                    } else
                    {
                        points[0] = LegCoordsDZ(r, theta, dx, dy, dz);
                        points[1] = LegCoordsDZ(r, theta, dx, dy, dz + step);
                        points[2] = LegCoordsDZ(r, theta + MathHelper.DegreesToRadians(thetaStep), dx, dy, dz + step);
                        points[3] = LegCoordsDZ(r, theta + MathHelper.DegreesToRadians(thetaStep), dx, dy, dz);
                    }
                    
                    Vector3 normal = NormalCoords(points[0], points[1], points[2]);
                    Vector2[] imagePoints = new Vector2[4];
                    imagePoints[0] = ImageCoords(i, j, rows, columns);
                    imagePoints[1] = ImageCoords(i + 1, j, rows, columns);
                    imagePoints[2] = ImageCoords(i + 1, j + 1, rows, columns);
                    imagePoints[3] = ImageCoords(i, j + 1, rows, columns);
                    for (int t = 0; t < 4; t++)
                    {
                        vertices.Add(points[t].X);
                        vertices.Add(points[t].Y);
                        vertices.Add(points[t].Z);
                        vertices.Add(normal.X);
                        vertices.Add(normal.Y);
                        vertices.Add(normal.Z);
                        vertices.Add(imagePoints[t].X);
                        vertices.Add(imagePoints[t].Y);

                        // For testing normals
                        _normalCoordsList.Add(points[t].X);
                        _normalCoordsList.Add(points[t].Y);
                        _normalCoordsList.Add(points[t].Z);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(points[t].X + normal.X * 0.05f);
                        _normalCoordsList.Add(points[t].Y + normal.Y * 0.05f);
                        _normalCoordsList.Add(points[t].Z + normal.Z * 0.05f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                        _normalCoordsList.Add(1f);
                    }
                }
                if (isDX){ 
                    dx += step;
                } else dz += step;
            }
        }

        public void CalculateKettle(ref Kettle kettle, ref Shader shader)
        {
            float dx = -0.8f, dy = -0.7f, dz = -0.7f, r = 0.5f, r2 = 0.05f;

            kettle.Container = new Model(2, TextureUnit.Texture2);
            kettle.CoffeeFoam = new Model(4, TextureUnit.Texture4);
            kettle.ContainerBottom = new Model(3, TextureUnit.Texture3);
            kettle.Lid = new Model(0, TextureUnit.Texture0);
            kettle.Legs = new Model[4];
            kettle.ReversedLegs = new Model[4];
            kettle.Circles = new Model[2];
            kettle.ReversedCircles = new Model[2];
            kettle.Sticks = new Model[4];
            kettle.ReversedSticks = new Model[4];
            for (int i = 0; i < 4; i++)
            {
                kettle.Legs[i] = new Model(0, TextureUnit.Texture0);
                kettle.ReversedLegs[i] = new Model(0, TextureUnit.Texture0);
                kettle.Sticks[i] = new Model(0, TextureUnit.Texture0);
                kettle.ReversedSticks[i] = new Model(0, TextureUnit.Texture0);
            }
            for (int i = 0; i < 2; i++)
            {
                kettle.Circles[i] = new Model(0, TextureUnit.Texture0);
                kettle.ReversedCircles[i] = new Model(0, TextureUnit.Texture0);
            }

            List<float> vertices = new List<float>();

            BuildInCylindrical(ref vertices, _rows, _columns, r, 2.5f, 90, 90, 0, 360, dx, dy, dz);
            kettle.Container.SetVertices(ref vertices);
            kettle.Container.UpdateBuffers(ref shader);
            vertices.Clear();

            float rHolder = r * (float)Math.Sin(MathHelper.DegreesToRadians(5));
            float dyHolder = r * (float)Math.Cos(MathHelper.DegreesToRadians(5));
            BuildCircle(ref vertices, false, _rows, _columns, r, dx, dy + 2.5f, dz);
            BuildInCylindrical(ref vertices, _rows, _columns, r, 2 * r, -90, -5, 0, 360, dx, dy + 2.5f, dz);
            BuildInCylindrical(ref vertices, _rows, _columns, rHolder, 0.08f, 90, 90, 0, 360, dx, dy + dyHolder + 2.5f, dz);
            rHolder /= (float)Math.Sin(Math.PI / 6);
            dyHolder += 0.08f + rHolder * (float)Math.Cos(Math.PI / 6);
            BuildInCylindrical(ref vertices, _rows, _columns, rHolder, 2 * rHolder, -150, 0, 0, 360, dx, dy + dyHolder + 2.5f, dz);
            kettle.Lid.SetVertices(ref vertices);
            kettle.Lid.UpdateBuffers(ref shader);
            vertices.Clear();

            BuildCircle(ref vertices, false, _rows, _columns, r, dx, dy, dz);
            kettle.ContainerBottom.SetVertices(ref vertices);
            kettle.ContainerBottom.UpdateBuffers(ref shader);
            vertices.Clear();

            BuildCircle(ref vertices, true, _rows, _columns, r, dx, dy + 2.5f / 2f, dz);
            kettle.CoffeeFoam.SetVertices(ref vertices);
            kettle.CoffeeFoam.UpdateBuffers(ref shader);
            vertices.Clear();

            BuildFlatDX(ref vertices, _rows, _columns,
                      new Vector3(dx + r2, dy, dz + r + 0.01f),
                      new Vector3(dx + r2, dy + 2.5f, dz + r + 0.01f),
                      new Vector3(dx - r2, dy + 2.5f, dz + r + 0.01f),
                      new Vector3(dx - r2, dy, dz + r + 0.01f));
            kettle.Sticks[0].SetVertices(ref vertices);
            kettle.Sticks[0].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildFlatDZ(ref vertices, _rows, _columns,
                      new Vector3(dx - r - 0.01f, dy, dz + r2),
                      new Vector3(dx - r - 0.01f, dy + 2.5f, dz + r2),
                      new Vector3(dx - r - 0.01f, dy + 2.5f, dz - r2),
                      new Vector3(dx - r - 0.01f, dy, dz - r2));
            kettle.Sticks[1].SetVertices(ref vertices);
            kettle.Sticks[1].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildFlatDX(ref vertices, _rows, _columns,
                       new Vector3(dx - r2, dy, dz - r - 0.01f),
                       new Vector3(dx - r2, dy + 2.5f, dz - r - 0.01f),
                       new Vector3(dx + r2, dy + 2.5f, dz - r - 0.01f),
                       new Vector3(dx + r2, dy, dz - r - 0.01f));
            kettle.Sticks[2].SetVertices(ref vertices);
            kettle.Sticks[2].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildFlatDZ(ref vertices, _rows, _columns,
                      new Vector3(dx + r + 0.01f, dy, dz - r2),
                      new Vector3(dx + r + 0.01f, dy + 2.5f, dz - r2),
                      new Vector3(dx + r + 0.01f, dy + 2.5f, dz + r2),
                      new Vector3(dx + r + 0.01f, dy, dz + r2));
            kettle.Sticks[3].SetVertices(ref vertices);
            kettle.Sticks[3].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildFlatDX(ref vertices, _rows, _columns,
                      new Vector3(dx - r2, dy, dz + r + 0.005f),
                      new Vector3(dx - r2, dy + 2.5f, dz + r + 0.005f),
                      new Vector3(dx + r2, dy + 2.5f, dz + r + 0.005f),
                      new Vector3(dx + r2, dy, dz + r + 0.005f));
            kettle.ReversedSticks[0].SetVertices(ref vertices);
            kettle.ReversedSticks[0].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildFlatDZ(ref vertices, _rows, _columns,
                      new Vector3(dx - r - 0.005f, dy, dz - r2),
                      new Vector3(dx - r - 0.005f, dy + 2.5f, dz - r2),
                      new Vector3(dx - r - 0.005f, dy + 2.5f, dz + r2),
                      new Vector3(dx - r - 0.005f, dy, dz + r2));
            kettle.ReversedSticks[1].SetVertices(ref vertices);
            kettle.ReversedSticks[1].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildFlatDX(ref vertices, _rows, _columns,
                       new Vector3(dx + r2, dy, dz - r - 0.005f),
                       new Vector3(dx + r2, dy + 2.5f, dz - r - 0.005f),
                       new Vector3(dx - r2, dy + 2.5f, dz - r - 0.005f),
                       new Vector3(dx - r2, dy, dz - r - 0.005f));
            kettle.ReversedSticks[2].SetVertices(ref vertices);
            kettle.ReversedSticks[2].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildFlatDZ(ref vertices, _rows, _columns,
                      new Vector3(dx + r + 0.005f, dy, dz + r2),
                      new Vector3(dx + r + 0.005f, dy + 2.5f, dz + r2),
                      new Vector3(dx + r + 0.005f, dy + 2.5f, dz - r2),
                      new Vector3(dx + r + 0.005f, dy, dz - r2));
            kettle.ReversedSticks[3].SetVertices(ref vertices);
            kettle.ReversedSticks[3].UpdateBuffers(ref shader);
            vertices.Clear();

            for (int i = 0; i < 2; i++)
            {
                BuildInCylindrical(ref vertices, _rows, _columns, r + 0.02f, 0.2f, 90, 90, 0, 360, dx, dy + 0.75f * (float)(i + 1), dz);
                kettle.Circles[i].SetVertices(ref vertices);
                kettle.Circles[i].UpdateBuffers(ref shader);
                vertices.Clear();
            }
            for (int i = 0; i < 2; i++)
            {
                BuildInCylindrical(ref vertices, _rows, _columns, -(r + 0.015f), -0.2f, 90, 90, 0, 360, dx, dy + 0.2f + 0.75f * (float)(i + 1), dz);
                kettle.ReversedCircles[i].SetVertices(ref vertices);
                kettle.ReversedCircles[i].UpdateBuffers(ref shader);
                vertices.Clear();
            }

            BuildLeg(ref vertices, true, _rows, _columns, r2, -0.1f, dx + 0.1f / 2f, dy - r2, dz + r);
            kettle.Legs[0].SetVertices(ref vertices);
            kettle.Legs[0].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildLeg(ref vertices, true, _rows, _columns, r2, -0.1f, dx + 0.1f / 2f, dy - r2, dz - r);
            kettle.Legs[1].SetVertices(ref vertices);
            kettle.Legs[1].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildLeg(ref vertices, false, _rows, _columns, r2, -0.1f, dx - r, dy - r2, dz + 0.1f / 2f);
            kettle.Legs[2].SetVertices(ref vertices);
            kettle.Legs[2].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildLeg(ref vertices, false, _rows, _columns, r2, -0.1f, dx + r, dy - r2, dz + 0.1f / 2f);
            kettle.Legs[3].SetVertices(ref vertices);
            kettle.Legs[3].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildLeg(ref vertices, true, _rows, _columns, r2 - 0.01f, 0.1f, dx - 0.1f / 2f, dy - r2, dz + r);
            kettle.ReversedLegs[0].SetVertices(ref vertices);
            kettle.ReversedLegs[0].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildLeg(ref vertices, true, _rows, _columns, r2 - 0.01f, 0.1f, dx - 0.1f / 2f, dy - r2, dz - r);
            kettle.ReversedLegs[1].SetVertices(ref vertices);
            kettle.ReversedLegs[1].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildLeg(ref vertices, false, _rows, _columns, r2 - 0.01f, 0.1f, dx - r, dy - r2, dz - 0.1f / 2f);
            kettle.ReversedLegs[2].SetVertices(ref vertices);
            kettle.ReversedLegs[2].UpdateBuffers(ref shader);
            vertices.Clear();
            BuildLeg(ref vertices, false, _rows, _columns, r2 - 0.01f, 0.1f, dx + r, dy - r2, dz - 0.1f / 2f);
            kettle.ReversedLegs[3].SetVertices(ref vertices);
            kettle.ReversedLegs[3].UpdateBuffers(ref shader);
            vertices.Clear();

        }

        public void CalculateCup(ref Cup cup, ref Shader shader)
        {
            float dx = 1f, dy = -0.19f, dz = -0.5f, r = 0.6f;
            cup.Container = new Model(1, TextureUnit.Texture1);
            cup.Handle = new Model(1, TextureUnit.Texture1);
            cup.CoffeeFoam = new Model(4, TextureUnit.Texture4);

            List<float> vertices = new List<float>();

            BuildInCylindrical(ref vertices, _rows, _columns, r, 2f * r, -180, -90, 0, 360, dx, dy, dz);
            cup.Container.SetVertices(ref vertices);
            cup.Container.UpdateBuffers(ref shader);
            vertices.Clear();

            BuildCircle(ref vertices, true, _rows, _columns, r, dx, dy, dz);
            cup.CoffeeFoam.SetVertices(ref vertices);
            cup.CoffeeFoam.UpdateBuffers(ref shader);
            vertices.Clear();

            BuildTorus(ref vertices, _rows, _columns, 0.2f, 0.02f, -140, 90, -180, 180, dx + r - 0.05f, dy - 0.3f, dz);
            cup.Handle.SetVertices(ref vertices);
            cup.Handle.UpdateBuffers(ref shader);
            vertices.Clear();
        }

        public void CalculatePlate(ref Model plate, ref Shader shader)
        {
            float dx = 1f, dy = -0.795f, dz = -0.5f, r = 0.85f;

            List<float> vertices = new List<float>();

            BuildCircle(ref vertices, true, _rows, _columns, r, dx, dy, dz);
            r = r / (float)Math.Sin(Math.PI / 6);
            dy += r * (float)Math.Cos(Math.PI / 6);
            BuildInCylindrical(ref vertices, _rows, _columns, r, 2f * r, 140, 150, 0, 360, dx, dy, dz);
            plate.SetVertices(ref vertices);
            plate.UpdateBuffers(ref shader);
            vertices.Clear();
        }

        public void CalculateReversedPlate(ref Model plate, ref Shader shader)
        {
            float dx = 1f, dy = -0.8f, dz = -0.5f, r = 0.85f;

            List<float> vertices = new List<float>();

            BuildCircle(ref vertices, false, _rows, _columns, r, dx, dy, dz);
            r = r / (float)Math.Sin(Math.PI / 6);
            dy += r * (float)Math.Cos(Math.PI / 6);
            BuildInCylindrical(ref vertices, _rows, _columns, r, 2f * r, 150, 140, 0, 360, dx, dy, dz);
            plate.SetVertices(ref vertices);
            plate.UpdateBuffers(ref shader);
            vertices.Clear();
        }

        public void CalculateSpoon(ref Spoon spoon, ref Shader shader)
        {
            float dx = 0.3f, dy = -0.585f, dz = -0.7f, r = 0.2f, r2 = 0.05f;
            spoon.Container = new Model(0, TextureUnit.Texture0);
            spoon.Stick = new Model(0, TextureUnit.Texture0);

            List<float> vertices = new List<float>();

            BuildInCylindrical(ref vertices, _rows, _columns, r, 0, 135, 180, 0, 360, dx, dy, dz);
            spoon.Container.SetVertices(ref vertices);
            spoon.Container.UpdateBuffers(ref shader);
            vertices.Clear();

            BuildFlatDXDZ(ref vertices, _rows, _columns,
                      new Vector3(dx - r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) - 0.02f),
                      new Vector3(dx - r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) + 0.4f),
                      new Vector3(dx + r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) + 0.4f),
                      new Vector3(dx + r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) - 0.02f));
            spoon.Stick.SetVertices(ref vertices);
            spoon.Stick.UpdateBuffers(ref shader);
            vertices.Clear();
        }

        public void CalculateReversedSpoon(ref Spoon spoon, ref Shader shader)
        {
            float dx = 0.3f, dy = -0.59f, dz = -0.7f, r = 0.2f, r2 = 0.05f;
            spoon.Container = new Model(0, TextureUnit.Texture0);
            spoon.Stick = new Model(0, TextureUnit.Texture0);

            List<float> vertices = new List<float>();

            BuildInCylindrical(ref vertices, _rows, _columns, r, 0, 180, 135, 0, 360, dx, dy, dz);
            spoon.Container.SetVertices(ref vertices);
            spoon.Container.UpdateBuffers(ref shader);
            vertices.Clear();

            BuildFlatDXDZ(ref vertices, _rows, _columns,
                      new Vector3(dx + r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) - 0.02f),
                      new Vector3(dx + r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) + 0.4f),
                      new Vector3(dx - r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) + 0.4f),
                      new Vector3(dx - r2, dy - r / (float)Math.Sqrt(2), dz + r / (float)Math.Sqrt(2) - 0.02f));
            spoon.Stick.SetVertices(ref vertices);
            spoon.Stick.UpdateBuffers(ref shader);
            vertices.Clear();
        }

        public void CalculateCube(ref Model model, float r, ref Vector3 center, ref Shader shader)
        {
            List<float> vertices = new List<float>();
            BuildFlatDXDZ(ref vertices, 1, 1,
                      new Vector3(center.X - r, center.Y + r, center.Z - r),
                      new Vector3(center.X - r, center.Y + r, center.Z + r),
                      new Vector3(center.X + r, center.Y + r, center.Z + r),
                      new Vector3(center.X + r, center.Y + r, center.Z - r));
            BuildFlatDXDZ(ref vertices, 1, 1,
                      new Vector3(center.X - r, center.Y - r, center.Z - r),
                      new Vector3(center.X - r, center.Y - r, center.Z + r),
                      new Vector3(center.X + r, center.Y - r, center.Z + r),
                      new Vector3(center.X + r, center.Y - r, center.Z - r));
            BuildFlatDX(ref vertices, 1, 1,
                      new Vector3(center.X - r, center.Y - r, center.Z + r),
                      new Vector3(center.X - r, center.Y + r, center.Z + r),
                      new Vector3(center.X + r, center.Y - r, center.Z + r),
                      new Vector3(center.X + r, center.Y + r, center.Z + r));
            BuildFlatDX(ref vertices, 1, 1,
                      new Vector3(center.X - r, center.Y - r, center.Z - r),
                      new Vector3(center.X - r, center.Y + r, center.Z - r),
                      new Vector3(center.X + r, center.Y - r, center.Z - r),
                      new Vector3(center.X + r, center.Y + r, center.Z - r));
            BuildFlatDZ(ref vertices, 1, 1,
                      new Vector3(center.X + r, center.Y - r, center.Z - r),
                      new Vector3(center.X + r, center.Y + r, center.Z - r),
                      new Vector3(center.X + r, center.Y + r, center.Z + r),
                      new Vector3(center.X + r, center.Y - r, center.Z + r));
            BuildFlatDZ(ref vertices, 1, 1,
                      new Vector3(center.X - r, center.Y - r, center.Z - r),
                      new Vector3(center.X - r, center.Y + r, center.Z - r),
                      new Vector3(center.X - r, center.Y + r, center.Z + r),
                      new Vector3(center.X - r, center.Y - r, center.Z + r));
            model.SetVertices(ref vertices);
            model.UpdateBuffers(ref shader);
        }

        public void CalculateIlluminant(ref Model illuminant, ref Vector3 center, ref Shader shader)
        {
            float r = 0.2f;
            illuminant = new Model(5, TextureUnit.Texture5);
            CalculateCube(ref illuminant, r, ref center, ref shader);
        }

        public void CalculateFloor(ref Model floor, ref Shader shader)
        {
            float dx = 0f, dy = -0.8f, dz = 0f;
            floor = new Model(6, TextureUnit.Texture6);

            List<float> vertices = new List<float>();

            BuildFlatDXDZ(ref vertices, 1, 1,
                          new Vector3(dx - 30.0f, dy, dz - 30.0f),
                          new Vector3(dx - 30.0f, dy, dz + 30.0f),
                          new Vector3(dx + 30.0f, dy, dz + 30.0f),
                          new Vector3(dx + 30.0f, dy, dz - 30.0f));

            floor.SetVertices(ref vertices);
            floor.UpdateBuffers(ref shader);
            vertices.Clear();
        }

        public ref float[] GetNormalCoords() => ref _normalCoords;

        public void UpdateNormalCoords()
        {
            _normalCoords = new float[_normalCoordsList.Count];
            for (var i = 0; i < _normalCoordsList.Count; i++)
                _normalCoords[i] = _normalCoordsList[i];
        }
    }
}
