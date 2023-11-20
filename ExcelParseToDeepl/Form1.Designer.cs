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
            Button_GlossarySelection = new Button();
            Button_TranslationFolderSelection = new Button();
            TextBox_TranslationFiles = new TextBox();
            Button_TranslateAlerts = new Button();
            textBox3 = new TextBox();
            Button_TranslateLanguages = new Button();
            SuspendLayout();
            // 
            // TextBox_GlossaryFiles
            // 
            TextBox_GlossaryFiles.AllowDrop = true;
            TextBox_GlossaryFiles.Location = new Point(13, 12);
            TextBox_GlossaryFiles.Name = "TextBox_GlossaryFiles";
            TextBox_GlossaryFiles.Size = new Size(362, 23);
            TextBox_GlossaryFiles.TabIndex = 1;
            TextBox_GlossaryFiles.TextChanged += TextBoxGlossarySelect_OnTextChanged;
            TextBox_GlossaryFiles.DragDrop += TextBoxGlossarySelect_DragDrop;
            TextBox_GlossaryFiles.DragOver += TextBoxGlossarySelect_DragOver;
            // 
            // Button_GenerateGlossaries
            // 
            Button_GenerateGlossaries.Location = new Point(539, 13);
            Button_GenerateGlossaries.Name = "Button_GenerateGlossaries";
            Button_GenerateGlossaries.Size = new Size(199, 24);
            Button_GenerateGlossaries.TabIndex = 2;
            Button_GenerateGlossaries.Text = "Generate glossary files";
            Button_GenerateGlossaries.UseVisualStyleBackColor = true;
            Button_GenerateGlossaries.Click += ButtonGenerateGlossaries_OnClick;
            // 
            // Button_GlossarySelection
            // 
            Button_GlossarySelection.Location = new Point(381, 12);
            Button_GlossarySelection.Name = "Button_GlossarySelection";
            Button_GlossarySelection.Size = new Size(152, 24);
            Button_GlossarySelection.TabIndex = 3;
            Button_GlossarySelection.Text = "Select Glossary Excel...";
            Button_GlossarySelection.UseVisualStyleBackColor = true;
            Button_GlossarySelection.Click += ButtonSelectGlossaryFiles_OnClick;
            // 
            // Button_TranslationFolderSelection
            // 
            Button_TranslationFolderSelection.Location = new Point(381, 74);
            Button_TranslationFolderSelection.Name = "Button_TranslationFolderSelection";
            Button_TranslationFolderSelection.Size = new Size(152, 24);
            Button_TranslationFolderSelection.TabIndex = 4;
            Button_TranslationFolderSelection.Text = "Select folder to translate...";
            Button_TranslationFolderSelection.UseVisualStyleBackColor = true;
            Button_TranslationFolderSelection.Click += ButtonSelectTranslationFiles_OnClick;
            // 
            // TextBox_TranslationFiles
            // 
            TextBox_TranslationFiles.AllowDrop = true;
            TextBox_TranslationFiles.Location = new Point(13, 74);
            TextBox_TranslationFiles.Name = "TextBox_TranslationFiles";
            TextBox_TranslationFiles.Size = new Size(362, 23);
            TextBox_TranslationFiles.TabIndex = 5;
            TextBox_TranslationFiles.TextChanged += TextBoxTranslationFolderSelect_OnTextChanged;
            // 
            // Button_TranslateAlerts
            // 
            Button_TranslateAlerts.Location = new Point(539, 60);
            Button_TranslateAlerts.Name = "Button_TranslateAlerts";
            Button_TranslateAlerts.Size = new Size(199, 24);
            Button_TranslateAlerts.TabIndex = 6;
            Button_TranslateAlerts.Text = "Translate 'Alert' files in folder";
            Button_TranslateAlerts.UseVisualStyleBackColor = true;
            Button_TranslateAlerts.Click += ButtonTranslateAlerts_OnClick;
            // 
            // textBox3
            // 
            textBox3.AllowDrop = true;
            textBox3.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox3.Location = new Point(13, 153);
            textBox3.Multiline = true;
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.ScrollBars = ScrollBars.Vertical;
            textBox3.Size = new Size(725, 348);
            textBox3.TabIndex = 7;
            // 
            // Button_TranslateLanguages
            // 
            Button_TranslateLanguages.Location = new Point(539, 90);
            Button_TranslateLanguages.Name = "Button_TranslateLanguages";
            Button_TranslateLanguages.Size = new Size(200, 23);
            Button_TranslateLanguages.TabIndex = 8;
            Button_TranslateLanguages.Text = "Translate Language files in folder";
            Button_TranslateLanguages.UseVisualStyleBackColor = true;
            Button_TranslateLanguages.Click += ButtonTranslateLanguages_OnClick;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(747, 509);
            Controls.Add(Button_TranslateLanguages);
            Controls.Add(textBox3);
            Controls.Add(Button_TranslateAlerts);
            Controls.Add(TextBox_TranslationFiles);
            Controls.Add(Button_TranslationFolderSelection);
            Controls.Add(Button_GlossarySelection);
            Controls.Add(Button_GenerateGlossaries);
            Controls.Add(TextBox_GlossaryFiles);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            Text = "Deepl Glossaries from Excel & Update translation files";
            Load += MainForm_OnLoad;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TextBox TextBox_GlossaryFiles;
        private Button Button_GenerateGlossaries;
        private Button Button_GlossarySelection;
        private Button Button_TranslationFolderSelection;
        private TextBox TextBox_TranslationFiles;
        private TextBox textBox3;
        private Button Button_TranslateAlerts;
        private Button Button_TranslateLanguages;
    }
}