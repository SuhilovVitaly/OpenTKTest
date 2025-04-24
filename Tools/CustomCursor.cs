using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SkiaSharp;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace OpenTKTest.Tools
{
    public class CustomCursor : IDisposable
    {
        private int _texture;
        private int _vao;
        private int _vbo;
        private int _shader;
        private Vector2 _position;
        private Vector2 _size;

        public CustomCursor(string imagePath)
        {
            LoadTexture(imagePath);
            CreateShader();
            CreateBuffers();
        }

        private void LoadTexture(string imagePath)
        {
            _texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            using (var bitmap = SKBitmap.Decode(imagePath))
            {
                _size = new Vector2(bitmap.Width, bitmap.Height);
                
                // Конвертируем в RGBA
                var pixels = new byte[bitmap.Width * bitmap.Height * 4];
                var sourcePixels = bitmap.Pixels;
                for (int i = 0; i < sourcePixels.Length; i++)
                {
                    var pixel = sourcePixels[i];
                    int idx = i * 4;
                    pixels[idx] = pixel.Red;
                    pixels[idx + 1] = pixel.Green;
                    pixels[idx + 2] = pixel.Blue;
                    pixels[idx + 3] = pixel.Alpha;
                }

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    bitmap.Width, bitmap.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        private void CreateShader()
        {
            string vertexShaderSource = @"
                #version 330 core
                layout (location = 0) in vec2 aPosition;
                layout (location = 1) in vec2 aTexCoord;
                
                out vec2 texCoord;
                uniform vec2 uPosition;
                uniform vec2 uSize;
                uniform vec2 uScreenSize;
                
                void main()
                {
                    vec2 pos = (aPosition * uSize + uPosition) / uScreenSize * 2.0 - 1.0;
                    gl_Position = vec4(pos, 0.0, 1.0);
                    texCoord = aTexCoord;
                }";

            string fragmentShaderSource = @"
                #version 330 core
                in vec2 texCoord;
                out vec4 FragColor;
                
                uniform sampler2D uTexture;
                
                void main()
                {
                    FragColor = texture(uTexture, texCoord);
                }";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            _shader = GL.CreateProgram();
            GL.AttachShader(_shader, vertexShader);
            GL.AttachShader(_shader, fragmentShader);
            GL.LinkProgram(_shader);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private void CreateBuffers()
        {
            float[] vertices = {
                // Position    // TexCoords
                0.0f, 1.0f,   0.0f, 0.0f,
                1.0f, 0.0f,   1.0f, 1.0f,
                0.0f, 0.0f,   0.0f, 1.0f,
                
                0.0f, 1.0f,   0.0f, 0.0f,
                1.0f, 1.0f,   1.0f, 0.0f,
                1.0f, 0.0f,   1.0f, 1.0f
            };

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();

            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
        }

        public void Update(Vector2 mousePosition)
        {
            _position = new Vector2(
                 mousePosition.X - _size.X + 13,
                mousePosition.Y - _size.Y + 5
            );
        }

        public void Render(Vector2 screenSize)
        {
            GL.UseProgram(_shader);
            GL.BindVertexArray(_vao);
            GL.BindTexture(TextureTarget.Texture2D, _texture);

            int positionLocation = GL.GetUniformLocation(_shader, "uPosition");
            int sizeLocation = GL.GetUniformLocation(_shader, "uSize");
            int screenSizeLocation = GL.GetUniformLocation(_shader, "uScreenSize");

            GL.Uniform2(positionLocation, _position);
            GL.Uniform2(sizeLocation, _size);
            GL.Uniform2(screenSizeLocation, screenSize);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            GL.Disable(EnableCap.Blend);
        }

        public void Dispose()
        {
            GL.DeleteTexture(_texture);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(_shader);
        }
    }
} 