namespace CadPlugin.App;

using System.Globalization;
using System.Text;
using CadPlugin.Core;
using CadPlugin.Wrapper;

public partial class Form1 : Form
{
    private readonly BracketParameters parameters = new();
    private readonly BracketBuilder builder = new(new Kompas3DWrapper());

    public Form1()
    {
        InitializeComponent();
        ApplyDefaultsToUi();
        WireEvents();
        RefreshValidationUi();
    }

    private void ApplyDefaultsToUi()
    {
        textPhoneWidth.Text = Format(parameters.PhoneWidthMm);
        textInnerWidth.Text = Format(parameters.InnerWidthMm);
        textArmLength.Text = Format(parameters.ArmLengthMm);
        textArmThickness.Text = Format(parameters.ArmThicknessMm);
        textBaseThickness.Text = Format(parameters.BaseThicknessMm);
    }

    private void WireEvents()
    {
        textPhoneWidth.TextChanged += (_, _) => OnAnyParameterChanged();
        textInnerWidth.TextChanged += (_, _) => OnAnyParameterChanged();
        textArmLength.TextChanged += (_, _) => OnAnyParameterChanged();
        textArmThickness.TextChanged += (_, _) => OnAnyParameterChanged();
        textBaseThickness.TextChanged += (_, _) => OnAnyParameterChanged();
        buttonBuild.Click += (_, _) => OnBuildClicked();
    }

    private void OnAnyParameterChanged()
    {
        TryRead(textPhoneWidth, out var phoneWidth);
        TryRead(textInnerWidth, out var innerWidth);
        TryRead(textArmLength, out var armLength);
        TryRead(textArmThickness, out var armThickness);
        TryRead(textBaseThickness, out var baseThickness);

        parameters.PhoneWidthMm = phoneWidth;
        parameters.InnerWidthMm = innerWidth;
        parameters.ArmLengthMm = armLength;
        parameters.ArmThicknessMm = armThickness;
        parameters.BaseThicknessMm = baseThickness;

        RefreshValidationUi();
    }

    private void RefreshValidationUi()
    {
        var issues = parameters.Validate();

        SetFieldState(textPhoneWidth, issues, nameof(BracketParameters.PhoneWidthMm));
        SetFieldState(textInnerWidth, issues, nameof(BracketParameters.InnerWidthMm));
        SetFieldState(textArmLength, issues, nameof(BracketParameters.ArmLengthMm));
        SetFieldState(textArmThickness, issues, nameof(BracketParameters.ArmThicknessMm));
        SetFieldState(textBaseThickness, issues, nameof(BracketParameters.BaseThicknessMm));

        listErrors.BeginUpdate();
        listErrors.Items.Clear();

        var phoneWidthAllowed = parameters.GetPhoneWidthAllowedRangeByInnerWidth();
        var innerWidthAllowed = parameters.GetInnerWidthAllowedRangeByPhoneWidth();
        listErrors.Items.Add($"Зазор: {BracketDefaults.ClearanceMm:0.###} мм");
        listErrors.Items.Add($"Допустимая ширина телефона при текущей внутренней ширине: {phoneWidthAllowed.Min:0.###}-{phoneWidthAllowed.Max:0.###} мм");
        listErrors.Items.Add($"Допустимая внутренняя ширина при текущей ширине телефона: {innerWidthAllowed.Min:0.###}-{innerWidthAllowed.Max:0.###} мм");

        if (issues.Count == 0)
        {
            var plan = BracketGeometryPlanner.CreatePlan(parameters);
            listErrors.Items.Add($"Расчетная внешняя ширина кронштейна: {plan.OuterWidthMm:0.###} мм");
            listErrors.Items.Add($"Расчетная общая высота кронштейна: {plan.TotalHeightMm:0.###} мм");
            listErrors.Items.Add($"Глубина модели по умолчанию: {plan.DepthMm:0.###} мм");
            listErrors.Items.Add($"Точек контура для эскиза: {plan.Contour.Count}");
        }

        listErrors.Items.Add(string.Empty);

        foreach (var issue in issues)
        {
            listErrors.Items.Add($"{issue.Field}: {issue.Message}");
        }

        listErrors.EndUpdate();
    }

    private void OnBuildClicked()
    {
        OnAnyParameterChanged();
        try
        {
            var issues = builder.Build(parameters);
            if (issues.Count > 0)
            {
                MessageBox.Show(
                    "Исправь ошибки параметров справа, затем повтори построение.",
                    "Валидация",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var plan = BracketGeometryPlanner.CreatePlan(parameters);
            var previewPath = SavePreviewSvg(plan);

            MessageBox.Show(
                "Модель кронштейна успешно построена в КОМПАС-3D.\n\n" +
                $"SVG-превью контура сохранено в:\n{previewPath}",
                "Готово",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            var details = new StringBuilder()
                .AppendLine("Ошибка при обращении к API КОМПАС-3D.")
                .AppendLine()
                .AppendLine(ex.Message)
                .AppendLine();

            if (ex.InnerException is not null)
            {
                details
                    .AppendLine()
                    .AppendLine($"Внутренняя причина: {ex.InnerException.Message}")
                    .AppendLine();
            }

            details
                .Append("(Если ошибка TYPE_E_ELEMENTNOTFOUND, обычно это несовпадение ожидаемого свойства или элемента API.)")
                .ToString();

            MessageBox.Show(
                details.ToString(),
                "COM/Interop ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string SavePreviewSvg(BracketBuildPlan plan)
    {
        var previewPath = Path.Combine(AppContext.BaseDirectory, "last-bracket-preview.svg");
        File.WriteAllText(previewPath, BracketSvgExporter.CreateSvg(plan), Encoding.UTF8);
        return previewPath;
    }

    private static void SetFieldState(TextBox textBox, IReadOnlyList<ValidationIssue> issues, string field)
    {
        var hasIssue = issues.Any(x => x.Field == field);
        textBox.BackColor = hasIssue ? Color.MistyRose : SystemColors.Window;
    }

    private static bool TryRead(TextBox tb, out double value)
    {
        var s = (tb.Text ?? string.Empty).Trim();
        if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        if (double.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
        {
            return true;
        }

        value = double.NaN;
        return false;
    }

    private static string Format(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);
}
