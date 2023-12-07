using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public struct XYAxisInterpolator
{
    private FixedList128Bytes<Point> points;

    public XYAxisInterpolator(FixedList128Bytes<Point> points)
    {
        this.points = points;
    }

    public void AddPoint(double x, double y)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].X < x) points.Insert(i, new Point(x, y));
        }
    }

    public void RemovePoint(int index)
    {
        if (index >= 0 && index < points.Length)
        {
            points.RemoveAt(index);
        }
        else
        {
            throw new IndexOutOfRangeException("Index is out of range.");
        }
    }

    public double GetYValue(double x)
    {
        if (points.Length == 0)
        {
            throw new InvalidOperationException("No points have been added.");
        }
        if (points.Length == 1)
        {
            return points[0].Y;
        }

        Point lowerPoint = points[0];
        Point upperPoint = points[^1];

        if (x <= lowerPoint.X)
        {
            return lowerPoint.Y;
        }
        if (x >= upperPoint.X)
        {
            return upperPoint.Y;
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            if (x >= points[i].X && x <= points[i + 1].X)
            {
                double t = (x - points[i].X) / (points[i + 1].X - points[i].X);
                return Lerp(points[i].Y, points[i + 1].Y, t);
            }
        }

        return 0; // This should never happen unless there's an issue with the input points.
    }

    private double Lerp(double a, double b, double t)
    {
        return a + t * (b - a);
    }

    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}