namespace DeeplTranslator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Logger.InitializeLogger(textBox3);
        }

        private void ButtonSelectTranslationFiles_OnClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            TextBox_TranslationFiles.AppendText(dialog.SelectedPath);
        }

        private void ButtonSelectGlossaryFiles_OnClick(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextBox_GlossaryFiles.Text = dialog.FileName;
            }
        }

        private void ButtonGenerateGlossaries_OnClick(object sender, EventArgs e)
        {
            if (TextBox_GlossaryFiles.Text.Length == 0)
            {
                TextBox_GlossaryFiles.Text = @"Select a file by dropping it here...";
                TextBox_GlossaryFiles.Update();
            }
            else
            {
                var file = new FileInfo(TextBox_GlossaryFiles.Text);
                string extn = file.Extension;
                System.Diagnostics.Debug.WriteLine("File Extension: {0}", extn);

                if (extn == ".xlsx")
                {
                    var translatorService = new TranslatorService();
                    translatorService.ExcelParser.ParseExcel(file.DirectoryName!, file.Name);
                    translatorService.ExcelParser.GenerateDictionaries();
                    translatorService.UpdateDeeplGlossary();
                }
                else
                {
                    TextBox_GlossaryFiles.Text = @"I'll only 'eat' .xlsx files. Select a file by dropping it here...";
                    TextBox_GlossaryFiles.Update();
                }
            }
            TextBox_GlossaryFiles.Text = string.Empty;
        }

        private async void ButtonTranslateAlerts_OnClick(object sender, EventArgs e)
        {
            string baseDirectory = TextBox_TranslationFiles.Text;

            var jsonFiles = Directory.GetFiles(baseDirectory, "*.json", SearchOption.AllDirectories)
                .ToList();

            var translatorService = new TranslatorService();
            await translatorService.TranslateAlertFiles(jsonFiles);
            TextBox_TranslationFiles.Text = string.Empty;
        }

        private async void ButtonTranslateLanguages_OnClick(object sender, EventArgs e)
        {
            string baseDirectory = TextBox_TranslationFiles.Text;

            var jsFiles = Directory.GetFiles(baseDirectory, "*.js", SearchOption.AllDirectories)
                .ToList();

            string[] exceptions = { "index.js", "i18n.js", "i18n-config.js" };
            foreach (string exc in exceptions)
            {
                jsFiles.RemoveAll(u => u.Contains(exc));
            }

            var translatorService = new TranslatorService();
            await translatorService.TranslateLanguageFiles(jsFiles);
            TextBox_TranslationFiles.Text = string.Empty;
        }

        private void MainForm_OnLoad(object sender, EventArgs e)
        {
            System.Drawing.Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            // Sets window size to a quarter of the primary - screens resolution.
            this.Size = new Size(Convert.ToInt32(0.25 * workingRectangle.Width), Convert.ToInt32(0.25 * workingRectangle.Height));

            this.Location = new Point(10, 10);
        }

        private void TextBoxFileSelect_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is not string[] files || !files.Any()) return;
            
            if (sender is TextBox textbox)
            {
                textbox.Text = files.First();
            }
        }

        private void TextBoxFileSelect_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
        }
    }
}