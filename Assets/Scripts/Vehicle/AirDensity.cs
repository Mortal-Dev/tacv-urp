using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;
using UnityEngine;

public readonly partial struct AirDensity
{
    private static readonly FixedList512Bytes<float> altitudes = new FixedList512Bytes<float>()
    {
        -1000,
        0,
        1000,
        2000,
        4000,
        5000,
        6000,
        7000,
        8000,
        9000,
        10000,
        15000,
        20000,
        25000,
        30000,
        40000,
        50000,
        60000,
        70000,
        80000
    };

    private static readonly FixedList512Bytes<float> airDensity = new FixedList512Bytes<float>()
    {
        1.347f,
        1.225f,
        1.007f,
        0.9093f,
        0.8194f,
        0.7364f,
        0.6601f,
        0.5900f,
        0.5258f,
        0.4671f,
        0.4135f,
        0.1948f,
        0.08891f,
        0.04008f,
        0.01841f,
        0.003996f,
        0.001027f,
        0.0003097f,
        0.00008283f,
        0.00001846f
    };

    public static float GetAirDensityFromFeet(float altitudeFeet)
    {
        return GetAirDensityFromMeters(altitudeFeet / 3.2808399f);
    }

    public static float GetAirDensityFromMeters(float altitudeMeters)
    {
        KeyValuePair<float, float> upperAirAltitudeDensity = new(float.NegativeInfinity, float.NegativeInfinity);
        KeyValuePair<float, float> lowerAirAltitudeDensity = new(float.NegativeInfinity, float.NegativeInfinity);

        for (int i = altitudes.Length - 1; i >= 0; i--)
        {
            if (altitudeMeters >= altitudes[i])
            {
                lowerAirAltitudeDensity = new KeyValuePair<float, float>(altitudes[i], airDensity[i]);
                break;
            }

            upperAirAltitudeDensity = new KeyValuePair<float, float>(altitudes[i], airDensity[i]);
        }

        return math.lerp(upperAirAltitudeDensity.Value, lowerAirAltitudeDensity.Value, (upperAirAltitudeDensity.Key - altitudeMeters) / (upperAirAltitudeDensity.Key - lowerAirAltitudeDensity.Key));
    }
}