﻿namespace Disciples.Engine
{
    public static class GameInfo
    {
        public const int OriginalWidth = 800;

        public const int OriginalHeight = 600;


        //public static int Width { get; } = OriginalWidth;

        //public static int Height { get; } = OriginalHeight;

        public static int Width { get; } = 1440;

        public static int Height { get; } = 1080;


        public static double Scale { get; } = (double) Height / OriginalHeight;
    }
}