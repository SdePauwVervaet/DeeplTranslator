namespace DeeplTranslator
{
    public static class Logger
    {
        private static TextBox _textBoxLogger = null!;
        private static int counter = 0;
        
        public static void InitializeLogger(TextBox textBox)
        {
            _textBoxLogger = textBox ?? throw new ArgumentNullException($"Logger cannot be initialized because {nameof(textBox)} is null");
        }

        public static void LogMessage(string message)
        {
            if (_textBoxLogger.InvokeRequired)
            {
                _textBoxLogger.Invoke(new Action(() => LogMessage(message)));
            }
            else
            {
                _textBoxLogger.AppendText($"{message} \r\n");
                _textBoxLogger.Select(_textBoxLogger.Text.Length - 1, 0);
                _textBoxLogger.ScrollToCaret();
                _textBoxLogger.Update();
            }
        }
    }
}