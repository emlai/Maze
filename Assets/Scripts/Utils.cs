using Unity.Mathematics;

static class Utils
{
    public static int RoundTowardsZero(float value)
    {
        if (value > 0)
            return (int) math.floor(value);
        else
            return (int) math.ceil(value);
    }

    public static int RoundAwayFromZero(float value)
    {
        if (value > 0)
            return (int) math.ceil(value);
        else
            return (int) math.floor(value);
    }

    public static bool IsBetween(this int value, int a, int b)
    {
        if (a < b)
            return value > a && value < b;
        else
            return value > b && value < a;
    }
}
