using Unity.Mathematics;

static class Utils
{
    public static int RoundAwayFromZero(float value)
    {
        if (value > 0)
            return (int) math.ceil(value);
        else
            return (int) math.floor(value);
    }
}
