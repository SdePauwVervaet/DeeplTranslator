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
            textBox3 = new TextBox();
            Button_TranslateLanguages = new Button();
            Button_FolderSelect = new Button();
            button1 = new Button();
            GroupBox_Glossaries = new GroupBox();
            GroupBox_Translations = new GroupBox();
            GroupBox_Glossaries.SuspendLayout();
            GroupBox_Translations.SuspendLayout();
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
            // textBox3
            // 
            textBox3.AllowDrop = true;
            resources.ApplyResources(textBox3, "textBox3");
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
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
            Button_FolderSelect.Click += ButtonSelectGlossaryFiles_OnClick;
            // 
            // button1
            // 
            resources.ApplyResources(button1, "button1");
            button1.Name = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += ButtonSelectTranslationFiles_OnClick;
            // 
            // GroupBox_Glossaries
            // 
            GroupBox_Glossaries.BackColor = SystemColors.Control;
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
            // Form1
            // 
            AutoScaleMode = AutoScaleMode.None;
            resources.ApplyResources(this, "$this");
            Controls.Add(GroupBox_Translations);
            Controls.Add(GroupBox_Glossaries);
            Controls.Add(Button_TranslateLanguages);
            Controls.Add(textBox3);
            Controls.Add(Button_TranslateAlerts);
            Controls.Add(Button_GenerateGlossaries);
            Name = "Form1";
            Load += MainForm_OnLoad;
            GroupBox_Glossaries.ResumeLayout(false);
            GroupBox_Glossaries.PerformLayout();
            GroupBox_Translations.ResumeLayout(false);
            GroupBox_Translations.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox TextBox_GlossaryFiles;
        private Button Button_GenerateGlossaries;
        private TextBox TextBox_TranslationFiles;
        private TextBox textBox3;
        private Button Button_TranslateAlerts;
        private Button Button_TranslateLanguages;
        private Label TranslationsLabel;
        private Button Button_FolderSelect;
        private Button button1;
        private GroupBox GroupBox_Glossaries;
        private GroupBox GroupBox_Translations;
    }
}