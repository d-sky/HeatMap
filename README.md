# HeatMap
The class for heat (temperature) maps generation. It can be used for overlays on maps for example. [Live example](https://квартиры-домики.рф/Карта-цен).



# Using:
```C#
    class Program
    {
        static void Main(string[] args)
        {
            //Test points with values from -100 to 100
            List<PointWithValue> pnts = new List<PointWithValue>();
            pnts.Add(new PointWithValue() { x = 50, y = 50, value = -100 });
            pnts.Add(new PointWithValue() { x = 250, y = 25, value = 45 });
            pnts.Add(new PointWithValue() { x = 450, y = 125, value = 10 });
            pnts.Add(new PointWithValue() { x = 25, y = 300, value = -50 });
            pnts.Add(new PointWithValue() { x = 250, y = 350, value = 0 });
            pnts.Add(new PointWithValue() { x = 410, y = 410, value = 100 });


            HeatMap hm = new HeatMap();
            hm.Points = pnts;
            hm.Width = 500;
            hm.Height = 500;
            var bitmap = hm.Draw();
            bitmap.Save("TestMapResult.png");
        }
    }
```

Test points used in example:

![Test Points](https://raw.githubusercontent.com/d-sky/HeatMap/master/TestMapPoints.png)

The result:

![Result](https://raw.githubusercontent.com/d-sky/HeatMap/master/TestMapResult.png)
