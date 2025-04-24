using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKTest.Screens
{
    public class ScreenMain : GameWindow
    {
        public ScreenMain() : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Size = Tools.GetScreenDimensions(),
                Title = "OpenTK Test",
                WindowState = WindowState.Fullscreen,
                WindowBorder = WindowBorder.Hidden
            })
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            SwapBuffers();
        }
    }
} 