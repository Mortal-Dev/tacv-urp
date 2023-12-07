using System;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

public struct HighFidelityFixedAnimationCurve
{
	public int Resolution { get; private set; }

	private FixedList4096Bytes<float> values;
	private WrapMode preWrapMode;
	private WrapMode postWrapMode;

	public void SetCurve(AnimationCurve curve, int resolution = 1023)
	{
		if (curve == null)
			throw new NullReferenceException("Animation curve is null.");

		values = new FixedList4096Bytes<float>();

		preWrapMode = curve.preWrapMode;
		postWrapMode = curve.postWrapMode;

		this.Resolution = resolution;

		for (int i = 0; i < resolution; i++)
			values.Add(curve.Evaluate((float)i / (float)resolution));
	}

	public float Evaluate(float t)
	{
		var count = values.Length;

		if (count == 1)
			return values[0];

		if (t <= 0f)
		{
			switch (preWrapMode)
			{
				default:
					return values[0];
				case WrapMode.Loop:
					t = 1f - (math.abs(t) % 1f);
					break;
				case WrapMode.PingPong:
					t = pingpong(t, 1f);
					break;
			}
		}
		else if (t > 1f)
		{
			switch (postWrapMode)
			{
				default:
					return values[count - 1];
				case WrapMode.Loop:
					t %= 1f;
					break;
				case WrapMode.PingPong:
					t = pingpong(t, 1f);
					break;
			}
		}

		var it = t * (count - 1);

		var lower = (int)it;
		var upper = lower + 1;
		if (upper >= count)
			upper = count - 1;

		return math.lerp(values[lower], values[upper], it - lower);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float repeat(float t, float length)
	{
		return math.clamp(t - math.floor(t / length) * length, 0, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float pingpong(float t, float length)
	{
		t = repeat(t, length * 2f);
		return length - math.abs(t - length);
	}
}

public struct LowFidelityFixedAnimationCurve
{
	public int Resolution { get; private set; }

	private FixedList512Bytes<float> values;
	private WrapMode preWrapMode;
	private WrapMode postWrapMode;

	public void SetCurve(AnimationCurve curve, int resolution = 127)
	{
		if (curve == null)
			throw new NullReferenceException("Animation curve is null.");

		values = new FixedList512Bytes<float>();

		preWrapMode = curve.preWrapMode;
		postWrapMode = curve.postWrapMode;

		this.Resolution = resolution;

		for (int i = 0; i < resolution; i++)
			values.Add(curve.Evaluate((float)i / (float)resolution));
	}

	public float Evaluate(float t)
	{
		var count = values.Length;

		if (count == 1)
			return values[0];

		if (t <= 0f)
		{
			switch (preWrapMode)
			{
				default:
					return values[0];
				case WrapMode.Loop:
					t = 1f - (math.abs(t) % 1f);
					break;
				case WrapMode.PingPong:
					t = pingpong(t, 1f);
					break;
			}
		}
		else if (t > 1f)
		{
			switch (postWrapMode)
			{
				default:
					return values[count - 1];
				case WrapMode.Loop:
					t %= 1f;
					break;
				case WrapMode.PingPong:
					t = pingpong(t, 1f);
					break;
			}
		}

		var it = t * (count - 1);

		var lower = (int)it;
		var upper = lower + 1;
		if (upper >= count)
			upper = count - 1;

		return math.lerp(values[lower], values[upper], it - lower);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float repeat(float t, float length)
	{
		return math.clamp(t - math.floor(t / length) * length, 0, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private float pingpong(float t, float length)
	{
		t = repeat(t, length * 2f);
		return length - math.abs(t - length);
	}
}