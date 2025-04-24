using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenTKTest.Tools
{
    /// <summary>
    /// Provides common game utilities
    /// </summary>
    public static class GameTools
    {
        /// <summary>
        /// Gets the primary monitor dimensions
        /// </summary>
        /// <returns>Screen dimensions as Vector2i</returns>
        public static Vector2i GetScreenDimensions()
        {
            // Получаем размеры основного монитора для кроссплатформенной работы
            var primaryMonitor = Monitors.GetPrimaryMonitor();
            var screenSize = primaryMonitor.ClientArea.Size;
            
            return new Vector2i(screenSize.X, screenSize.Y);
        }
    }
}