namespace DeeplTranslator
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<CheckBox, string> _machineTypeCheckBoxes = new Dictionary<CheckBox, string>();
        private string _currentSelectedMachineTypeAlarmFileName = string.Empty;
        private readonly TranslatorService _translatorService;
        public Form1()
        {
            InitializeComponent();
            _translatorService = new TranslatorService();
            Logger.InitializeLogger(TextBox_Logger);

            _machineTypeCheckBoxes.Add(CheckBox_BEX25, "bex10alerttranslations.json");
            _machineTypeCheckBoxes.Add(CheckBox_QSeries, "qseriesx10alerttranslations.json");
            _machineTypeCheckBoxes.Add(CheckBox_Trike, "trikealerttranslations.json");
            _machineTypeCheckBoxes.Add(CheckBox_Quad, "quadalerttranslations.json");
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

        private void ButtonSelectFolderAlertFile_OnClick(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            dialog.Description = @"Select Folder";
            dialog.ShowNewFolderButton = false;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TextBox_FileToMergeInto.Text = dialog.SelectedPath;
            }
        }

        private void ButtonSelectJsonAlertFile_OnClick(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog();
            dialog.Filter = @"Json Files|*.json";
            dialog.Title = @"Select .json File";
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            if (dialog.ShowDialog() != DialogResult.OK) return;

            if (Path.GetExtension(dialog.FileName).Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                TextBox_FileToMergeInto.Text = dialog.FileName;
            }
            else
            {
                MessageBox.Show(@"Please select a valid .json file.", @"Invalid File Type", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private async void Button_OnClickConvertNewAlerts(object sender, EventArgs e)
        {
            string fileFolderPath = TextBox_NewAlerts.Text.Trim();

            if (String.IsNullOrEmpty(fileFolderPath))
            {
                MessageBox.Show(@"Please enter a folder path.", @"Empty or invalid Folder Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (String.IsNullOrWhiteSpace(_currentSelectedMachineTypeAlarmFileName))
            {
                MessageBox.Show(@"No machine type selected, please select a machine type.", @"Unknown machine type", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            Logger.LogMessage(Environment.NewLine);
            await _translatorService.ConvertTxtFiles(fileFolderPath, _currentSelectedMachineTypeAlarmFileName);
        }

        private async void MergeJsonFiles_OnClick(object sender, EventArgs e)
        {
            string fileFolderPath = TextBox_NewAlerts.Text.Trim();
            string alertsFolderPath = TextBox_FileToMergeInto.Text.Trim();

            if (String.IsNullOrEmpty(fileFolderPath))
            {
                MessageBox.Show(@"Please enter a folder path.", @"Empty or invalid Folder Path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (String.IsNullOrWhiteSpace(_currentSelectedMachineTypeAlarmFileName))
            {
                MessageBox.Show(@"No machine type selected, please select a machine type.", @"Unknown machine type", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            await _translatorService.MergeJsonFiles(fileFolderPath, _currentSelectedMachineTypeAlarmFileName, alertsFolderPath);
        }

        private void ButtonGenerateGlossaries_OnClick(object sender, EventArgs e)
        {
            string filePath = TextBox_GlossaryFiles.Text.Trim();

            if (String.IsNullOrEmpty(filePath))
            {
                MessageBox.Show(@"Please select a .xlsx file!", @"Empty or Invalid file.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var file = new FileInfo(filePath);
            string extn = file.Extension.ToLower();
            System.Diagnostics.Debug.WriteLine("File Extension: {0}", extn);

            if (extn == ".xlsx")
            {
                _translatorService.ExcelParser.ParseExcel(file.DirectoryName!, file.Name);
                _translatorService.ExcelParser.GenerateDictionaries();
                _translatorService.UpdateDeeplGlossary();
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
                    await _translatorService.TranslateAlertFiles(jsonFiles);
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

                    await _translatorService.TranslateLanguageFiles(jsFiles);
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

        private void OnCheckChanged_MachineType(object sender, EventArgs e)
        {
            var selectedCheckBox = (CheckBox)sender;

            if (!selectedCheckBox.Checked) return;

            foreach (CheckBox checkBox in _machineTypeCheckBoxes.Keys.Where(checkBox => checkBox != selectedCheckBox))
            {
                checkBox.Checked = false;
            }
            _currentSelectedMachineTypeAlarmFileName = _machineTypeCheckBoxes[selectedCheckBox];
        }

        private async void ConvertOldKeys_OnClick(object sender, EventArgs e)
        {
            string fileFolderPath = TextBox_NewAlerts.Text.Trim();

            await _translatorService.ConvertOldKeysToNewKeys(fileFolderPath);
        }
    }
}