using Unity.Mathematics;

public static class MathHelper
{
    public static float Magnitude(this float3 value) { return (float)math.sqrt(value.x * value.x + value.y * value.y + value.z * value.z); }

    public static float3 Normalize(this float3 value)
    {
        float mag = value.Magnitude();
        if (mag > 0.00001F)
            return value / mag;
        else
            return float3.zero;
    }
}