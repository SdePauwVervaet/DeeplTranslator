namespace DeeplTranslator
{
    public static class Logger
    {
        private static TextBox _textBox = null!;
        
        public static void InitializeLogger(TextBox textBox)
        {
            _textBox = textBox ?? throw new ArgumentNullException($"Logger cannot be initialized because {nameof(textBox)} is null");
        }

        public static async Task LogMessage(string message)
        {
            await Task.Run(() =>
            {
                _textBox.AppendText($"{message} \r\n");
                _textBox.Select(_textBox.Text.Length - 1, 0);
                _textBox.ScrollToCaret();
                _textBox.Update();
            });
        }
    }
}