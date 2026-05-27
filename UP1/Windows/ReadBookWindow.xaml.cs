using System.Windows;
using UP1;
using UP1.Windows;

namespace UP1.Views
{
    public partial class ReadBookWindow : Window
    {
        public ReadBookWindow(Book book)
        {
            if (MainWindow.CurrentUser?.IsFrozen == true)
            {
                MessageBox.Show("Вы не можете читать книги, пока аккаунт заморожен.", "Доступ запрещён");
                this.Close();
                return;
            }

            InitializeComponent();
            tbBookTitle.Text = book.Title ?? "Книга";
            tbBookText.Text = book.Contente ?? "Текст книги...";
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}