using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ComputerGraphicsFifth.Sources
{
    class Texture
    {
        private int[] _texture = new int[1];
        private int _index;

        public Texture(int index, TextureUnit unit)
        {
            _index = index;
            List<string> paths = new List<string>();
            paths.Add("../../Textures/Metal.jpg");
            paths.Add("../../Textures/Porcelain.jpg");
            paths.Add("../../Textures/CoffeeContainer.png");
            paths.Add("../../Textures/CoffeeContainerBottom.jpg");
            paths.Add("../../Textures/CoffeeUp.jpg");
            paths.Add("../../Textures/White.png");
            paths.Add("../../Textures/Linoleum.jpg");

            GL.GenTextures(1, _texture);
            var image = Image.Load<Rgba32>(paths[index]);
            var texture = ConvertToGlFormat(image);

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, _texture[0]);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texture.ToArray());
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Bind() => GL.BindTexture(TextureTarget.Texture2D, _texture[0]);

        public void Unbind() => GL.BindTexture(TextureTarget.Texture2D, 0);

        private List<byte> ConvertToGlFormat(Image<Rgba32> image)
        {
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

            return pixels;
        }
    }
}
