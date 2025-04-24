using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenTKTest.Tools
{
    public class TextRenderer : IDisposable
    {
        private readonly Font _font;
        private readonly Brush _brush;
        private int _textureId;
        private int _width;
        private int _height;
        private int _vertexBufferObject;
        private int _vertexArrayObject;
        private int _shaderProgram;

        // Вершины для отрисовки текста (нормализованные координаты)
        private float[] _vertices =
        {
            // Position     // TexCoords
            0.0f, 0.0f,    0.0f, 0.0f,  // Top-left
            1.0f, 0.0f,    1.0f, 0.0f,  // Top-right
            1.0f, 1.0f,    1.0f, 1.0f,  // Bottom-right
            0.0f, 1.0f,    0.0f, 1.0f   // Bottom-left
        };

        public TextRenderer(string fontFamily = "Arial", float fontSize = 12)
        {
            _font = new Font(fontFamily, fontSize);
            _brush = Brushes.White;

            // Create and compile shaders
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText("Shaders/TextVertex.glsl"));
            GL.CompileShader(vertexShader);

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText("Shaders/TextFragment.glsl"));
            GL.CompileShader(fragmentShader);

            // Create and link shader program
            _shaderProgram = GL.CreateProgram();
            GL.AttachShader(_shaderProgram, vertexShader);
            GL.AttachShader(_shaderProgram, fragmentShader);
            GL.LinkProgram(_shaderProgram);

            // Delete shaders as they're linked into the program and no longer needed
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            // Generate and bind VAO
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            // Generate and bind VBO
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            // Position attribute
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // TexCoord attribute
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));
            GL.EnableVertexAttribArray(1);
        }

        public void RenderText(string text, int x, int y)
        {
            using (var bitmap = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var size = graphics.MeasureString(text, _font);
                _width = (int)size.Width;
                _height = (int)size.Height;

                using (var textBitmap = new Bitmap(_width, _height))
                using (var textGraphics = Graphics.FromImage(textBitmap))
                {
                    textGraphics.Clear(Color.Transparent);
                    textGraphics.DrawString(text, _font, _brush, 0, 0);

                    var data = textBitmap.LockBits(
                        new Rectangle(0, 0, _width, _height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    if (_textureId == 0)
                    {
                        _textureId = GL.GenTexture();
                        GL.BindTexture(TextureTarget.Texture2D, _textureId);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    }

                    GL.BindTexture(TextureTarget.Texture2D, _textureId);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                        _width, _height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
                        PixelType.UnsignedByte, data.Scan0);

                    textBitmap.UnlockBits(data);
                }
            }

            // Получаем размеры viewport
            int[] viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
            int viewportWidth = viewport[2];
            int viewportHeight = viewport[3];

            // Преобразуем координаты из экранных в OpenGL (-1 до 1)
            float glX = (2.0f * x) / viewportWidth - 1.0f;
            // Корректируем Y-координату с учетом высоты текста
            float glY = -1.0f + (2.0f * (viewportHeight - y)) / viewportHeight;
            float glWidth = (2.0f * _width) / viewportWidth;
            float glHeight = (2.0f * _height) / viewportHeight;

            // Создаем вершины для текста
            float[] vertices = {
                // Position                     // TexCoords
                glX, glY,                      0.0f, 0.0f,  // Top-left
                glX + glWidth, glY,            1.0f, 0.0f,  // Top-right
                glX + glWidth, glY - glHeight, 1.0f, 1.0f,  // Bottom-right
                glX, glY - glHeight,           0.0f, 1.0f   // Bottom-left
            };

            // Обновляем буфер вершин
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StreamDraw);

            GL.UseProgram(_shaderProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            GL.Uniform1(GL.GetUniformLocation(_shaderProgram, "textTexture"), 0);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            GL.Disable(EnableCap.Blend);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteTexture(_textureId);
            GL.DeleteProgram(_shaderProgram);
            _font.Dispose();
            _brush.Dispose();
        }
    }
} 