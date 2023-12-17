using Unity.Physics;

public static class PhysicsMassExtensions
{
    public static float GetMass(this PhysicsMass physicsMass)
    {
        return 1.0f / physicsMass.InverseMass;
    }
}