using CadPlugin.Core;

namespace CadPlugin.Core.Tests;

public sealed class BracketParametersTests
{
    [Fact]
    public void Validate_Defaults_AreValid()
    {
        var p = new BracketParameters();

        var issues = p.Validate();

        Assert.Empty(issues);
    }

    [Theory]
    [InlineData(49)]
    [InlineData(91)]
    public void Validate_PhoneWidth_OutOfRange_ProducesIssue(double phoneWidth)
    {
        var p = new BracketParameters
        {
            PhoneWidthMm = phoneWidth,
        };

        var issues = p.Validate();

        Assert.Contains(issues, x => x.Field == nameof(BracketParameters.PhoneWidthMm));
    }

    [Fact]
    public void Validate_DependencyViolation_ProducesIssuesForBothFields()
    {
        var p = new BracketParameters
        {
            PhoneWidthMm = 80,
            InnerWidthMm = 81,
        };

        var issues = p.Validate();

        Assert.Contains(issues, x => x.Field == nameof(BracketParameters.InnerWidthMm));
        Assert.Contains(issues, x => x.Field == nameof(BracketParameters.PhoneWidthMm));
    }

    [Fact]
    public void GetInnerWidthAllowedRangeByPhoneWidth_RespectsClearanceAndBaseRange()
    {
        var p = new BracketParameters
        {
            PhoneWidthMm = 70,
        };

        var r = p.GetInnerWidthAllowedRangeByPhoneWidth();

        Assert.Equal(72, r.Min, 6);
        Assert.Equal(p.InnerWidthRangeMm.Max, r.Max, 6);
    }

    [Fact]
    public void GetPhoneWidthAllowedRangeByInnerWidth_RespectsClearanceAndBaseRange()
    {
        var p = new BracketParameters
        {
            InnerWidthMm = 76,
        };

        var r = p.GetPhoneWidthAllowedRangeByInnerWidth();

        Assert.Equal(p.PhoneWidthRangeMm.Min, r.Min, 6);
        Assert.Equal(74, r.Max, 6);
    }

    [Fact]
    public void CreatePlan_ComputesExpectedOuterWidthAndHeight()
    {
        var p = new BracketParameters
        {
            InnerWidthMm = 78,
            ArmThicknessMm = 4,
            BaseThicknessMm = 6,
            ArmLengthMm = 65,
        };

        var plan = BracketGeometryPlanner.CreatePlan(p);

        Assert.Equal(86, plan.OuterWidthMm, 6);
        Assert.Equal(79, plan.TotalHeightMm, 6);
    }

    [Fact]
    public void CreatePlan_PositionsRightArmAfterInnerGap()
    {
        var p = new BracketParameters
        {
            InnerWidthMm = 80,
            ArmThicknessMm = 5,
            BaseThicknessMm = 7,
            ArmLengthMm = 60,
        };

        var plan = BracketGeometryPlanner.CreatePlan(p);

        Assert.Equal(0, plan.LeftArm.X, 6);
        Assert.Equal(28.25, plan.RightArm.X, 6);
        Assert.Equal(33.25, plan.Base.Width, 6);
        Assert.Equal(90, plan.DepthMm, 6);
    }

    [Fact]
    public void CreatePlan_BuildsContourWithExpectedCorners()
    {
        var p = new BracketParameters
        {
            InnerWidthMm = 80,
            ArmThicknessMm = 5,
            BaseThicknessMm = 7,
            ArmLengthMm = 60,
        };

        var plan = BracketGeometryPlanner.CreatePlan(p);

        Assert.Equal(10, plan.Contour.Count);
        Assert.Equal(new Point2D(0, 0), plan.Contour[0]);
        Assert.Equal(new Point2D(33.25, 0), plan.Contour[1]);
        Assert.Equal(new Point2D(28.25, 10), plan.Contour[3]);
        Assert.Equal(new Point2D(24.0625, 65), plan.Contour[7]);
        Assert.Equal(new Point2D(24.0625, 75), plan.Contour[8]);
        Assert.Equal(new Point2D(0, 75), plan.Contour[9]);
    }

    [Fact]
    public void CreatePlan_AddsTopHooksInsideClampWidth()
    {
        var p = new BracketParameters
        {
            InnerWidthMm = 77,
            ArmThicknessMm = 5,
            BaseThicknessMm = 6,
            ArmLengthMm = 70,
        };

        var plan = BracketGeometryPlanner.CreatePlan(p);

        Assert.True(plan.LeftHook.Width > 0);
        Assert.True(plan.RightHook.Width > 0);
        Assert.Equal(p.BaseThicknessMm, plan.LeftHook.X, 6);
        Assert.Equal(plan.LeftHook.X, plan.RightHook.X, 6);
        Assert.Equal(plan.TotalHeightMm - plan.HookHeightMm, plan.LeftHook.Y, 6);
        Assert.Equal(plan.OuterWidthMm, plan.DepthMm, 6);
    }

    [Fact]
    public void CreateSvg_ContainsPolygonAndDimensions()
    {
        var plan = BracketGeometryPlanner.CreatePlan(new BracketParameters());

        var svg = BracketSvgExporter.CreateSvg(plan);

        Assert.Contains("<polygon", svg);
        Assert.Contains("Внешняя ширина", svg);
        Assert.Contains("Кронштейн для телефона", svg);
    }
}
