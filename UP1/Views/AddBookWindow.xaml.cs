using System.Linq;
using System.Windows;
using UP1;
using UP1.Windows;

namespace UP1.Views
{
    public partial class AddBookWindow : Window
    {
        public AddBookWindow()
        {
            InitializeComponent();
            txtAuthor.Text = MainWindow.CurrentUser?.DisplayName ?? "Автор";
            LoadGenres();
        }

        private void LoadGenres()
        {
            cmbGenre.ItemsSource = App.Db.Genre.Select(g => g.GenreName).ToList();
            if (cmbGenre.Items.Count > 0)
                cmbGenre.SelectedIndex = 0;
        }

        private void BtnPublish_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Название книги обязательно!", "Ошибка");
                return;
            }

            var newBook = new Book
            {
                Title = txtTitle.Text.Trim(),
                Description = txtDescription.Text,
            };

            App.Db.Book.Add(newBook);
            App.Db.SaveChanges(); // сначала сохраняем книгу, чтобы получить ID

            // Добавляем жанр
            if (!string.IsNullOrEmpty(cmbGenre.Text))
            {
                var selectedGenreName = cmbGenre.Text;
                var genre = App.Db.Genre.FirstOrDefault(g => g.GenreName == selectedGenreName);

                if (genre != null)
                {
                    newBook.Genre.Add(genre);   // добавляем связь
                    App.Db.SaveChanges();
                }
            }

            MessageBox.Show("Книга успешно опубликована!", "Успех");
            this.DialogResult = true;
            this.Close();
        }

        private void txtTitle_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}