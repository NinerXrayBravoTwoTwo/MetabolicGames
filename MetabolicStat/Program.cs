using MetabolicStat.Program;
using MetabolicStat.StatMath;
using System.Text.RegularExpressions;

void GenerateMetabolicReports(string? folderName, double bucketDays, FuelStat[] mgStats1, FuelStat[] bkStats1, List<GkiStat> list)
{
    string ReportTitleBuilder(string reportName, string samples, double dayInterval, TimeSpan timeSpan)
    {
        var reportTitle =
            $"Title:, {reportName}- {dayInterval:N2} day interval with {samples} samples in {timeSpan.Days} days";
        return reportTitle;
    }


    void KetoneReport(FuelStat[] listFuelStats, string? writeFolderName, double interval)
    {
        // Write BK report
        var sum = new FuelStat("sumKetone");
        var samples = 0;

        var result = listFuelStats.Where(x => x.Name.StartsWith("BK-"))
            .Select(item => new FuelStat(item)).ToArray();

        foreach (var item in result)
        {
            sum.Add(item);
            samples += (int)item.N;
        }

        var report = new List<string>
        {
            ReportTitleBuilder("BK", samples.ToString(), interval, sum.TimeSpan) // TITLE
            , FuelStat.Header
            , string.Join("\n", result.Where(x => x.Name.StartsWith("BK-")).Select(x => x.ToString()))
        };

        File.WriteAllLines($"{writeFolderName}\\BK-{interval:N2}-days.csv", report.ToArray());
    }

    void GkiReport(List<GkiStat> gkiStats, string? writeFolderName, double interval)
    {
        // Write GKI report
        var ketoN = gkiStats.Sum(x => x.N);
        // var gluN = gkiStats.Sum(x => x.Ng); // TODO: GKI samples are actually a set of BK BG and CGM ... BK or BG is not accurate estimate of the number of samples ...

        var mint = DateTime.MaxValue.Ticks;
        var maxt = DateTime.MinValue.Ticks;

        foreach (var item in gkiStats)
        {
            mint = Math.Min(mint, item.FromDateTime.Ticks);
            maxt = Math.Max(maxt, item.ToDateTime.Ticks);
        }

        var gkiSpan = new TimeSpan(maxt - mint);
        var report = new List<string>
        {
            ReportTitleBuilder("GKI", $"{ketoN}", interval, gkiSpan) // TITLE
            ,
            GkiStat.Header, string.Join('\n', gkiStats.Select(x => x.ToString()))
        };

        File.WriteAllLines($"{writeFolderName}\\GKI-{interval:N2}-days.csv", report.ToArray());
    }

    void GlucoseReport(IEnumerable<FuelStat> mgList, string? writeFolderName, double interval)
    {
        // include all months with any valid glucose data not just CGM merged BG months
        // use CGM- stats for any missing mgm stats and BG- stats for any missing CGM- stats

        var listGlucose = mgList.OrderBy(x => x.FromDateTime).ToArray();

        var glucose = listGlucose as FuelStat?[];
        var enumerable = listGlucose as FuelStat?[];

#pragma warning disable CS8602
        // This data should has been through an invalid data interpolation before this point if there are still null values I need it to fail here
        var mint = DateTime.MaxValue.Ticks;
        var maxt = DateTime.MinValue.Ticks;

        foreach (var item in glucose)
        {
            mint = Math.Min(mint, item.FromDateTime.Ticks);
            maxt = Math.Max(maxt, item.ToDateTime.Ticks);
        }

        var timeSpan = new TimeSpan(maxt - mint);

        var samples = $"{enumerable.Sum(stat => stat.N):N}";
#pragma warning restore CS8602

        var report = new List<string>
        {
            ReportTitleBuilder("BG & CGM", samples, interval, timeSpan) // TITLE
            , FuelStat.Header
        };

#pragma warning disable CS8602
        // Missing report data will be glaringly obvious to the user, no check necessary it can not be recovered from here
        report.AddRange(enumerable.Select(x => x.ToString()));
#pragma warning restore CS8602

        File.WriteAllLines($"{writeFolderName}\\CGM-{interval:N2}-days.csv", report.ToArray());
    }

    {
        KetoneReport(bkStats1, folderName, bucketDays);

        GkiReport(list, folderName, bucketDays);

        GlucoseReport(mgStats1, folderName, bucketDays);
    }
}

