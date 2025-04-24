using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTest.Tools;
using TextRenderer = OpenTKTest.Tools.TextRenderer;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK.Windowing.Common.Input;

namespace OpenTKTest.Screens
{
    public class ScreenMain : GameWindow
    {
        private readonly string _versionText = "Version 1.01";
        private TextRenderer _textRenderer;
        private Rectangle _versionTextBounds;
        private CustomCursor _defaultCursor;
        private CustomCursor _activeCursor;
        private bool _isOverVersionText;

        public ScreenMain() : base(
            new GameWindowSettings()
            {
                IsMultiThreaded = false,
                RenderFrequency = 60.0,
                UpdateFrequency = 60.0
            },
            new NativeWindowSettings()
            {
                Size = GameTools.GetScreenDimensions(),
                Title = "OpenTK Test",
                WindowState = WindowState.Fullscreen,
                WindowBorder = WindowBorder.Hidden,
                StartFocused = true,
                StartVisible = true,
                IsEventDriven = false,
                API = ContextAPI.OpenGL,
                APIVersion = new Version(4, 1),
                Flags = ContextFlags.Default,
                Profile = ContextProfile.Core,
                AutoLoadBindings = true
            })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            _textRenderer = new TextRenderer("Arial", 14);
            _defaultCursor = new CustomCursor("Assets/Cursors/default-cursor-48.png");
            _activeCursor = new CustomCursor("Assets/Cursors/cursor-active.png");
            CursorState = CursorState.Hidden;

            // Вычисляем размеры текста для определения области клика
            using (var bitmap = new Bitmap(1, 1))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var size = graphics.MeasureString(_versionText, new Font("Arial", 14));
                _versionTextBounds = new Rectangle(5, Size.Y - 50, (int)size.Width, (int)size.Height);
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            var mouseX = (int)e.X;
            var mouseY = (int)e.Y;

            // Проверяем, находится ли курсор в границах текста
            _isOverVersionText = mouseX >= _versionTextBounds.X && 
                mouseX <= _versionTextBounds.X + _versionTextBounds.Width &&
                mouseY >= _versionTextBounds.Y && 
                mouseY <= _versionTextBounds.Y + _versionTextBounds.Height;

            // Конвертируем координаты мыши для OpenGL (Y снизу)
            var currentPosition = new Vector2(mouseX, Size.Y - mouseY);
            _defaultCursor.Update(currentPosition);
            _activeCursor.Update(currentPosition);

            // Отладочный вывод
            Console.WriteLine($"Mouse: ({mouseX}, {mouseY})");
            Console.WriteLine($"Text bounds: X={_versionTextBounds.X}, Y={_versionTextBounds.Y}, W={_versionTextBounds.Width}, H={_versionTextBounds.Height}");
            Console.WriteLine($"IsOver: {_isOverVersionText}");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Рисуем текст в левом нижнем углу
            _textRenderer.RenderText(_versionText, 5, Size.Y - 50);

            // Рисуем соответствующий курсор
            var screenSize = new Vector2(Size.X, Size.Y);
            if (_isOverVersionText)
            {
                _activeCursor.Render(screenSize);
            }
            else
            {
                _defaultCursor.Render(screenSize);
            }

            SwapBuffers();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left && _isOverVersionText)
            {
                TerminateProcess();
            }
        }

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Keys.Escape)
            {
                TerminateProcess();
            }
        }

        private void TerminateProcess()
        {
            Close();
            Process.GetCurrentProcess().Kill();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            _textRenderer.Dispose();
            _defaultCursor?.Dispose();
            _activeCursor?.Dispose();
        }
    }
} 