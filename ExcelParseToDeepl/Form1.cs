using DeeplTranslator;

namespace ExcelParseToDeepl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; // get all files dropped 
            if (files != null && files.Any())
                textBox1.Text = files.First(); //select the first one  
        }

        private void textBox1_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                textBox1.Text = "Select a file by dropping it here...";
                textBox1.Update();
            }
            else
            {
                FileInfo file = new FileInfo(textBox1.Text);
                string extn = file.Extension;
                System.Diagnostics.Debug.WriteLine("File Extension: {0}", extn);

                if (extn == ".xlsx")
                {
                    Parser parser = new Parser();
                    parser.ParseExcel(file.DirectoryName, file.Name);
                    parser.GenerateDictionaries((message) =>
                    {
                        textBox3.Text = message;
                        textBox3.Update();
                    });
                    parser.UpdateDeeplGlossary((message) =>
                    {
                        textBox3.Text += $"{message} \r\n";
                        textBox3.Update();
                    });


                }
                else
                {
                    textBox1.Text = "I'll only 'eat' .xlsx files. Select a file by dropping it here...";
                    textBox1.Update();
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Open_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textBox1.Text = dialog.FileName;
                }
            }
        }
        private void Open_Click_2(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();

                textBox2.AppendText(dialog.SelectedPath);
            }
        }


        private void textBox2_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; // get all files dropped 
            if (files != null && files.Any())
                textBox1.Text = files.First(); //select the first one  
        }

        private void textBox2_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }



        private async void button3_Click(object sender, EventArgs e)
        {
            List<string> selectedFiles = new List<string>();
            List<string> files = Directory.GetFiles(textBox2.Text, "*.*", SearchOption.AllDirectories)
                    .Where(file => new string[] { ".json" }
                    .Contains(Path.GetExtension(file)))
                    .ToList();
            //System.Windows.Forms.MessageBox.Show("Files found: " + files.Count, "Message");
            selectedFiles = files;

            Parser parser = new Parser();
            await parser.TranslateAlertFiles(selectedFiles, (message) =>
            {
                textBox3.Text += $"{message} \r\n";
                textBox3.Select(textBox3.Text.Length - 1, 0);
                textBox3.ScrollToCaret();
                textBox3.Update();
                return Task.CompletedTask;
            });
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button4_Click(object sender, EventArgs e)
        {
            List<string> files = Directory.GetFiles(textBox2.Text, "*.*", SearchOption.AllDirectories)
                    .Where(file => new string[] { ".js" }
                    .Contains(Path.GetExtension(file)))
                    .ToList();

            string[] exceptions = { "index.js", "i18n.js", "i18n-config.js" };
            foreach (string exc in exceptions)
            {
                files.RemoveAll(u => u.Contains(exc));
            }

            Parser parser = new Parser();
            await parser.TranslateLanguageFiles(files, (message) =>
            {
                textBox3.Text += $"{message} \r\n";
                textBox3.Select(textBox3.Text.Length - 1, 0);
                textBox3.ScrollToCaret();
                textBox3.Update();
                return Task.CompletedTask;
            });
        }

    }
}