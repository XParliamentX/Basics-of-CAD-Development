namespace CadPlugin.App;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        labelTitle = new Label();
        labelPhoneWidth = new Label();
        labelInnerWidth = new Label();
        labelArmLength = new Label();
        labelArmThickness = new Label();
        labelBaseThickness = new Label();
        textPhoneWidth = new TextBox();
        textInnerWidth = new TextBox();
        textArmLength = new TextBox();
        textArmThickness = new TextBox();
        textBaseThickness = new TextBox();
        buttonBuild = new Button();
        listErrors = new ListBox();
        SuspendLayout();
        // 
        // labelTitle
        // 
        labelTitle.AutoSize = true;
        labelTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        labelTitle.Location = new Point(12, 9);
        labelTitle.Name = "labelTitle";
        labelTitle.Size = new Size(504, 28);
        labelTitle.TabIndex = 0;
        labelTitle.Text = "Плагин: кронштейн крепления телефона (КОМПАС)";
        // 
        // labelPhoneWidth
        // 
        labelPhoneWidth.AutoSize = true;
        labelPhoneWidth.Location = new Point(12, 55);
        labelPhoneWidth.Name = "labelPhoneWidth";
        labelPhoneWidth.Size = new Size(177, 20);
        labelPhoneWidth.TabIndex = 1;
        labelPhoneWidth.Text = "Ширина телефона, мм";
        // 
        // labelInnerWidth
        // 
        labelInnerWidth.AutoSize = true;
        labelInnerWidth.Location = new Point(12, 93);
        labelInnerWidth.Name = "labelInnerWidth";
        labelInnerWidth.Size = new Size(244, 20);
        labelInnerWidth.TabIndex = 2;
        labelInnerWidth.Text = "Внутренняя ширина зажима, мм";
        // 
        // labelArmLength
        // 
        labelArmLength.AutoSize = true;
        labelArmLength.Location = new Point(12, 131);
        labelArmLength.Name = "labelArmLength";
        labelArmLength.Size = new Size(172, 20);
        labelArmLength.TabIndex = 3;
        labelArmLength.Text = "Длина плеча, мм (L)";
        // 
        // labelArmThickness
        // 
        labelArmThickness.AutoSize = true;
        labelArmThickness.Location = new Point(12, 169);
        labelArmThickness.Name = "labelArmThickness";
        labelArmThickness.Size = new Size(189, 20);
        labelArmThickness.TabIndex = 4;
        labelArmThickness.Text = "Толщина плеча, мм (T)";
        // 
        // labelBaseThickness
        // 
        labelBaseThickness.AutoSize = true;
        labelBaseThickness.Location = new Point(12, 207);
        labelBaseThickness.Name = "labelBaseThickness";
        labelBaseThickness.Size = new Size(203, 20);
        labelBaseThickness.TabIndex = 5;
        labelBaseThickness.Text = "Толщина основания, мм (B)";
        // 
        // textPhoneWidth
        // 
        textPhoneWidth.Location = new Point(290, 52);
        textPhoneWidth.Name = "textPhoneWidth";
        textPhoneWidth.Size = new Size(120, 27);
        textPhoneWidth.TabIndex = 6;
        // 
        // textInnerWidth
        // 
        textInnerWidth.Location = new Point(290, 90);
        textInnerWidth.Name = "textInnerWidth";
        textInnerWidth.Size = new Size(120, 27);
        textInnerWidth.TabIndex = 7;
        // 
        // textArmLength
        // 
        textArmLength.Location = new Point(290, 128);
        textArmLength.Name = "textArmLength";
        textArmLength.Size = new Size(120, 27);
        textArmLength.TabIndex = 8;
        // 
        // textArmThickness
        // 
        textArmThickness.Location = new Point(290, 166);
        textArmThickness.Name = "textArmThickness";
        textArmThickness.Size = new Size(120, 27);
        textArmThickness.TabIndex = 9;
        // 
        // textBaseThickness
        // 
        textBaseThickness.Location = new Point(290, 204);
        textBaseThickness.Name = "textBaseThickness";
        textBaseThickness.Size = new Size(120, 27);
        textBaseThickness.TabIndex = 10;
        // 
        // buttonBuild
        // 
        buttonBuild.Location = new Point(12, 250);
        buttonBuild.Name = "buttonBuild";
        buttonBuild.Size = new Size(398, 40);
        buttonBuild.TabIndex = 11;
        buttonBuild.Text = "Построить в КОМПАС-3D";
        buttonBuild.UseVisualStyleBackColor = true;
        // 
        // listErrors
        // 
        listErrors.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        listErrors.FormattingEnabled = true;
        listErrors.ItemHeight = 20;
        listErrors.Location = new Point(430, 52);
        listErrors.Name = "listErrors";
        listErrors.Size = new Size(510, 344);
        listErrors.TabIndex = 12;
        // 
        // Form1
        // 
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 420);
        Controls.Add(listErrors);
        Controls.Add(buttonBuild);
        Controls.Add(textBaseThickness);
        Controls.Add(textArmThickness);
        Controls.Add(textArmLength);
        Controls.Add(textInnerWidth);
        Controls.Add(textPhoneWidth);
        Controls.Add(labelBaseThickness);
        Controls.Add(labelArmThickness);
        Controls.Add(labelArmLength);
        Controls.Add(labelInnerWidth);
        Controls.Add(labelPhoneWidth);
        Controls.Add(labelTitle);
        MinimumSize = new Size(980, 470);
        Name = "Form1";
        Text = "Кронштейн телефона - параметры";
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label labelTitle;
    private Label labelPhoneWidth;
    private Label labelInnerWidth;
    private Label labelArmLength;
    private Label labelArmThickness;
    private Label labelBaseThickness;
    private TextBox textPhoneWidth;
    private TextBox textInnerWidth;
    private TextBox textArmLength;
    private TextBox textArmThickness;
    private TextBox textBaseThickness;
    private Button buttonBuild;
    private ListBox listErrors;
}
