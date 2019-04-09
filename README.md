# HeatMap
The class for heat (temperature) maps generation. It can be used for overlays on maps for example. [Live example](https://квартиры-домики.рф/Карта-цен).



# Using:
```C#
//Test points with values from -100 to 100
List<PointWithValue> pnts = new List<PointWithValue>();
pnts.Add(new PointWithValue() { X = 50, Y = 50, Value = -100 });
pnts.Add(new PointWithValue() { X = 250, Y = 25, Value = 45 });
pnts.Add(new PointWithValue() { X = 450, Y = 125, Value = 10 });
pnts.Add(new PointWithValue() { X = 25, Y = 300, Value = -50 });
pnts.Add(new PointWithValue() { X = 250, Y = 350, Value = 0 });
pnts.Add(new PointWithValue() { X = 410, Y = 410, Value = 100 });

HeatMap hm = new HeatMap();
hm.Points = pnts;
hm.Width = 500;
hm.Height = 500;
var bitmap = hm.Draw();
bitmap.Save("TestMapResult.png");
```

Test points used in example:

![Test Points](https://raw.githubusercontent.com/d-sky/HeatMap/master/TestMapPoints.png)

The result:

![Result](https://raw.githubusercontent.com/d-sky/HeatMap/master/TestMapResult.png)
