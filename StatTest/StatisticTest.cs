using MetabolicStat.StatMath;
using Xunit;
using Xunit.Abstractions;

namespace StatTest;

public class StatisticTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StatisticTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EmptyStatIsNan()
    {
        // create statistic
        var test = new Statistic();

        Assert.True(test.IsNaN);

        _testOutputHelper.WriteLine(test.ToString());
    }

    [Fact]
    public void KnownDatasetStat()
    {
        var stat = new Statistic();

        double x = 0;
        double y = -1;
        while (x < 100)
            stat.Add(x++, y++);

        _testOutputHelper.WriteLine(stat.ToString());

        Assert.False(stat.IsNaN);
        Assert.Equal(0, stat.MinX);
        Assert.Equal(1, stat.Correlation());
        Assert.Equal(49.5, stat.MeanX());
        Assert.Equal(1, stat.Slope());
    }

    [Fact]
    public void StatClone()
    {
        var orig = new Statistic();
        double x = 0, y = -1;

        while (x < 100)
            orig.Add(x++, y++);


        _testOutputHelper.WriteLine(orig.ToString());

        Assert.False(orig.IsNaN);

        var clone = new Statistic(orig);

        Assert.False(clone.IsNaN);

        Assert.Equal(orig.ToString(), clone.ToString());

        // The clone should not update if the original changes, i.e. they are not the same instance
        orig.Add(0, 0);
        _testOutputHelper.WriteLine(clone.ToString());

        Assert.NotEqual(orig.ToString(), clone.ToString());
        Assert.Equal(clone.N + 1, orig.N);
    }

    [Fact]
    public void StatAddStat()
    {
        // set up
        var orig = new Statistic();

        double x = 0, y = -1;

        while (x < 500)
            orig.Add(x++, y++);
        _testOutputHelper.WriteLine(orig.ToString());

        var clone = new Statistic(orig); // Make a clone (new unrelated instance) of the statistic.


        Assert.False(orig.IsNaN);
        Assert.False(clone.IsNaN);

        // test - 
        clone.Add(orig); // Adding the original statistic to its clone, 
        // a> should only effect the clone.
        // b> number of samples should double
        // c> variance should remain the same 
        // d> other sum attributes should all be doubled.
        // e> Mean should remain the same.

        Assert.Equal(orig.N * 2, clone.N);
        Assert.Equal(orig.Qx2(), clone.Qx2());
        Assert.Equal(orig.Qy2(), clone.Qy2());

        Assert.Equal(orig.Sx * 2, clone.Sx);
        Assert.Equal(orig.Sy * 2, clone.Sy);
        Assert.Equal(orig.Sy2 * 2, clone.Sy2);
        Assert.Equal(orig.Sxy * 2, clone.Sxy);

        Assert.Equal(orig.MeanX(), clone.MeanX());
        Assert.Equal(orig.MeanY(), clone.MeanY());


        Assert.False(clone.IsNaN);

        // Assert
        _testOutputHelper.WriteLine(clone.ToString());
    }
}