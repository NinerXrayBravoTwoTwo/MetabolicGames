using System.Text.RegularExpressions;
using MetabolicStat;
using MetabolicStat.FuelStatistics;

FuelStat[] Interpolate(FuelStat[] inputSet)
{
    #region Interpolation

    IEnumerable<FuelStat> InterpolateListFuelStats(IEnumerable<FuelStat> statsToCheckForNan)
    {
        // This interpolation method depends on the list being in order otherwise the math is pointless
        var interpolateMe = statsToCheckForNan.ToList().OrderBy(x => x.FromDateTime).ToArray();

        var interpolated = new List<FuelStat>(); // create new empty stat list

        for (var i = 1; i < interpolateMe.Length - 1; i++)
            if (interpolateMe[i].IsNaN)
            {
                // Add the previous and next item to me
                var newValue = new FuelStat(interpolateMe[i]);
                try
                {
                    newValue.Add(interpolateMe[i - 1]); //  give it the average of the surrounding samples
                    newValue.Add(interpolateMe[i + 1]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Do nothing, boundary error would be better to trim off ends of array
                }

                if (newValue.IsNaN)
                {
                    // still a problem child.  Requires additional interpolation pass
                    interpolated.Add(interpolateMe[i]);
                }
                else
                {
                    // add the interpolated new one
                    var interpFuelStat = new FuelStat(newValue)
                    {
                        IsInterpolated = true
                    };

                    interpolated.Add(interpFuelStat);
                    //Console.WriteLine($"bucket interpolation for: '{interpolateMe[i].Name} N={interpolateMe[i].N}'");
                }
            }
            else
            {
                interpolated.Add(interpolateMe[i]);
            }

        return interpolated;
    }

    IEnumerable<FuelStat>? resultSet = null;

    if (inputSet.Any(item => item.IsNaN))
        resultSet = InterpolateListFuelStats(inputSet.ToArray());

    if (resultSet == null)
        return inputSet;

    var enumerable = resultSet as FuelStat[] ?? resultSet.ToArray();

    if (enumerable.Length > 1)
        inputSet = new List<FuelStat>(enumerable).ToArray();

    return inputSet;
}

FuelStat[] InterpolateReport(FuelStat[] inputSet, string tag)
{
    if (inputSet.Length == 0) return inputSet;

    var resultSet = inputSet.Select(stat => new FuelStat(stat)).ToArray();

    var limit = 0;
    while (limit++ <= 3)
    {
        var nanCount = resultSet.Count(x => x.IsNaN);
        if (nanCount <= 0) continue;

        Console.WriteLine($"{tag}: {nanCount} NaN before interpolation pass: '{limit}'");
        resultSet = Interpolate(resultSet);
    }

    return resultSet;
}

#endregion

#region Get the program arguments and read the data

IEnumerable<FuelStat>? ReadDataFromFile(string fileName, double bucketDays)
{
    IEnumerable<FuelStat>? resultFuelStats = null;
    try
    {
        var statMatrix = new ComputeStatMatrix(fileName);
        resultFuelStats = statMatrix.Run(bucketDays, out var count, out var timeSpan).ToList();
        // Console.WriteLine($"{count} values spanning {timeSpan} days\n");
    }
    catch (Exception error)
    {
        Console.WriteLine(error.Message);
    }

    return resultFuelStats?.ToArray();
}

var argString = string.Join(" ", args);

if (argString.Length == 0)
{
    Console.WriteLine("'-file filename.csv' is required\n");
    return;
}

//
var fileName = string.Empty;
var match = Regex.Match(argString, @"\-file\s+([\w\W]+)", RegexOptions.IgnoreCase);
if (match.Success) fileName = match.Groups[1].Value;

if (match.Success == false || fileName.Equals(string.Empty))
{
    Console.WriteLine($"{args[1]} is not a valid file");
    return;
}

#endregion

/* ****************** */

// * A year is 365.24+ days, these day divisions are powers of 2 divisions of a year so reports see all the data and
// * there are no gaps in display of available data
// * 7.0238884615 days in a week
// * 30.43685 is the number of days in month if a month goes into a year 12 times.
// * 60.8737 days in two months
// * 91.31055 days in a quarter
// * 182.6211 days in half a year
// * 365.2422 days in a year  
// this is mainly for reporting my month quarter and year.
// Write the three report stats GKI, Cgm/Bg, Bk for each bucket-day size.

foreach (var bucketDays in
         new[]
         {
             0.240378875, 0.48075775, 0.9615155, 1.9023031, 3.80460625, 7.6092125, 15.218425, 30.43685, 60.8737,
             91.31055
         })
{
    var fuelStats = ReadDataFromFile(fileName, bucketDays);
    Console.WriteLine($"Creating tables for '{bucketDays}' days.");
    // Root of Reporting
    if (fuelStats != null)
    {
        var enumerable = fuelStats as FuelStat[] ?? fuelStats.ToArray();
        var cgmList2 = enumerable.Where(x => x.Name.StartsWith("CGM-")).OrderBy(x => x.FromDateTime).ToArray();
        var bgList2 = enumerable.Where(x => x.Name.StartsWith("BG-")).OrderBy(x => x.FromDateTime).ToArray();
        var bkList2 = enumerable.Where(x => x.Name.StartsWith("BK-")).OrderBy(x => x.FromDateTime).ToArray();

        // Merge matching buckets of CGM and BG together
        var mgList2 = new List<FuelStat>();

        foreach (var cgmStat in cgmList2)
        {
            //Lookup matching "BG"
            var nameSplit = cgmStat.Name.Split('-');
            var target = "BG-" + $"{nameSplit[1]}-{nameSplit[2]}";

            var bgStat = bgList2.FirstOrDefault(x => x.Name.Equals(target));

            if (bgStat != null)
            {
                var name = "MGL-" + $"{nameSplit[1]}-{nameSplit[2]}";
                var mglStat = new FuelStat(name, cgmStat);
                mglStat.Add(bgStat); // Merge BK into CGM to create merged glucose
                mgList2.Add(mglStat);
            }
            else
            {
                mgList2.Add(cgmStat);
            }
        }

        // Add the BG buckets that had no matching MG buckets to the MG list
        var mgListPlus = new List<FuelStat>(mgList2);

        // Are there cases where there are no CGM buckets for corresponding BG buckets ?
        foreach (var bgStat in bgList2.Where(x => x.Name.StartsWith("BG-")).ToArray())
        {
            var nameSplit = bgStat.Name.Split('-');

            var target = "CGM-" + $"{nameSplit[1]}-{nameSplit[2]}";

            var cgmStat = cgmList2.FirstOrDefault(x => x.Name.Equals(target));

            if (cgmStat == null)
            {
                Console.WriteLine(
                    $"Missing cgm bucket: {target} using => {bgStat.Name} with only BG data, N={bgStat.N}");
                mgListPlus.Add(bgStat); // mglist will now require sorting before reporting
            }
        }

        //// TODO: Are there cases where a bucket does not exist for the CGM range?  Do I care? so far no data is invented to fill in gaps.
        //var lastDate = DateTime.MinValue;

        #region interpolate

        var bkStats = InterpolateReport(
            bkList2
                .Select(item => new FuelStat(item))
                .OrderBy(x => x.FromDateTime)
                .ToArray()
            , "BK");

        // Interpolate MGS list
        var mgStats = InterpolateReport(
            mgListPlus
                .Select(item => new FuelStat(item))
                .OrderBy(x => x.FromDateTime)
                .ToArray()
            , "GLU");

        #endregion

        // Calculate GKI stats
        var gkiList = (from bkStat in bkStats
                    .Where(x => x.Name.StartsWith("BK-"))
                let nameSplit = bkStat.Name.Split('-')
                let target =
                    "MGL-" + $"{nameSplit[1]}-{nameSplit[2]}" // BUG what if there is just a CGM for this bucket?
                let glStat = mgStats.FirstOrDefault(x => x.Name.Equals(target))
                where glStat != null
                let name = "GKI-" + $"{nameSplit[1]}-{nameSplit[2]}"
                select new GkiStat(glStat, bkStat, name))
            .ToList(); // TODO: Is Glucose in mg OR mmol.  Some partial work has been done

        // Write Reports
        var directoryName = new FileInfo(fileName).DirectoryName;

        ReportWriter.GenerateMetabolicReports(directoryName, bucketDays, mgStats, bkStats, gkiList);
    }
}

// Find this BK, It is a test case with 0 value BK that caused div by zero error, expected
//8/1/2021,BK-8/1/2021-8/31/2021, 1.27, 0.1, 4.6, 0.1618, -1.32443, 41
//// Because it tests the divide by zero in GKI calculation 

////var bkStats = fuelStats.Where(x => x.Name.Contains("BK-") && x.MinX.Equals(0));
////Console.WriteLine(string.Join('\n', bkStats.Select(x => x.ToString())));

// Suspend the screen.  
//Console.ReadLine();