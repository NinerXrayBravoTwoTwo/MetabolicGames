using MetabolicStat.StatMath;

namespace MetabolicStat.FuelStatistics;

public class FuelStat : Statistic, IFuelStat
{
    public FuelStat(string name)
    {
        Name = name;
    }

    public FuelStat(string name, double x, double y) : base(x, y / TimeSpan.TicksPerDay)
    {
        Name = name;
    }

    public FuelStat(string name, FuelStat cloneMe) : base(cloneMe)
    {
        Name = name;
        InterpolatedCount = cloneMe.InterpolatedCount;
    }

    public FuelStat(FuelStat cloneMe) : base(cloneMe)
    {
        Name = cloneMe.Name;
        InterpolatedCount = cloneMe.InterpolatedCount;
    }

    public int InterpolatedCount { get; set; }

    public bool IsInterpolated
    {
        set
        {
            if (value) InterpolatedCount++;
        }
        get => InterpolatedCount > 0;
    }

    public string Name { get; set; }

    public new void Add(double x, double y)
    {
        base.Add(x, y / TimeSpan.TicksPerDay);
    }

    public DateTime FromDateTime => new((long)MinY * TimeSpan.TicksPerDay);

    public DateTime ToDateTime => new((long)MaxY * TimeSpan.TicksPerDay);

    public string DateRange => $"{FromDateTime} -- {ToDateTime}";

    public TimeSpan TimeSpan => ToDateTime - FromDateTime;

    public new string ToString()
    {
        string result;
        try
        {
            var isInfin = double.IsPositiveInfinity(Slope());
            result = isInfin
                ? $"Nan - {Name} - {N}"
                : $"{FromDateTime.ToShortDateString()},{InterpolatedCount},{Name},{MeanX():N3},{MinX},{MaxX},{Qx():N4},{Qy():N4},{Slope():N4},{Qx2():N4},{Math.Sqrt(Qx2()):N4},{N}";
        }
        catch (Exception error)
        {
            result = error.GetType().ToString();
        }

        return result;
    }

    public static string Header => "index,Interpolations,name,meanX,minX,maxX,Qx,Qy,slope,Qx2,sqrt(Qx2),N";
}