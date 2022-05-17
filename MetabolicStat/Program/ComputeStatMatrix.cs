using System.Text.RegularExpressions;

namespace MetabolicStat.Program;

// Todo: Add feature Sleep statistic values for and glucose
// Todo: Add feature morning statistic value for ketone & GKI
// Todo: Add bucketSize parameter

public class ComputeStatMatrix
{
    private DateTime _dateBreaker;

    public ComputeStatMatrix(string fileName)
    {
        FileName = fileName;

        // Kick off date the algorithm syncs with year fraction boundary's base 12 and fractions thereof
        // when year fractions or multiples are chosen for bucket-breaker

        _dateBreaker =
            DateTime.Parse(
                @"01-01-2020 00:00 -0800"); // begin of first year. IFF datebreaker increment is set to a base 12 fraction of a year your buckets will align on year/month bountriess
    }

    public string FileName { get; internal set; }

    public IEnumerable<FuelStat> Run(double bucketDays, out int count, out TimeSpan timeSpan)
    {
        var counter = 0;
        var minDate = DateTime.MaxValue.Ticks;
        var maxDate = DateTime.MinValue.Ticks;

        Regex dateMatch = new(@",(\d+-\d\d-\d\d \d\d:\d\d:\d\d (-0\d00)),", RegexOptions.Compiled);
        Regex sourceMatch = new(@"^(\w+),(\d+\.\d+)", RegexOptions.Compiled);

        var bucketName = string.Empty;

        var fuelStatList = new List<FuelStat>();

        foreach (var line in File.ReadLines(FileName))
        {
            // Find date x value
            var dateString = dateMatch.Match(line);

            if (!dateString.Success) continue;

            // Find source and value stat group and y value
            var sourceString = sourceMatch.Match(line);

            if (!sourceString.Success) continue;

            var date = DateTime.Parse(dateString.Groups[1].Value);

            // bucket interval is from (inclusive) -> to (exclusive) to avoid same data in two buckets
            if (date >= _dateBreaker)
            {
                bucketName = $"{_dateBreaker.ToShortDateString()}-";

                while (date >= _dateBreaker)
                {
                    bucketName = $"{_dateBreaker.ToShortDateString()}-";
                    // creating bucket sizes that are fractions of year, in days.  year = 12 months, month = 30.4 days, and sub sequent halves progression
                    // days: 486.9896  (1.33) 243.4948 (0.66) 121.7474 (1/3) 91.31058 (1/4) 60.87373 (1/6) 30.43685 (1/12) 15.218425 (1/24) 7.6092125 (1/48) 3.80460625 (1/96) 1.9023031 (1/192) 0.95114625 (1/384)  0.4755757813 (1/768)
                    _dateBreaker = _dateBreaker.AddDays(bucketDays);
                }

                bucketName += $"{_dateBreaker.ToShortDateString()}";
            }

            // Separate by Source
            var value = double.Parse(sourceString.Groups[2].Value);
            var source = sourceString.Groups[1].Value;

            // transform source names, add time component to source
            if (Regex.IsMatch(source, @"^Gluco"))
                source = value < 35 ? "CGM_err" : $"CGM-{bucketName}";
            else if (Regex.IsMatch(source, @"^BloodGluco"))
                source = value is < 300 and >= 35 ? $"BG-{bucketName}" : "BG_err";
            else if (Regex.IsMatch(source, @"^BloodKetone"))
                source = $"BK-{bucketName}";
            else
                continue; // data row is ignored

            if (fuelStatList.Find(x => x.Name.Equals(source)) == null)
                fuelStatList.Add(new FuelStat(source));

            // Debug, Locate the stat with the zero ketone
            //if (source.StartsWith("BK") && value == 0.0)
            //    Console.WriteLine(fuelStatList.FirstOrDefault(x => x.Name.Equals(source)));

            var fuelStat = fuelStatList.FirstOrDefault(x => x.Name.Equals(source));
            if (value <= 0)
                value = 0.08; // Neither Glucose or ketone values can be negative. Zero is not possible either.
            fuelStat!.Add(value, date.Ticks);

            minDate = Math.Min(date.Ticks, minDate);
            maxDate = Math.Max(date.Ticks, maxDate);

            counter++;
        }

        count = counter;
        timeSpan = TimeSpan.FromTicks(maxDate - minDate);
        return fuelStatList;
    }
}