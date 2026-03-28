using System.Globalization;
using System.Text;

namespace CadPlugin.Core;

public static class BracketDefaults
{
    public const double ClearanceMm = 1.0;
    public const double MinSupportReachMm = 18.0;
    public const double MinRetainerHeightMm = 10.0;
    public const double MinTopRetainerReachMm = 12.0;
}

public readonly record struct Range(double Min, double Max)
{
    public bool Contains(double value) => value >= Min && value <= Max;
}

public sealed record ValidationIssue(string Field, string Message);

public readonly record struct Point2D(double X, double Y);

public readonly record struct Rectangle2D(double X, double Y, double Width, double Height)
{
    public double Right => X + Width;

    public double Top => Y + Height;
}

public sealed record BracketBuildPlan(
    Rectangle2D Base,
    Rectangle2D LeftArm,
    Rectangle2D RightArm,
    Rectangle2D LeftHook,
    Rectangle2D RightHook,
    IReadOnlyList<Point2D> Contour,
    double InnerWidthMm,
    double OuterWidthMm,
    double TotalHeightMm,
    double HookInsetMm,
    double HookHeightMm,
    double DepthMm);

public sealed class BracketParameters
{
    public Range PhoneWidthRangeMm { get; } = new(50, 90);
    public Range InnerWidthRangeMm { get; } = new(52, 92);
    public Range ArmLengthRangeMm { get; } = new(30, 120);
    public Range ArmThicknessRangeMm { get; } = new(3, 10);
    public Range BaseThicknessRangeMm { get; } = new(3, 12);

    public double PhoneWidthMm { get; set; } = 75;
    public double InnerWidthMm { get; set; } = 77;
    public double ArmLengthMm { get; set; } = 70;
    public double ArmThicknessMm { get; set; } = 5;
    public double BaseThicknessMm { get; set; } = 6;

    public Range GetPhoneWidthAllowedRangeByInnerWidth()
    {
        var max = InnerWidthMm - 2 * BracketDefaults.ClearanceMm;
        var cappedMax = Math.Min(PhoneWidthRangeMm.Max, max);
        return new Range(PhoneWidthRangeMm.Min, cappedMax);
    }

    public Range GetInnerWidthAllowedRangeByPhoneWidth()
    {
        var min = PhoneWidthMm + 2 * BracketDefaults.ClearanceMm;
        var cappedMin = Math.Max(InnerWidthRangeMm.Min, min);
        return new Range(cappedMin, InnerWidthRangeMm.Max);
    }

    public IReadOnlyList<ValidationIssue> Validate()
    {
        var issues = new List<ValidationIssue>();

        ValidateRange(issues, nameof(PhoneWidthMm), PhoneWidthMm, PhoneWidthRangeMm);
        ValidateRange(issues, nameof(InnerWidthMm), InnerWidthMm, InnerWidthRangeMm);
        ValidateRange(issues, nameof(ArmLengthMm), ArmLengthMm, ArmLengthRangeMm);
        ValidateRange(issues, nameof(ArmThicknessMm), ArmThicknessMm, ArmThicknessRangeMm);
        ValidateRange(issues, nameof(BaseThicknessMm), BaseThicknessMm, BaseThicknessRangeMm);

        var dependencyOk = InnerWidthMm >= PhoneWidthMm + 2 * BracketDefaults.ClearanceMm;
        if (!dependencyOk)
        {
            issues.Add(
                new ValidationIssue(
                    nameof(InnerWidthMm),
                    $"Должно быть >= ширина_телефона + 2*зазор ({FormatMm(PhoneWidthMm)} + 2*{FormatMm(BracketDefaults.ClearanceMm)})"));
            issues.Add(
                new ValidationIssue(
                    nameof(PhoneWidthMm),
                    $"Должно быть <= внутренняя_ширина - 2*зазор ({FormatMm(InnerWidthMm)} - 2*{FormatMm(BracketDefaults.ClearanceMm)})"));
        }

        var phoneWidthAllowed = GetPhoneWidthAllowedRangeByInnerWidth();
        if (phoneWidthAllowed.Max < phoneWidthAllowed.Min)
        {
            issues.Add(
                new ValidationIssue(
                    nameof(InnerWidthMm),
                    "Слишком маленькая внутренняя ширина для заданного зазора (не остаётся допустимого диапазона для ширины телефона)."));
        }

        var innerWidthAllowed = GetInnerWidthAllowedRangeByPhoneWidth();
        if (innerWidthAllowed.Min > innerWidthAllowed.Max)
        {
            issues.Add(
                new ValidationIssue(
                    nameof(PhoneWidthMm),
                    "Слишком большая ширина телефона для заданного зазора (не остаётся допустимого диапазона для внутренней ширины)."));
        }

        return issues;
    }

    private static void ValidateRange(
        ICollection<ValidationIssue> issues,
        string field,
        double value,
        Range range)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            issues.Add(new ValidationIssue(field, "Некорректное число."));
            return;
        }

        if (!range.Contains(value))
        {
            issues.Add(
                new ValidationIssue(
                    field,
                    $"Допустимый диапазон: {FormatMm(range.Min)}-{FormatMm(range.Max)} мм."));
        }
    }

    private static string FormatMm(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}

