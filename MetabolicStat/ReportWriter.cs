using MetabolicStat.FuelStatistics;
using MetabolicStat.StatMath;

namespace MetabolicStat;

internal static class ReportWriter
{
    // TODO:  Need to add report summaries from three reports.Need to complete report summary for GKI report
    public static void GenerateMetabolicReports(string? folderName, double bucketDays, IEnumerable<FuelStat> mgStats1,
        FuelStat[] bkStats1, List<GkiStat> list)
    {
        string ReportTitleBuilder(string reportName, int samples, double dayInterval, TimeSpan timeSpan)
        {
            var reportTitle =
                $"Title:, {reportName}- {dayInterval:F2} day interval with {samples} samples in {timeSpan.Days} days";
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
                ReportTitleBuilder("BK", samples, interval, sum.TimeSpan) // TITLE
                , FuelStat.Footer(statOfStats)
                , FuelStat.Header
                , string.Join("\r\n", result.Where(x => x.Name.StartsWith("BK-")).Select(x => x.ToString())),
                FuelStat.Footer(statOfStats)
            };
            File.WriteAllLines($"{writeFolderName}\\BK-{interval:N2}-days.csv", report.ToArray());
        }

        void GkiReport(List<GkiStat> gkiStats, string? writeFolderName, double interval)
        {
            // Write GKI report
            var ketoN = gkiStats.Sum(x => x.N);

            var mint = DateTime.MaxValue.Ticks;
            var maxt = DateTime.MinValue.Ticks;

            var gkiStatOfstats = new Statistic();
            var gluStatOfstats = new Statistic();
            var ketStatOfstats = new Statistic();
            foreach (var item in gkiStats)
            {
                mint = Math.Min(mint, item.FromDateTime.Ticks);
                maxt = Math.Max(maxt, item.ToDateTime.Ticks);
                gkiStatOfstats.Add(item.MeanX, item.MeanY * TimeSpan.TicksPerDay);
                gluStatOfstats.Add(item.GlucoseStat.MeanX(), item.GlucoseStat.MeanY() * TimeSpan.TicksPerDay);
                ketStatOfstats.Add(item.KetoneStat.MeanX(), item.KetoneStat.MeanY() * TimeSpan.TicksPerDay);
            }

            var gkiSamples = gkiStats.Sum((gki => gki.N));
            var ketSamples = gkiStats.Sum(gki => gki.KetoneStat.N);
            var gluSambles = gkiStats.Sum(gki => gki.GlucoseStat.N);
            var gkiSpan = new TimeSpan(maxt - mint);
            var report = new List<string>
            {
                ReportTitleBuilder("GKI", (int)ketoN, interval, gkiSpan) // TITLE
                , GkiStat.Footer(gkiStatOfstats, gluStatOfstats, ketStatOfstats, gkiSamples, gluSambles, ketSamples)
                , GkiStat.Header
                , string.Join("\r\n", gkiStats.Select(x => x.ToString()))
                , GkiStat.Footer(gkiStatOfstats, gluStatOfstats, ketStatOfstats, gkiSamples,gluSambles,ketSamples)
            };

            File.WriteAllLines($"{writeFolderName}\\GKI-{interval:N2}-days.csv", report.ToArray());
        }

        void GlucoseReport(IEnumerable<FuelStat> mgList, string? writeFolderName, double interval)
        {
            // include all months with any valid glucose data not just CGM merged BG months
            // use CGM- stats for any missing mgm stats and BG- stats for any missing CGM- stats

            var listGlucose = mgList.OrderBy(x => x.FromDateTime).ToArray();

            //var glucose = listGlucose;
            var enumerable = listGlucose;

            var statOfstats = new Statistic();
            var statOfglucose = new Statistic();

            foreach (var item in enumerable)
            {
                statOfglucose.Add(item);
                statOfstats.Add(item.MeanX(), item.MeanY() * TimeSpan.TicksPerDay);
            }

            var timeSpan = new TimeSpan((long)statOfglucose.MaxY * TimeSpan.TicksPerDay -
                                        (long)statOfglucose.MinY * TimeSpan.TicksPerDay);

            var report = new List<string>
            {
                FuelStat.Footer(statOfstats)
                , ReportTitleBuilder("BG & CGM", (int)enumerable.Sum(stat => stat.N), interval, timeSpan) // TITLE
                , FuelStat.Header, string.Join("\r\n", enumerable.Select(x => x.ToString()))
                , FuelStat.Footer(statOfstats)
            };

            File.WriteAllLines($"{writeFolderName}\\CGM-{interval:N2}-days.csv", report.ToArray());
        }

        {
            KetoneReport(bkStats1, folderName, bucketDays);

            GkiReport(list, folderName, bucketDays);

            GlucoseReport(mgStats1, folderName, bucketDays);
        }
    }
}