using OpenTK.Mathematics;

namespace OpenTKTest.Tools
{
    public static class GameTools
    {
        public static Vector2i GetScreenDimensions()
        {
            // Get primary screen dimensions
            var screen = Screen.PrimaryScreen;
            return new Vector2i(screen.Bounds.Width, screen.Bounds.Height);
        }
    }
}