public static class BracketGeometryPlanner
{
    public static BracketBuildPlan CreatePlan(BracketParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var outerWidth = parameters.InnerWidthMm + 2 * parameters.ArmThicknessMm;
        var backThickness = parameters.BaseThicknessMm;
        var shelfThickness = parameters.ArmThicknessMm;
        var frontLipThickness = parameters.ArmThicknessMm;
        var frontLipHeight = Math.Max(BracketDefaults.MinRetainerHeightMm, parameters.ArmThicknessMm * 2.0);
        var topLipHeight = Math.Max(BracketDefaults.MinRetainerHeightMm, parameters.ArmThicknessMm * 2.0);
        var supportReach = Math.Max(BracketDefaults.MinSupportReachMm, parameters.PhoneWidthMm * 0.35);
        var topRetainerReach = Math.Max(BracketDefaults.MinTopRetainerReachMm, supportReach * 0.65);
        var totalHeight = shelfThickness + parameters.ArmLengthMm + topLipHeight;

        var @base = new Rectangle2D(
            X: 0,
            Y: 0,
            Width: backThickness + supportReach,
            Height: shelfThickness);

        var leftArm = new Rectangle2D(
            X: 0,
            Y: 0,
            Width: backThickness,
            Height: totalHeight);

        var rightArm = new Rectangle2D(
            X: backThickness + supportReach - frontLipThickness,
            Y: 0,
            Width: frontLipThickness,
            Height: frontLipHeight);

        var leftHook = new Rectangle2D(
            X: backThickness,
            Y: totalHeight - topLipHeight,
            Width: topRetainerReach,
            Height: topLipHeight);

        var rightHook = new Rectangle2D(
            X: backThickness,
            Y: totalHeight - topLipHeight,
            Width: topRetainerReach,
            Height: topLipHeight);

        var contour = new List<Point2D>
        {
            new(0, 0),
            new(@base.Right, 0),
            new(@base.Right, frontLipHeight),
            new(rightArm.X, frontLipHeight),
            new(rightArm.X, shelfThickness),
            new(backThickness, shelfThickness),
            new(backThickness, leftHook.Y),
            new(leftHook.Right, leftHook.Y),
            new(leftHook.Right, totalHeight),
            new(0, totalHeight),
        };

        return new BracketBuildPlan(
            Base: @base,
            LeftArm: leftArm,
            RightArm: rightArm,
            LeftHook: leftHook,
            RightHook: rightHook,
            Contour: contour,
            InnerWidthMm: parameters.InnerWidthMm,
            OuterWidthMm: outerWidth,
            TotalHeightMm: totalHeight,
            HookInsetMm: topRetainerReach,
            HookHeightMm: topLipHeight,
            DepthMm: outerWidth);
    }
}

public static class BracketSvgExporter
{
    public static string CreateSvg(BracketBuildPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        const double margin = 8;
        var width = plan.OuterWidthMm + margin * 2;
        var height = plan.TotalHeightMm + margin * 2;

        var points = string.Join(
            " ",
            plan.Contour.Select(point => $"{Format(point.X + margin)},{Format(height - point.Y - margin)}"));

        var sb = new StringBuilder();
        sb.AppendLine("""<svg xmlns="http://www.w3.org/2000/svg" version="1.1">""");
        sb.AppendLine($"""  <svg viewBox="0 0 {Format(width)} {Format(height)}" width="{Format(width * 4)}" height="{Format(height * 4)}">""");
        sb.AppendLine("""    <rect width="100%" height="100%" fill="#f8f6f1" />""");
        sb.AppendLine($"""    <polygon points="{points}" fill="#d3e4cd" stroke="#253237" stroke-width="1.2" />""");
        sb.AppendLine($"""    <line x1="{Format(plan.LeftArm.Right + margin)}" y1="{Format(height - plan.Base.Top - margin)}" x2="{Format(plan.RightArm.X + margin)}" y2="{Format(height - plan.Base.Top - margin)}" stroke="#bc4749" stroke-width="0.8" stroke-dasharray="2 2" />""");
        sb.AppendLine("""    <text x="8" y="16" font-family="Segoe UI, Arial, sans-serif" font-size="6" fill="#253237">Кронштейн для телефона: контур</text>""");
        sb.AppendLine($"""    <text x="8" y="24" font-family="Segoe UI, Arial, sans-serif" font-size="5" fill="#253237">Внешняя ширина: {Format(plan.OuterWidthMm)} мм, высота: {Format(plan.TotalHeightMm)} мм, глубина: {Format(plan.DepthMm)} мм</text>""");
        sb.AppendLine($"""    <text x="8" y="31" font-family="Segoe UI, Arial, sans-serif" font-size="5" fill="#bc4749">Внутренняя ширина зажима: {Format(plan.InnerWidthMm)} мм</text>""");
        sb.AppendLine("""  </svg>""");
        sb.AppendLine("""</svg>""");
        return sb.ToString();
    }

    private static string Format(double value)
        => value.ToString("0.###", CultureInfo.InvariantCulture);
}
