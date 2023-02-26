namespace Disciples.Engine
{
    public static class GameInfo
    {
        public const int OriginalWidth = 800;

        public const int OriginalHeight = 600;


        //public static int Width { get; } = OriginalWidth;

        //public static int Height { get; } = OriginalHeight;

        public static double Width { get; } = 1440;

        public static double Height { get; } = 1080;


        public static double OffsetX = (1920 - Width) / 2;

        public static double OffsetY = (1080 - Height) / 2;


        public static double Scale { get; } = (double) Height / OriginalHeight;
    }
}
