using MetabolicStat.StatMath;

namespace MetabolicStat.FuelStatistics;

public class GkiStat : IFuelStat
{
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


        if (!glucose.IsNaN || !ketone.IsNaN)
        {
            Qx2 = glucose.Qx2() / conversion / ketone.Qx2();
            Qx = glucose.Qx() / conversion / ketone.Qx();
        }
        else
        {
            Qx2 = 0;
            Qx = 0;
        }

        // Save glucose and ketone gkiStat for reporting

        KetoneStat = ketone;
        GlucoseStat = glucose;
    }

    // save mmol switch for other future calculations
    public bool IsGlucoseMmol { get; set; }

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
        string result;
        try
        {
            result = !GlucoseStat.IsNaN && !KetoneStat.IsNaN
                ? $"{FromDateTime.ToShortDateString()},{Name}"
                  + $",{MeanX:F2},{MinX:F2},{MaxX:F2},{Math.Sqrt(Qx2):F4},{Qx:F4},{N}" //GKI
                  + $",{GlucoseStat.MeanX():F2},{GlucoseStat.MinX:F2},{GlucoseStat.MaxX:F2},{Math.Sqrt(GlucoseStat.Qx2()):F4},{GlucoseStat.Qx():F4},{GlucoseStat.N}" // GLU
                  + $",{KetoneStat.MeanX():F2},{KetoneStat.MinX:F2},{KetoneStat.MaxX:F2},{Math.Sqrt(KetoneStat.Qx2()):F4},{KetoneStat.Qx():F4},{KetoneStat.N}" //BK
                  + $",{GlucoseStat.Qx() / 18 / KetoneStat.Qx():F4}"
                  + $",{GlucoseStat.MeanX() / 18:F4}"
                : $",{Name}, Glucose:{GlucoseStat.N}, Ketone: {KetoneStat.N}";
        }
        catch (Exception error)
        {
            result = error.GetType().ToString();
        }

        return result;
    }

    // Keep the Header grouped with the ToString method for ease of maintenance please
    public static string Header => "YminDate,name"
                                   + ",MeanXgki,MinXgki,MaxXgki,Sqrt(Qx2)gki,QxGki,Ngki"
                                   + ",MeanXglu,MinXglu,MaxXglu,Sqrt(Qx2)glu,QxGlu,Nglu"
                                   + ",MeanXbk,MinXbk,MaxXbk,sqrt(Qx2)bk,QxBk,Nbk"
                                   + ",QxGlu/QxBk"
                                   + ",Glu/18";

      public static string Footer(Statistic gkiStat, Statistic gluStat, Statistic ketStat, double gkiSamples, double gluSamples, double ketSamples)
    {
        //var midBucket = new DateTime((long)gkiStat.MeanY() );
        var minDate = new DateTime((long)gkiStat.MinY);
        var maxDate = new DateTime((long)gkiStat.MaxY);
        return
            $"ttlGki,{minDate.ToShortDateString()}-{maxDate.ToShortDateString()},{gkiStat.MeanX():F2},{gkiStat.MinX:F3},{gkiStat.MaxX:F3},{Math.Sqrt(gkiStat.Qx2()):F3},{gkiStat.Qx():F3},{gkiStat.N:F0}"
            + $"\r\nttlGlu,{gluStat.MeanX():F3},{gluStat.MinX:F3},{gluStat.MaxX:F3},{gluStat.MaxX:F3},{Math.Sqrt(gluStat.Qx2()):F3},{gluStat.Qx():F3},{gluSamples:F0}"
            + $"\r\nttlKet,{ketStat.MeanX():F3},{ketStat.MinX:F3},{ketStat.MaxX:F3},{ketStat.MaxX:F3},{Math.Sqrt(ketStat.Qx2()):F3},{ketStat.Qx():F3},{ketSamples:F0}";
    }

    #endregion
}