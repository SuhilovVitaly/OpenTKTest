using OpenTK.Mathematics;

namespace OpenTKTest
{
    public static class Tools
    {
        public static Vector2i GetScreenDimensions()
        {
            // Get primary screen dimensions
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            return new Vector2i(screen.Bounds.Width, screen.Bounds.Height);
        }
    }
} 