using OpenTKTest.Screens;

namespace OpenTKTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var window = new ScreenMain();
            window.Run();
        }
    }
} 