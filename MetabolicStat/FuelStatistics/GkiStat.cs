using MetabolicStat.StatMath;

namespace MetabolicStat.FuelStatistics;

public class GkiStat : IFuelStat
{

    // save mmol switch for other future calculations
    public bool IsGlucoseMmol { get; set; }

    public GkiStat(Statistic glucose, Statistic ketone, string name, bool useGlucoseMmol = false)
    {
        IsGlucoseMmol = useGlucoseMmol;

        var conversion = IsGlucoseMmol ? 1 : 18;
        Name = name;

        MeanX = glucose.MeanX() / conversion / ketone.MeanX();

        MinX = glucose.MinX / conversion / ketone.MinX;

        MaxX = glucose.MaxX / conversion / ketone.MaxX;


        MeanY = (glucose.MeanY() + ketone.MeanY()) / 2;

        MaxY = Math.Max(glucose.MaxY, ketone.MaxY);

        MinY = Math.Min(glucose.MinY, ketone.MinY);

        // Wild guess at variance, I believe this is incorrect

        Qx2 = glucose.Qx2() / conversion / ketone.Qx2();

        try
        {
            Qx = glucose.Qx() / conversion / ketone.Qx();
        }
        catch (InvalidOperationException)
        {
            Qx = 0;
        }

        // Save glucose and ketone stat for reporting

        KetoneStat = ketone;
        GlucoseStat = glucose;
    }

    public double Qx { get; set; }
    public double Qx2 { get; }
    public Statistic KetoneStat { get; internal set; }
    public Statistic GlucoseStat { get; internal set; }
    public double MeanX { get; set; }
    public double MeanY { get; set; }
    public DateTime FromDateTime => new((long)MinY * TimeSpan.TicksPerDay);

    public bool IsNaN => GlucoseStat.IsNaN || KetoneStat.IsNaN;

    public DateTime ToDateTime => new((long)MaxY * TimeSpan.TicksPerDay);
    public string Name { get; set; }
    public string DateRange => $"{FromDateTime} -- {ToDateTime}";
    public TimeSpan TimeSpan => ToDateTime - FromDateTime;
    /// <summary>
    ///     Total The number of total glucose samples divided by the number of ketone samples ... debate please
    /// </summary>
    public double N => KetoneStat.N;
    public double MaxX { get; set; } // max value
    public double MinX { get; set; } // min value
    public double MaxY { get; set; } // end date
    public double MinY { get; set; } // start date
    double IFuelStat.MeanX()
    {
        return MeanX;
    }
    double IFuelStat.MeanY()
    {
        return MeanY;
    }

    #region Report
    public override string ToString()
    {
        // note: divided  ticks / day  by slope, ie value/t to  get slope/day or slope / month if mul by 30
        return
            $"{FromDateTime.ToShortDateString()},{Name}"
            + $",{MeanX:F2},{MinX:F2},{MaxX:F2},{Math.Sqrt(Qx2):F4},{Qx:F4},{N}" //GKI
            + $",{GlucoseStat.MeanX():F2},{GlucoseStat.MinX:F2},{GlucoseStat.MaxX:F2},{Math.Sqrt(GlucoseStat.Qx2()):F4},{GlucoseStat.Qx():F4},{GlucoseStat.N}" // GLU
            + $",{KetoneStat.MeanX():F2},{KetoneStat.MinX:F2},{KetoneStat.MaxX:F2},{Math.Sqrt(KetoneStat.Qx2()):F4},{KetoneStat.Qx():F4},{KetoneStat.N}" //BK
            + $",{GlucoseStat.Qx() / 18 / KetoneStat.Qx():F4}"
            + $",{GlucoseStat.MeanX() / 18:F4}";
    }

    // Keep the Header grouped with the ToString method for ease of maintenance please
    public static string Header => "YminDate,name"
                                   + ",MeanXgki,MinXgki,MaxXgki,Sqrt(Qx2)gki,QxGki,Ngki"
                                   + ",MeanXglu,MinXglu,MaxXglu,Sqrt(Qx2)glu,QxGlu,Nglu"
                                   + ",MeanXbk,MinXbk,MaxXbk,sqrt(Qx2)bk,QxBk,Nbk"
                                   + ",QxGlu/QxBk"
                                   + ",Glu/18"
                                   ;

    #endregion
}