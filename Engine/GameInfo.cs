namespace Inftastructure
{
    public static class GameInfo
    {
        public static double Scale = 1080.0 / 600.0;

        public static (double X, double Y) OffsetCoordinates(double x, double y)
        {
            double newX, newY;

            newX = -290 + 175 * x + 220 * y;
            newY = -420 + 125 * x - 75 * y;

            return (newX, newY);
        }
    }
}
