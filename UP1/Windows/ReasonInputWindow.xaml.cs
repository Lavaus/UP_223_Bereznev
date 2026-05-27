using System.Windows;

namespace UP1.Windows
{
    public partial class ReasonInputWindow : Window
    {
        public string Reason { get; private set; }

        public ReasonInputWindow(string title, string prompt)
        {
            InitializeComponent();
            Title = title;
            lblPrompt.Text = prompt;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Пожалуйста, укажите причину жалобы.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Reason = txtReason.Text.Trim();
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