FuelStat[] Interpolate(FuelStat[] inputSet)
{
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
                    Console.WriteLine($"bucket interpolation for: '{interpolateMe[i].Name} N={interpolateMe[i].N}'");
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

// Get the data subroutine
static List<FuelStat> FuelStats(string file, double bucketDays, out int i, out TimeSpan timeSpan1)
{
    // Root of data generator
    var statMatrix = new ComputeStatMatrix(file);

    var list = statMatrix.Run(bucketDays, out i, out timeSpan1).ToList();
    return list;
}

/* ****************** */

const double bucketDays = 0.240378875; // 0.48075775; // 0.9615155; // 1.9023031; // 3.80460625;// 7.6092125; // 15.218425; // 30.43685


#region Get the program arguments and read the data
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

List<FuelStat> fuelStats;
try
{
    fuelStats = FuelStats(fileName, bucketDays, out var count, out var timeSpan);
    Console.WriteLine($"{count} values spanning {timeSpan} days\n");
}
catch (FileNotFoundException error)
{
    Console.WriteLine(error.Message);
    return;
}
catch (DirectoryNotFoundException error)
{
    Console.WriteLine(error.Message);
    return;
}
catch (ArgumentException error)
{
    Console.WriteLine(error.Message);
    return;
}
#endregion

// Root of Reporting
// Create merged glucose stats = BG + CGM
// Note:This does not handle the case where there is no CGM data but are BG data for a bucket, taken care of below this loop ...
var mgList = new List<FuelStat>();
foreach (var cgmStat in fuelStats.Where(x => x.Name.StartsWith("CGM-")).ToArray())
{
    //mgList.Add(cgmStat);
    //Lookup matching "BG"
    var nameSplit = cgmStat.Name.Split('-');
    var target = "BG-" + $"{nameSplit[1]}-{nameSplit[2]}";

    var bgStat = fuelStats.FirstOrDefault(x => x.Name.Equals(target));

    if (bgStat != null)
    {
        var name = "MGL-" + $"{nameSplit[1]}-{nameSplit[2]}";
        var mglStat = new FuelStat(cgmStat);
        mglStat.Add(bgStat); // Merge BK into CGM to create merged glucose
        mgList.Add(mglStat);
    }
}

// Are there cases where there are no CGM buckets for corresponding BG buckets ?
foreach (var bgStat in fuelStats.Where(x => x.Name.StartsWith("BG-")).ToArray())
{
    var nameSplit = bgStat.Name.Split('-');

    var target = "CGM-" + $"{nameSplit[1]}-{nameSplit[2]}";

    var cgmStat = fuelStats.FirstOrDefault(x => x.Name.Equals(target));

    if (cgmStat == null)
    {
        Console.WriteLine($"Missing cgm bucket: {target} using => {bgStat.Name} with only BG data, N={bgStat.N}");
        mgList.Add(bgStat); // mglist will now require sorting before reporting
    }
}

//// Are there cases where a bucket does not exist for the CGM range?
//var lasttDate = DateTime.MinValue;
//foreach (var item in mgList.OrderBy(x => x.FromDateTime))
//{
//    if


//}

//return;  //Exit for debug

// Get interpolated BK list
var bkStats = fuelStats.Where(x => x.Name.StartsWith("BK-"))
.Select(item => new FuelStat(item)).ToArray();
{
    var nanCountBk = bkStats.Count(x => x.IsNaN);
    if (nanCountBk > 0)
    {
        Console.WriteLine($"{nanCountBk} NaN before 1st interpolation pass");
        bkStats = Interpolate(inputSet: bkStats);
    }
    nanCountBk = bkStats.Count(x => x.IsNaN);
    if (nanCountBk > 0)
    {
        Console.WriteLine($"{nanCountBk} NaN before 2nd interpolation pass");
        bkStats = Interpolate(inputSet: bkStats);
    }
    nanCountBk = bkStats.Count(x => x.IsNaN);
    if (nanCountBk > 0)
    {
        Console.WriteLine($"{nanCountBk} NaN before 3rd interpolation pass");
        //bkStats = Interpolate(bkStats);
        Console.WriteLine("Exiting because BK  bucket interpolation is failing.");
        return;  // Exit because interpolation is failing
    }
}



// Interpolate MGS list
// clone
var mgStats = mgList.Select(item => new FuelStat(item)).OrderBy(x => x.FromDateTime).ToArray();
{
    var nanCount = mgStats.Count(x => x.IsNaN);
    if (nanCount > 0)
    {
        Console.WriteLine($"{nanCount} NaN before 1st interpolation pass");
        mgStats = Interpolate(inputSet: mgStats);
    }

    nanCount = mgStats.Count(x => x.IsNaN);
    if (nanCount > 0)
    {
        Console.WriteLine($"{nanCount} NaN before 2st interpolation pass");
        mgStats = Interpolate(inputSet: mgStats);
    }

    nanCount = mgStats.Count(x => x.IsNaN);
    if (nanCount > 0)
        Console.WriteLine($"Warning: there are still {nanCount}'NaN' buckets after 2 interpolate passes.");
}

// Calculate GKI stats
var gkiList = (from bkStat in bkStats
        .Where(x => x.Name.StartsWith("BK-"))
               let nameSplit = bkStat.Name.Split('-')
               let target = "MGL-" + $"{nameSplit[1]}-{nameSplit[2]}"
               let glStat = mgStats.FirstOrDefault(x => x.Name.Equals(target))
               where glStat != null
               let name = "GKI-" + $"{nameSplit[1]}-{nameSplit[2]}"
               select new GkiStat(glStat, bkStat, name)).ToList(); // TODO: Is Glucose in mg OR mmol.  Some partial work has been done

// Write Reports
var directoryName = new FileInfo(fileName).DirectoryName;

GenerateMetabolicReports(directoryName, bucketDays, mgStats, bkStats, gkiList);

// Find this BK, It is a test case with 0 value BK that caused div by zero error, expected
//8/1/2021,BK-8/1/2021-8/31/2021, 1.27, 0.1, 4.6, 0.1618, -1.32443, 41
//// Because it tests the divide by zero in GKI calculation 

////var bkStats = fuelStats.Where(x => x.Name.Contains("BK-") && x.MinX.Equals(0));
////Console.WriteLine(string.Join('\n', bkStats.Select(x => x.ToString())));

// Suspend the screen.  
//Console.ReadLine();