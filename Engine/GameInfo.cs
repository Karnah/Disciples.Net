namespace Engine
{
    public static class GameInfo
    {
        public static double Scale = 1080.0 / 600.0;

        public static (double X, double Y) OffsetCoordinates(double x, double y)
        {
            double newX, newY;

            newX = (-340 + 95 * x + 123 * y) * Scale;
            newY = (-230 + 60 * x - 43 * y) * Scale;

            return (newX, newY);
        }
    }
}
