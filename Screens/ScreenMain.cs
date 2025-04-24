using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTKTest.Tools;
using TextRenderer = OpenTKTest.Tools.TextRenderer;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using System.Diagnostics;
using OpenTK.Windowing.Common.Input;

namespace OpenTKTest.Screens
{
    public class ScreenMain : GameWindow
    {
        private readonly string _versionText = "Version 1.01";
        private TextRenderer _textRenderer;
        private Box2 _versionTextBounds;
        private CustomCursor _defaultCursor;
        private CustomCursor _activeCursor;
        private bool _isOverVersionText;

        public ScreenMain() : base(
            new GameWindowSettings()
            {
                UpdateFrequency = 60.0
            },
            new NativeWindowSettings()
            {
                Size = GameTools.GetScreenDimensions(),
                Title = "OpenTK Test",
                WindowState = WindowState.Normal,
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
            // Устанавливаем окно в полноэкранный режим после создания
            Location = new Vector2i(0, 0);
            Size = GameTools.GetScreenDimensions();
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            _textRenderer = new TextRenderer("Arial", 14);
            _defaultCursor = new CustomCursor("Assets/Cursors/default-cursor-48.png");
            _activeCursor = new CustomCursor("Assets/Cursors/cursor-active.png");
            CursorState = CursorState.Hidden;

            // Примерные размеры текста (можно настроить)
            float textWidth = _versionText.Length * 8; // примерно 8 пикселей на символ
            float textHeight = 20; // примерная высота для шрифта 14
            _versionTextBounds = new Box2(
                new Vector2(5, Size.Y - 50),
                new Vector2(5 + textWidth, Size.Y - 30)
            );
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            var mouseX = (int)e.X;
            var mouseY = (int)e.Y;

            // Проверяем, находится ли курсор в границах текста
            _isOverVersionText = mouseX >= _versionTextBounds.Min.X && 
                mouseX <= _versionTextBounds.Max.X &&
                mouseY >= _versionTextBounds.Min.Y && 
                mouseY <= _versionTextBounds.Max.Y;

            // Конвертируем координаты мыши для OpenGL (Y снизу)
            var currentPosition = new Vector2(mouseX, Size.Y - mouseY);
            _defaultCursor.Update(currentPosition);
            _activeCursor.Update(currentPosition);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Рисуем текст в левом нижнем углу
            _textRenderer.RenderText(_versionText, (int)_versionTextBounds.Min.X, (int)_versionTextBounds.Min.Y);

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
            Environment.Exit(0);
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