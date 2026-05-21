using System.Linq;
using System.Windows;
using UP1;

namespace UP1.Views
{
    public partial class EditBookWindow : Window
    {
        private Book currentBook;

        public EditBookWindow(Book book)
        {
            InitializeComponent();
            currentBook = book;
            LoadBookData();
        }

        private void LoadBookData()
        {
            txtTitle.Text = currentBook.Title;
            txtDescription.Text = currentBook.Description;

            // Загружаем жанры в ComboBox
            cmbGenre.ItemsSource = App.Db.Genre.Select(g => g.GenreName).ToList();

            // Показываем текущий жанр книги
            var firstGenre = currentBook.Genre.FirstOrDefault();
            if (firstGenre != null)
                cmbGenre.Text = firstGenre.GenreName;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Название обязательно!");
                return;
            }

            currentBook.Title = txtTitle.Text.Trim();
            currentBook.Description = txtDescription.Text;

            // Обновляем жанры
            currentBook.Genre.Clear();

            if (!string.IsNullOrEmpty(cmbGenre.Text))
            {
                var genre = App.Db.Genre.FirstOrDefault(g => g.GenreName == cmbGenre.Text);
                if (genre != null)
                {
                    currentBook.Genre.Add(genre);
                }
            }

            App.Db.SaveChanges();

            MessageBox.Show("Книга успешно обновлена!", "Успех");
            this.DialogResult = true;
            this.Close();
        }
    }
}