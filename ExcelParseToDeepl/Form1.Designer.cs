namespace DeeplTranslator
{
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            TextBox_GlossaryFiles = new TextBox();
            Button_GenerateGlossaries = new Button();
            TextBox_TranslationFiles = new TextBox();
            Button_TranslateAlerts = new Button();
            TextBox_Logger = new TextBox();
            Button_TranslateLanguages = new Button();
            Button_FolderSelect = new Button();
            button1 = new Button();
            GroupBox_Glossaries = new GroupBox();
            GroupBox_Translations = new GroupBox();
            TabControl_Main = new TabControl();
            TabPage_Translate = new TabPage();
            groupBox1 = new GroupBox();
            TabPage_NewAlerts = new TabPage();
            GroupBox_NewAlertsOptions = new GroupBox();
            Button_ConvertTxt = new Button();
            GroupBox_NewAlertsFileDestination = new GroupBox();
            Button_NewAlertSelectFileDestination = new Button();
            TextBox_NewAlertFileDestination = new TextBox();
            groupBox_NewAlerts = new GroupBox();
            Button_SelectNewAlerts = new Button();
            TextBox_NewAlerts = new TextBox();
            GroupBox_Glossaries.SuspendLayout();
            GroupBox_Translations.SuspendLayout();
            TabControl_Main.SuspendLayout();
            TabPage_Translate.SuspendLayout();
            groupBox1.SuspendLayout();
            TabPage_NewAlerts.SuspendLayout();
            GroupBox_NewAlertsOptions.SuspendLayout();
            GroupBox_NewAlertsFileDestination.SuspendLayout();
            groupBox_NewAlerts.SuspendLayout();
            SuspendLayout();
            // 
            // TextBox_GlossaryFiles
            // 
            TextBox_GlossaryFiles.AllowDrop = true;
            resources.ApplyResources(TextBox_GlossaryFiles, "TextBox_GlossaryFiles");
            TextBox_GlossaryFiles.Name = "TextBox_GlossaryFiles";
            TextBox_GlossaryFiles.DragDrop += TextBoxFileSelect_DragDrop;
            TextBox_GlossaryFiles.DragOver += TextBoxFileSelect_DragOver;
            // 
            // Button_GenerateGlossaries
            // 
            resources.ApplyResources(Button_GenerateGlossaries, "Button_GenerateGlossaries");
            Button_GenerateGlossaries.Name = "Button_GenerateGlossaries";
            Button_GenerateGlossaries.UseVisualStyleBackColor = true;
            Button_GenerateGlossaries.Click += ButtonGenerateGlossaries_OnClick;
            // 
            // TextBox_TranslationFiles
            // 
            TextBox_TranslationFiles.AllowDrop = true;
            resources.ApplyResources(TextBox_TranslationFiles, "TextBox_TranslationFiles");
            TextBox_TranslationFiles.Name = "TextBox_TranslationFiles";
            TextBox_TranslationFiles.DragDrop += TextBoxFileSelect_DragDrop;
            TextBox_TranslationFiles.DragOver += TextBoxFileSelect_DragOver;
            // 
            // Button_TranslateAlerts
            // 
            resources.ApplyResources(Button_TranslateAlerts, "Button_TranslateAlerts");
            Button_TranslateAlerts.Name = "Button_TranslateAlerts";
            Button_TranslateAlerts.UseVisualStyleBackColor = true;
            Button_TranslateAlerts.Click += ButtonTranslateAlerts_OnClick;
            // 
            // TextBox_Logger
            // 
            TextBox_Logger.AllowDrop = true;
            resources.ApplyResources(TextBox_Logger, "TextBox_Logger");
            TextBox_Logger.Name = "TextBox_Logger";
            TextBox_Logger.ReadOnly = true;
            // 
            // Button_TranslateLanguages
            // 
            resources.ApplyResources(Button_TranslateLanguages, "Button_TranslateLanguages");
            Button_TranslateLanguages.Name = "Button_TranslateLanguages";
            Button_TranslateLanguages.UseVisualStyleBackColor = true;
            Button_TranslateLanguages.Click += ButtonTranslateLanguages_OnClick;
            // 
            // Button_FolderSelect
            // 
            resources.ApplyResources(Button_FolderSelect, "Button_FolderSelect");
            Button_FolderSelect.Name = "Button_FolderSelect";
            Button_FolderSelect.UseVisualStyleBackColor = true;
            Button_FolderSelect.Click += ButtonSelectExcelFile_OnClick;
            // 
            // button1
            // 
            resources.ApplyResources(button1, "button1");
            button1.Name = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += ButtonSelectFolder_OnClick;
            // 
            // GroupBox_Glossaries
            // 
            GroupBox_Glossaries.Controls.Add(Button_FolderSelect);
            GroupBox_Glossaries.Controls.Add(TextBox_GlossaryFiles);
            resources.ApplyResources(GroupBox_Glossaries, "GroupBox_Glossaries");
            GroupBox_Glossaries.Name = "GroupBox_Glossaries";
            GroupBox_Glossaries.TabStop = false;
            // 
            // GroupBox_Translations
            // 
            GroupBox_Translations.Controls.Add(button1);
            GroupBox_Translations.Controls.Add(TextBox_TranslationFiles);
            resources.ApplyResources(GroupBox_Translations, "GroupBox_Translations");
            GroupBox_Translations.Name = "GroupBox_Translations";
            GroupBox_Translations.TabStop = false;
            // 
            // TabControl_Main
            // 
            resources.ApplyResources(TabControl_Main, "TabControl_Main");
            TabControl_Main.Controls.Add(TabPage_Translate);
            TabControl_Main.Controls.Add(TabPage_NewAlerts);
            TabControl_Main.Name = "TabControl_Main";
            TabControl_Main.SelectedIndex = 0;
            // 
            // TabPage_Translate
            // 
            TabPage_Translate.BackColor = Color.WhiteSmoke;
            TabPage_Translate.Controls.Add(groupBox1);
            TabPage_Translate.Controls.Add(GroupBox_Translations);
            TabPage_Translate.Controls.Add(GroupBox_Glossaries);
            resources.ApplyResources(TabPage_Translate, "TabPage_Translate");
            TabPage_Translate.Name = "TabPage_Translate";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(Button_GenerateGlossaries);
            groupBox1.Controls.Add(Button_TranslateAlerts);
            groupBox1.Controls.Add(Button_TranslateLanguages);
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            // 
            // TabPage_NewAlerts
            // 
            TabPage_NewAlerts.BackColor = Color.WhiteSmoke;
            TabPage_NewAlerts.Controls.Add(GroupBox_NewAlertsOptions);
            TabPage_NewAlerts.Controls.Add(GroupBox_NewAlertsFileDestination);
            TabPage_NewAlerts.Controls.Add(groupBox_NewAlerts);
            resources.ApplyResources(TabPage_NewAlerts, "TabPage_NewAlerts");
            TabPage_NewAlerts.Name = "TabPage_NewAlerts";
            // 
            // GroupBox_NewAlertsOptions
            // 
            GroupBox_NewAlertsOptions.Controls.Add(Button_ConvertTxt);
            resources.ApplyResources(GroupBox_NewAlertsOptions, "GroupBox_NewAlertsOptions");
            GroupBox_NewAlertsOptions.Name = "GroupBox_NewAlertsOptions";
            GroupBox_NewAlertsOptions.TabStop = false;
            // 
            // Button_ConvertTxt
            // 
            resources.ApplyResources(Button_ConvertTxt, "Button_ConvertTxt");
            Button_ConvertTxt.Name = "Button_ConvertTxt";
            Button_ConvertTxt.UseVisualStyleBackColor = true;
            Button_ConvertTxt.Click += Button_OnClickConvertNewAlerts;
            // 
            // GroupBox_NewAlertsFileDestination
            // 
            GroupBox_NewAlertsFileDestination.Controls.Add(Button_NewAlertSelectFileDestination);
            GroupBox_NewAlertsFileDestination.Controls.Add(TextBox_NewAlertFileDestination);
            resources.ApplyResources(GroupBox_NewAlertsFileDestination, "GroupBox_NewAlertsFileDestination");
            GroupBox_NewAlertsFileDestination.Name = "GroupBox_NewAlertsFileDestination";
            GroupBox_NewAlertsFileDestination.TabStop = false;
            // 
            // Button_NewAlertSelectFileDestination
            // 
            resources.ApplyResources(Button_NewAlertSelectFileDestination, "Button_NewAlertSelectFileDestination");
            Button_NewAlertSelectFileDestination.Name = "Button_NewAlertSelectFileDestination";
            Button_NewAlertSelectFileDestination.UseVisualStyleBackColor = true;
            Button_NewAlertSelectFileDestination.Click += ButtonSelectFolderNewAlertFileDest_OnClick;
            // 
            // TextBox_NewAlertFileDestination
            // 
            TextBox_NewAlertFileDestination.AllowDrop = true;
            resources.ApplyResources(TextBox_NewAlertFileDestination, "TextBox_NewAlertFileDestination");
            TextBox_NewAlertFileDestination.Name = "TextBox_NewAlertFileDestination";
            // 
            // groupBox_NewAlerts
            // 
            groupBox_NewAlerts.Controls.Add(Button_SelectNewAlerts);
            groupBox_NewAlerts.Controls.Add(TextBox_NewAlerts);
            resources.ApplyResources(groupBox_NewAlerts, "groupBox_NewAlerts");
            groupBox_NewAlerts.Name = "groupBox_NewAlerts";
            groupBox_NewAlerts.TabStop = false;
            // 
            // Button_SelectNewAlerts
            // 
            resources.ApplyResources(Button_SelectNewAlerts, "Button_SelectNewAlerts");
            Button_SelectNewAlerts.Name = "Button_SelectNewAlerts";
            Button_SelectNewAlerts.UseVisualStyleBackColor = true;
            Button_SelectNewAlerts.Click += ButtonSelectFolderNewAlerts_OnClick;
            // 
            // TextBox_NewAlerts
            // 
            TextBox_NewAlerts.AllowDrop = true;
            resources.ApplyResources(TextBox_NewAlerts, "TextBox_NewAlerts");
            TextBox_NewAlerts.Name = "TextBox_NewAlerts";
            // 
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            Controls.Add(TextBox_Logger);
            Controls.Add(TabControl_Main);
            Name = "Form1";
            Load += MainForm_OnLoad;
            GroupBox_Glossaries.ResumeLayout(false);
            GroupBox_Glossaries.PerformLayout();
            GroupBox_Translations.ResumeLayout(false);
            GroupBox_Translations.PerformLayout();
            TabControl_Main.ResumeLayout(false);
            TabPage_Translate.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            TabPage_NewAlerts.ResumeLayout(false);
            GroupBox_NewAlertsOptions.ResumeLayout(false);
            GroupBox_NewAlertsFileDestination.ResumeLayout(false);
            GroupBox_NewAlertsFileDestination.PerformLayout();
            groupBox_NewAlerts.ResumeLayout(false);
            groupBox_NewAlerts.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox TextBox_GlossaryFiles;
        private Button Button_GenerateGlossaries;
        private TextBox TextBox_TranslationFiles;
        private TextBox TextBox_Logger;
        private Button Button_TranslateAlerts;
        private Button Button_TranslateLanguages;
        private Label TranslationsLabel;
        private Button Button_FolderSelect;
        private Button button1;
        private GroupBox GroupBox_Glossaries;
        private GroupBox GroupBox_Translations;
        private TabControl TabControl_Main;
        private TabPage TabPage_Translate;
        private TabPage TabPage_NewAlerts;
        private GroupBox groupBox_NewAlerts;
        private Button Button_SelectNewAlerts;
        private TextBox TextBox_NewAlerts;
        private GroupBox GroupBox_NewAlertsFileDestination;
        private Button Button_NewAlertSelectFileDestination;
        private Button Button_ConvertTxt;
        private TextBox TextBox_NewAlertFileDestination;
        private GroupBox GroupBox_NewAlertsOptions;
        private GroupBox groupBox1;
    }
}