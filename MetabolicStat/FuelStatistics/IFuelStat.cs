namespace MetabolicStat.FuelStatistics;

public interface IFuelStat
{
    /// <summary>
    ///     Statistic bucket name
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     A string representing from date through to date in "yyyy/mm/dd-yyyy-mm/dd"
    ///     This can be changed to any unique name model
    /// </summary>
    string DateRange { get; }

    /// <summary>
    ///     A .Net timespan value containing the interval length of this bucket
    /// </summary>
    TimeSpan TimeSpan { get; }

    /// <summary>
    ///     Number of Samples for this statistic.
    /// </summary>
    double N { get; }

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

    /// <summary>
    ///     To date value. Should be the value of MaxY * ticksPerDay
    /// </summary>
    DateTime ToDateTime { get; }

    /// <summary>
    ///     From date value.  Should be the value of MinY * ticksPerDay
    /// </summary>
    DateTime FromDateTime { get; }

    /// <summary>
    ///     Tests if there are enough samples for a valid linear regression
    /// </summary>
    bool IsNaN { get; }

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