using MetabolicStat.FuelStatistics;
using MetabolicStat.StatMath;

namespace MetabolicStat;

internal static class ReportWriter
{
    public static void GenerateMetabolicReports(string? folderName, double bucketDays, IEnumerable<FuelStat> mgStats1, FuelStat[] bkStats1, List<GkiStat> list)
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

            var statOfStats = new Statistic();

            var result = listFuelStats
                .Where(x => x.Name.StartsWith("BK-"))
                .Select(item => new FuelStat(item)).ToArray();

            foreach (var item in result)
            {
                sum.Add(item);
                samples += (int)item.N;
                statOfStats.Add(item.MeanX(), item.MeanY() * TimeSpan.TicksPerDay);
            }

            var report = new List<string>
            {
                ReportTitleBuilder("BK", samples.ToString(), interval, sum.TimeSpan) // TITLE
                , FuelStat.Footer(4, statOfStats)
                , FuelStat.Header
                , string.Join("\r\n", result.Where(x => x.Name.StartsWith("BK-")).Select(x => x.ToString()))
                , FuelStat.Footer(4, statOfStats)
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
                , GkiStat.Header, string.Join("\r\n", gkiStats.Select(x => x.ToString()))
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

}