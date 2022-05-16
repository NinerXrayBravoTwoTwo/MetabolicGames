namespace MetabolicStat.Program;

public interface IFuelStat
{
    string Name { get; set; }
    string DateRange { get; }
    TimeSpan TimeSpan { get; }

    /// <summary>
    ///     Number of Samples for this statistic.
    /// </summary>
    double N { get;}

    /// <summary>
    ///     Maximum Value of X (note; this is not reversed in Dec method)
    /// </summary>
    double MaxX { get; set; }

    /// <summary>
    ///     Minimum value of X (note; this is not reversed in Dec method)
    /// </summary>
    double MinX { get; set; }

    /// <summary>
    ///     Maximum Value of Y (nte; this is not reversed in Dec method)
    /// </summary>
    double MaxY { get; set; }

    /// <summary>
    ///     Minimum value of Y (note; this is not reversed in Dec method)
    /// </summary>
    double MinY { get; set; }

    DateTime ToDateTime { get; }

    DateTime FromDateTime { get; }

    /// <summary>
    ///     mean x = sum(x) / n
    /// </summary>
    /// <returns>Mean of x</returns>
    double MeanX();

    /// <summary>
    ///     mean y = sum(y) / n
    /// </summary>
    /// <returns>Mean of y</returns>
    double MeanY();
}