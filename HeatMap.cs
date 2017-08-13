using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

public class HeatMap
{
    public List<PointWithValue> Points;
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
    public List<RGB> Colors;

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
        return (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }

    bool IsPointInPolygon(HeatMapPoint point, List<HeatMapPoint> vs)
    {
        int x = point.x,
            y = point.y;
        bool inside = false;
        int i = 0,
            j = 0,
            xi = 0,
            xj = 0,
            yi = 0,
            yj = 0;
        bool intersect = false;

        j = vs.Count - 1;
        for (i = 0; i < vs.Count; i = i + 1)
        {
            xi = vs[i].x;
            yi = vs[i].y;
            xj = vs[j].x;
            yj = vs[j].y;

            intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
            if (intersect) { inside = !inside; }
            j = i;
        }

        return inside;
    }

    int SquareDistance(HeatMapPoint p0, HeatMapPoint p1)
    {
        int x = p0.x - p1.x,
            y = p0.y - p1.y;

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
        double r, g, b, q, p;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            p = 2 * l - q;
            r = HUE2RGB(p, q, h + 0.333);
            g = HUE2RGB(p, q, h);
            b = HUE2RGB(p, q, h - 0.333);
        }
        var result = new RGB((int)(r * 255) | 0, (int)(g * 255) | 0, (int)(b * 255) | 0, (int)(a * 255)); // (x << 0) = Math.floor(x)
        if (result.r > 255) result.r = 255;
        if (result.g > 255) result.g = 255;
        if (result.b > 255) result.b = 255;
        if (result.a > 255) result.a = 255;
        return result;
    }

    public HSL GetHSLColor(double value)
    {
        double val = value,
            tmp = 0,
            lim = ColorLimit,//0.35,
            min = MinValue,
            max = MaxValue,
            dif = max - min,
            lvs = NumberOfLevels;

        if (val < min)
        {
            val = min;
        }
        if (val > max)
        {
            val = max;
        }

        tmp = 1 - (1 - lim) - (((val - min) * lim) / dif);

        double a = 1;

        if (LevelsEnabled)
        {
            tmp = Math.Round(tmp * lvs) / lvs;
        }

        double l = 0.5;
        double s = 1;

        return new HSL() { h = tmp, s = s, l = l, a = a };
    }

    public RGB GetRGBColor(double value)
    {
        if (Colors == null)
        {
            var hsl = GetHSLColor(value);

            return HSL2RGB(hsl.h, hsl.s, hsl.l, hsl.a);
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
        int counter = 0;
        List<Tuple<int, int>> arr = new List<Tuple<int, int>>();
        int dis = 0;
        double inv = 0.0,
                t = 0.0,
                b = 0.0;
        int pwr = 2;
        Tuple<int, int> ptr;


        for (counter = 0; counter < this.Points.Count; counter = counter + 1)
        {
            dis = SquareDistance(point, this.Points[counter]);
            if (dis == 0)
            {
                return this.Points[counter].value;
            }
            arr.Insert(counter, new Tuple<int, int>(dis, counter));
        }

        arr = arr.OrderBy(a => a.Item1).ToList();

        for (counter = 0; counter < limit; counter = counter + 1)
        {
            ptr = arr[counter];
            inv = 1 / Math.Pow(ptr.Item1, pwr);
            t = t + inv * this.Points[ptr.Item2].value;
            b = b + inv;
        }

        return t / b;

    }

    public Bitmap Draw()
    {
        Bitmap img = new Bitmap(this.Width, this.Height);

        if (Limits.xMax == 0 && Limits.xMin == 0 && Limits.yMax == 0 && Limits.yMin == 0)
        {
            this.Limits = new HeatMapLimits { xMin = 0, xMax = this.Width, yMin = 0, yMax = this.Height };
        }

        int x = Limits.xMin;
        int y = Limits.yMin;
        int w = Width;
        int wy = w * y;
        int lim = Points.Count;
        double val = 0.0;
        int xBeg = Limits.xMin;
        int xEnd = Limits.xMax;
        int yEnd = Limits.yMax;
        bool isEmpty = true;

        while (y < yEnd)
        {
            val = GetPointValue(lim, new PointWithValue() { x = x, y = y });

            if (val != -255)
            {
                isEmpty = false;

                int imgX = x - Limits.xMin;
                int imgY = y - Limits.yMin;

                if (imgX < Width && imgY < Height && imgX >= 0 && imgY >= 0)
                {
                    var col = GetRGBColor(val);

                    var imgCol = Color.FromArgb(col.a, col.r, col.g, col.b);

                    img.SetPixel(imgX, imgY, imgCol);
                }
            }
            x = x + 1;
            if (x > xEnd)
            {
                x = xBeg;
                y = y + 1;
                wy = w * y;
            }
        }
        if (isEmpty) return null;
        return img;
    }
}

public class HeatMapPoint
{
    public int x;
    public int y;
}

public class PointWithValue : HeatMapPoint
{
    public double value;
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
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
    }

    public int r;
    public int g;
    public int b;
    public int a;
}

public class HSL
{
    public double h;
    public double s;
    public double l;
    public double a;
}
