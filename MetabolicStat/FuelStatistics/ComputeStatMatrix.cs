using System.Data;
using System.Text.RegularExpressions;

namespace MetabolicStat.FuelStatistics;

// Todo: Add feature Sleep statistic values for and glucose
// Todo: Add feature morning statistic value for ketone & GKI

public class ComputeStatMatrix
{
    private DateTime _dateBreaker;

    public ComputeStatMatrix(string fileName)
    {
        FileName = fileName;

        // Kick off date, algorithm syncs with year fraction boundary's at base 12 of year
        // This guarantees that quarters line up with year boundaries for every data set.
        // This ONLY WORKS IF BASE 12 fractions of a year are used for bucket sizes.
        // For example to divide a year into quarters, 1 year = 365.2422 days / 12 months = 30.437 days per month
        // My objective is to create buckets of consistent timespan.  By this logic a quarter is 91.311 days
        // NOTE: there is nothing to stop you from using whatever arbitrary time division you like just be careful that you don't
        // inadvertently eliminate data from the edges of your arbitrary buckets. To most people reading your reports a quarter of 91.3 days looks no
        // different than a quarter of 90 days.  
        // Caveat: 91.311 days / quarter however will spatially spread the time graphing out correctly when it is charted with a spreadsheet such as excel.

        _dateBreaker =
            DateTime.Parse(
                @"01-01-2015 00:00 -0800"); // begin of first year. IFF datebreaker increment is set to a base 12 fraction of a year
                                            // your buckets will align on year/month boundaries
    }

    public string FileName { get; internal set; }

    public IEnumerable<FuelStat> Run(double bucketDays, out int count, out TimeSpan timeSpan)
    {
        var counter = 0;
        var lineNumber = 0;

        var minDate = DateTime.MaxValue.Ticks;
        var maxDate = DateTime.MinValue.Ticks;

        var bucketRange = string.Empty;

        var fuelStatList = new List<FuelStat>();

        long sequenceCheck = 0;

        foreach (var item in PutYourDataInOrder(FileName))
        {
            lineNumber++;

            #region sequence check

            if (sequenceCheck > item.Date.Ticks)
                throw new DataException($"Out of sequence error: Line: {lineNumber}.");

            sequenceCheck = item.Date.Ticks;

            #endregion

            // bucket interval is from (inclusive) -> to (exclusive) to avoid same data in two buckets
            if (item.Date >= _dateBreaker)
            {
                bucketRange = $"{_dateBreaker.ToString("yy/MM/dd")}-";

                while (item.Date >= _dateBreaker)
                {
                    bucketRange = $"{_dateBreaker.ToString("yy/MM/dd")}-";
                    // creating bucket sizes that are fractions of year, in days.  year = 12 months, month = 30.4 days, and sub sequent halves progression
                    // days: 486.9896  (1.33) 243.4948 (0.66) 121.7474 (1/3) 91.31058 (1/4) 60.87373 (1/6) 30.43685 (1/12) 15.218425 (1/24) 7.6092125 (1/48) 3.80460625 (1/96) 1.9023031 (1/192) 0.95114625 (1/384)  0.4755757813 (1/768)
                    _dateBreaker = _dateBreaker.AddDays(bucketDays);
                }

                bucketRange += $"{_dateBreaker.ToString("yy/MM/dd")}";
            }

            // Separate by Type
            // transform type names, add time component to type

            string targetBucket;

            if (Regex.IsMatch(item.Type, @"^Gluco"))
            {
                targetBucket = item.Value < 35 ? "CGM_err" : $"CGM-{bucketRange}";
            }
            else if (Regex.IsMatch(item.Type, @"^BloodGluco"))
            {
                // TODO: Add a json configuration file for user configurable runtime params.
                targetBucket = item.Value is < 220 and >= 35 ? $"BG-{bucketRange}" : "BG_err";
            }
            else if (Regex.IsMatch(item.Type, @"^BloodKetone"))
            {
                targetBucket = $"BK-{bucketRange}";
            }
            else
            {
                Console.WriteLine($"Skipped item: {item}");
                continue; // data row is ignored 
            }

            if (fuelStatList.Find(x => x.Name.Equals(targetBucket)) == null)
            {
                fuelStatList.Add(new FuelStat(targetBucket, item.Value, item.Date.Ticks));
            }
            else
            {
                var fuelStat = fuelStatList.FirstOrDefault(x => x.Name.Equals(targetBucket));
                fuelStat!.Add(item.Value, item.Date.Ticks);
            }

            minDate = Math.Min(item.Date.Ticks, minDate);
            maxDate = Math.Max(item.Date.Ticks, maxDate);

            counter++;
        }

        count = counter;
        timeSpan = TimeSpan.FromTicks(maxDate - minDate);

        return fuelStatList;
    }

    private static IEnumerable<DataPoint> PutYourDataInOrder(string fileName)
    {
        var result = new List<DataPoint>();

        Regex dateMatch = new(@",(\d+-\d\d-\d\d \d\d:\d\d:\d\d (-0\d00)),", RegexOptions.Compiled);
        Regex typeMatch = new(@"^(Gluco\w+|Blood\w+),(\d+\.\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        foreach (var line in File.ReadLines(fileName))
        {
            //  Does it have a date?
            //  AND does it have a (glucose || BG || BK)

            var dateString = dateMatch.Match(line);
            if (!dateString.Success) continue;

            // Find type and value stat group and y value
            var typeString = typeMatch.Match(line);
            if (!typeString.Success) continue;


            var date = DateTime.Parse(dateString.Groups[1].Value);
            var value = double.Parse(typeString.Groups[2].Value);
            var type = typeString.Groups[1].Value;

            result.Add(new DataPoint(date, type, value));
        }
        return result.OrderBy(dp => dp.Date);
    }

    public class DataPoint
    {
        public DataPoint(DateTime date, string type, double value)
        {
            Date = date;
            Value = value;
            Type = type;
        }
        public DateTime Date { get; }
        public string Type { get; }
        public double Value { get; }
    }
}