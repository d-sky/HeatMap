using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

public class HeatMap
{
    public List<PointWithValue> Points;
    public List<RGB> Colors;
    public HeatMapLimits Limits;
    public int Width;
    public int Height;
    public double ColorLimit = 0.35;
    public int NumberOfLevels = 20;
    public int MinValue = -100;
    public int MaxValue = 100;
    public bool LevelsEnabled = false;
    protected bool useDefaultColors = true;
    public bool UseDefaultColors
    {
        get { return useDefaultColors; }
        set
        {
            useDefaultColors = value;
            if (value) SetDefaultColors();
            else Colors = null;
        }
    }

    public HeatMap()
    {
        Points = new List<PointWithValue>();
        Limits = new HeatMapLimits { xMin = 0, xMax = 0, yMin = 0, yMax = 0 };
        SetDefaultColors();
    }

    protected void SetDefaultColors()
    {
        Colors = new List<RGB>();
        Colors.Add(new RGB(85, 78, 177));
        Colors.Add(new RGB(67, 105, 196));
        Colors.Add(new RGB(64, 160, 180));
        Colors.Add(new RGB(78, 194, 98));
        Colors.Add(new RGB(108, 209, 80));
        Colors.Add(new RGB(190, 228, 61));
        Colors.Add(new RGB(235, 224, 53));
        Colors.Add(new RGB(234, 185, 57));
        Colors.Add(new RGB(233, 143, 67));
        Colors.Add(new RGB(225, 94, 93));
        Colors.Add(new RGB(147, 23, 78));
        Colors.Add(new RGB(114, 22, 56));
        Colors.Add(new RGB(84, 16, 41));
        Colors.Add(new RGB(43, 0, 1));
    }

    int CrossProduct(HeatMapPoint o, HeatMapPoint a, HeatMapPoint b)
    {
        return (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }

    int SquareDistance(HeatMapPoint p0, HeatMapPoint p1)
    {
        int x = p0.X - p1.X,
            y = p0.Y - p1.Y;

        return x * x + y * y;
    }

    double HUE2RGB(double p, double q, double t)
    {
        if (t < 0)
        {
            t += 1;
        }
        else if (t > 1)
        {
            t -= 1;
        }

        if (t >= 0.66)
        {
            return p;
        }
        else if (t >= 0.5)
        {
            return p + (q - p) * (0.66 - t) * 6;
        }
        else if (t >= 0.33)
        {
            return q;
        }
        else
        {
            return p + (q - p) * 6 * t;
        }
    }

    RGB HSL2RGB(double h, double s, double l, double a = 1)
    {
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = HUE2RGB(p, q, h + 0.333);
            g = HUE2RGB(p, q, h);
            b = HUE2RGB(p, q, h - 0.333);
        }
        var Result = new RGB((int)(r * 255) | 0, (int)(g * 255) | 0, (int)(b * 255) | 0, (int)(a * 255));
        if (Result.R > 255) Result.R = 255;
        if (Result.G > 255) Result.G = 255;
        if (Result.B > 255) Result.B = 255;
        if (Result.A > 255) Result.A = 255;

        return Result;
    }

    public HSL GetHSLColor(double value)
    {
        double temperature = 0,
            diff = MaxValue - MinValue;

        if (value < MinValue)
        {
            value = MinValue;
        }
        if (value > MaxValue)
        {
            value = MaxValue;
        }

        temperature = 1 - (1 - ColorLimit) - (((value - MinValue) * ColorLimit) / diff);

        if (LevelsEnabled)
        {
            temperature = Math.Round(temperature * NumberOfLevels) / NumberOfLevels;
        }

        double l = 0.5;
        double s = 1;
        double a = 1;

        return new HSL() { H = temperature, S = s, L = l, A = a };
    }

    public RGB GetRGBColor(double value)
    {
        if (Colors == null)
        {
            var hsl = GetHSLColor(value);

            return HSL2RGB(hsl.H, hsl.S, hsl.L, hsl.A);
        }
        else
        {
            var v = value < 0 ? Math.Abs(MinValue) - Math.Abs(value) : value + Math.Abs(MaxValue);
            var maxV = Math.Abs(MaxValue) + Math.Abs(MinValue);
            int index = (int)(v * (Colors.Count) / maxV);
            if (index == Colors.Count) index--;
            return Colors[index];
        }
    }

    double GetPointValue(int limit, PointWithValue point)
    {
        List<Tuple<int, int>> pointsWithDistance = new List<Tuple<int, int>>();
        int distance = 0;
        double inv = 0.0,
                t = 0.0,
                b = 0.0;

        for (int i = 0; i < this.Points.Count; i++)
        {
            distance = SquareDistance(point, this.Points[i]);
            if (distance == 0)
            {
                return this.Points[i].Value;
            }
            pointsWithDistance.Insert(i, new Tuple<int, int>(distance, i));
        }

        pointsWithDistance = pointsWithDistance.OrderBy(a => a.Item1).ToList();

        for (int i = 0; i < limit; i++)
        {
            var P = pointsWithDistance[i];
            inv = 1 / Math.Pow(P.Item1, 2);
            t = t + inv * this.Points[P.Item2].Value;
            b = b + inv;
        }

        return t / b;
    }

    public Bitmap Draw()
    {
        Bitmap result = new Bitmap(Width, Height);

        if (Limits.xMax == 0 && Limits.xMin == 0 && Limits.yMax == 0 && Limits.yMin == 0)
        {
            this.Limits = new HeatMapLimits { xMin = 0, xMax = Width, yMin = 0, yMax = Height };
        }

        int x = Limits.xMin;
        int y = Limits.yMin;
        int w = Width;
        int wy = w * y;
        int pointsLimit = Points.Count;
        int xStart = Limits.xMin;
        int xEnd = Limits.xMax;
        int yEnd = Limits.yMax;
        bool isEmpty = true;

        while (y < yEnd)
        {
            var val = GetPointValue(pointsLimit, new PointWithValue() { X = x, Y = y });

            if (val != -255)
            {
                isEmpty = false;

                int imgX = x - Limits.xMin;
                int imgY = y - Limits.yMin;

                if (imgX < Width && imgY < Height && imgX >= 0 && imgY >= 0)
                {
                    var col = GetRGBColor(val);

                    var imgCol = Color.FromArgb(col.A, col.R, col.G, col.B);

                    result.SetPixel(imgX, imgY, imgCol);
                }
            }
            x = x + 1;
            if (x > xEnd)
            {
                x = xStart;
                y = y + 1;
                wy = w * y;
            }
        }
        if (isEmpty) return null;

        return result;
    }
}

public class HeatMapPoint
{
    public int X;
    public int Y;
}

public class PointWithValue : HeatMapPoint
{
    public double Value;
}

public class HeatMapLimits
{
    public int xMin;
    public int xMax;
    public int yMin;
    public int yMax;
}

public class RGB
{
    public RGB(int r, int g, int b, int a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public int R;
    public int G;
    public int B;
    public int A;
}

public class HSL
{
    public double H;
    public double S;
    public double L;
    public double A;
}
