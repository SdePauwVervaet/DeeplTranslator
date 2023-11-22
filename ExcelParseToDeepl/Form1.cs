namespace DeeplTranslator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Logger.InitializeLogger(TextBox_Logger);
        }

        private void ButtonSelectFolder_OnClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = @"Select Folder";
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextBox_TranslationFiles.Text = dialog.SelectedPath;
            }
        }

        private void ButtonSelectExcelFile_OnClick(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog();
            dialog.Filter = @"Excel Files|*.xlsx";
            dialog.Title = @"Select Excel Files";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() != DialogResult.OK) return;

            if (Path.GetExtension(dialog.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                TextBox_GlossaryFiles.Text = dialog.FileName;
            }
            else
            {
                MessageBox.Show(@"Please select a valid .xlsx file.", @"Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ButtonSelectFolderNewAlerts_OnClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = @"Select Folder";
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextBox_NewAlerts.Text = dialog.SelectedPath;
            }
        }

        private void ButtonSelectFolderNewAlertFileDest_OnClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = @"Select Folder";
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextBox_NewAlertFileDestination.Text = dialog.SelectedPath;
            }
        }

        private void Button_OnClickConvertNewAlerts(object sender, EventArgs e)
        {
            string fileFolderPath = TextBox_NewAlerts .Text.Trim();
            string resultFilePath = TextBox_NewAlertFileDestination.Text.Trim();

            if (string.IsNullOrEmpty(fileFolderPath))
            {
                MessageBox.Show(@"Please enter a folder path.", @"Empty or invalid Folder Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<string> txtFiles = Directory.GetFiles(fileFolderPath, "*.txt").ToList();

            var translatorService = new TranslatorService();

            foreach (string file in txtFiles)
            {
                Logger.LogMessage(Environment.NewLine);
                translatorService.ConvertTxtFiles(file, resultFilePath);
            }
        }

        private void ButtonGenerateGlossaries_OnClick(object sender, EventArgs e)
        {
            string filePath = TextBox_GlossaryFiles.Text.Trim();

            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show(@"Please select a .xlsx file!", @"Empty or Invalid file.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var file = new FileInfo(filePath);
            string extn = file.Extension.ToLower();
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
                MessageBox.Show(@"I'll only 'eat' .xlsx files. Please select a .xlsx file.", @"Empty or Invalid file.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            TextBox_GlossaryFiles.Clear();
        }

        private async void ButtonTranslateAlerts_OnClick(object sender, EventArgs e)
        {
            string directoryPath = TextBox_TranslationFiles.Text;

            if (Directory.Exists(directoryPath))
            {
                var jsonFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories)
                    .ToList();

                if (jsonFiles.Count > 0)
                {
                    var translatorService = new TranslatorService();
                    await translatorService.TranslateAlertFiles(jsonFiles);
                    TextBox_TranslationFiles.Clear();
                }
                else
                {
                    MessageBox.Show(@"Folder is empty or does not contain any .json files.", @"No .json Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(string.IsNullOrWhiteSpace(directoryPath) ?
                    "Directory Path was empty!" :
                    $"Directory {directoryPath} does not exist.", @"Directory not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void ButtonTranslateLanguages_OnClick(object sender, EventArgs e)
        {
            string directoryPath = TextBox_TranslationFiles.Text;

            if (Directory.Exists(directoryPath))
            {
                var jsFiles = Directory.GetFiles(directoryPath, "*.js", SearchOption.AllDirectories)
                    .ToList();

                if (jsFiles.Count > 0)
                {
                    string[] exceptions = { "index.js", "i18n.js", "i18n-config.js" };
                    foreach (string exc in exceptions)
                    {
                        jsFiles.RemoveAll(u => u.Contains(exc));
                    }

                    var translatorService = new TranslatorService();
                    await translatorService.TranslateLanguageFiles(jsFiles);
                    TextBox_TranslationFiles.Clear();
                }
                else
                {
                    MessageBox.Show(@"Folder is empty or does not contain any .js files.", @"No .js Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(string.IsNullOrWhiteSpace(directoryPath) ?
                    "Directory Path was empty!" :
                    $"Directory {directoryPath} does not exist.", @"Directory not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MainForm_OnLoad(object sender, EventArgs e)
        {
            Rectangle workingRectangle = Screen.PrimaryScreen.WorkingArea;

            // Sets window size to a quarter of the primary - screens resolution.
            this.Size = new Size(Convert.ToInt32(0.25 * workingRectangle.Width), Convert.ToInt32(0.25 * workingRectangle.Height));

            this.Location = new Point(10, 10);
        }

        private void TextBoxFileSelect_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data!.GetData(DataFormats.FileDrop) is not string[] files || !files.Any()) return;

            if (sender is TextBox textbox)
            {
                textbox.Text = files.First();
            }
        }

        private void TextBoxFileSelect_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = e.Data!.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
        }
    }
}