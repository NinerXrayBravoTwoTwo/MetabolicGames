using MetabolicStat.FuelStatistics;
using System;
using Xunit;
using Xunit.Abstractions;

namespace StatTest;

public class GkiStatTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GkiStatTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EmptyGkiStatIsNan()
    {
        // create statistic
        var glu = new FuelStat("glu");
        var ket = new FuelStat("ket");
        IFuelStat test = new GkiStat(glu, ket, "gki-a");

        Assert.True(glu.IsNaN);
        Assert.True(ket.IsNaN);
        Assert.True(test.IsNaN);

        _testOutputHelper.WriteLine(test.ToString());
    }

    [Fact]
    public void KnownDataSetStat()
    {
        var dateBase = DateTime.Now;

        var glumg = RandomGlucose(dateBase, out var glummol);
        _testOutputHelper.WriteLine(glumg.ToString());
        _testOutputHelper.WriteLine(glummol.ToString());


        var ket = RandomKetone(dateBase);
        _testOutputHelper.WriteLine(ket.ToString());

        var gkimg = new GkiStat(glumg, ket, "gki-mg ");
        var gkimmol = new GkiStat(glummol, ket, "gki-mol", true);

        Assert.False(gkimg.IsNaN);
        _testOutputHelper.WriteLine(gkimg.ToString());
        _testOutputHelper.WriteLine(gkimmol.ToString());


        //_testOutputHelper.WriteLine(glu.ToString());
        //_testOutputHelper.WriteLine(ket.ToString());
        //_testOutputHelper.WriteLine(((Statistic)glu).ToString());
        //_testOutputHelper.WriteLine(((Statistic)ket).ToString());

        //Assert.False((stat.IsNaN));
        //Assert.Equal(0, stat.MinX);
        //Assert.Equal(1, stat.Correlation());
        //Assert.Equal(49.5, stat.MeanX());
        //Assert.Equal(1, stat.Slope());
    }

    private static FuelStat RandomGlucose(DateTime dateBase, out FuelStat gluMmol)
    {
        var glu = new FuelStat("glu-mg ");
        var glummol = new FuelStat("glu-mol");

        double x = 0;

        var gluValue = 85;
        var t = dateBase;
        while (x++ < 200)
        {
            if (gluValue < 75) gluValue += RandomGen.Next(-2, 6);
            if (gluValue > 100) gluValue += RandomGen.Next(-5, 3);
            else gluValue += RandomGen.Next(-4, 4);

            t = t.AddMinutes(3);
            glu.Add(gluValue, t.Ticks);
            glummol.Add(gluValue / 18.0, t.Ticks);
        }

        gluMmol = glummol;
        return glu;
    }

    private static FuelStat RandomKetone(DateTime dateBase)
    {
        var ketone = new FuelStat("ketone");
        double x = 0;

        var value = 0.5;
        var t = dateBase;

        while (x++ < 200)
        {
            var plusOrMinus = RandomGen.NextBool() ? -1 : 1;

            if (value < 0) value += RandomGen.NextDouble() * 0.5;
            if (value > 2) value -= value / 2;
            else value += RandomGen.NextDouble() + plusOrMinus * RandomGen.NextDouble();

            t = t.AddMinutes(3);
            ketone.Add(value, t.Ticks);
        }

        return ketone;
    }
}