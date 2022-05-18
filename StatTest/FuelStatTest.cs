using MetabolicStat.FuelStatistics;
using MetabolicStat.StatMath;
using Xunit;
using Xunit.Abstractions;

namespace StatTest;

public class FuelStatTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FuelStatTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EmptyFuelStatIsNan()
    {
        // create statistic
        FuelStat? test = new("statA");

        Assert.True(test.IsNaN);

        _testOutputHelper.WriteLine(test.ToString());
    }

    [Fact]
    public void KnownDataSetStat()
    {
        var stat = new FuelStat("Alpha");
        double x = 0;

        while (x < 100)
            stat.Add(x++);

        _testOutputHelper.WriteLine(stat.ToString());
        _testOutputHelper.WriteLine(((Statistic)stat).ToString());

        Assert.False((stat.IsNaN));
        Assert.Equal(0, stat.MinX);
        Assert.Equal(1, stat.Correlation());
        Assert.Equal(49.5, stat.MeanX());
        Assert.Equal(1, stat.Slope());
    }

    [Fact]
    public void FuelStatClone()
    {
        var orig = new FuelStat("Alpha");
        double x = 0, y = -1;

        while (x < 100) orig.Add(x++, y++);

        _testOutputHelper.WriteLine(orig.ToString());

        Assert.False(orig.IsNaN);

        var clone = new Statistic(orig);

        Assert.False(clone.IsNaN);

        Assert.Equal(((Statistic)orig).ToString(), clone.ToString());

        // The clone should not update if the original changes, i.e. they are not the same instance
        orig.Add(0, 0);
        _testOutputHelper.WriteLine(clone.ToString());

        Assert.NotEqual(((Statistic)orig).ToString(), clone.ToString());
        Assert.Equal(clone.N + 1, orig.N);
    }

    [Fact]
    public void FuelStatCloneBetter()
    {
        var orig = new FuelStat("Delta");
        double x = 0, y = -1;

        while (x < 100)
            orig.Add(x++, y++);

        _testOutputHelper.WriteLine(orig.ToString());

        Assert.False(orig.IsNaN);

        var clone = new FuelStat(orig);

        Assert.False(clone.IsNaN);

        Assert.Equal(orig.Name, clone.Name);

        Assert.Equal(orig.ToString(), clone.ToString());

        // The clone should not update if the original changes, i.e. they are not the same instance
        orig.Add(0, 0);

        _testOutputHelper.WriteLine(clone.ToString());

        Assert.NotEqual(orig.ToString(), clone.ToString());
        Assert.Equal(clone.N + 1, orig.N);
    }
}

