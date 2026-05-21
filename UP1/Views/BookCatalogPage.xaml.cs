using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP1;

namespace UP1.Views
{
    public partial class BookCatalogPage : Page
    {
        private List<Book> allBooks;

        public BookCatalogPage()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            allBooks = App.Db.Book
                .Include(b => b.Users)
                .Include(b => b.Genre)
                .ToList();

            dgBooks.ItemsSource = allBooks;
        }

        private void DgBooks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgBooks.SelectedItem is Book book)
            {
                NavigationService.Navigate(new BookDetailsPage(book));
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allBooks == null) return;

            var searchText = txtSearch.Text.ToLower().Trim();

            var filtered = allBooks.Where(b =>
                (b.Title?.ToLower().Contains(searchText) ?? false) ||
                (b.Users?.DisplayName?.ToLower().Contains(searchText) ?? false)
            ).ToList();

            dgBooks.ItemsSource = filtered;
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        // Для отображения жанров в DataGrid создадим конвертер на лету
        private void DgBooks_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Genre")
            {
                e.Column = new DataGridTextColumn
                {
                    Header = "Жанр",
                    Binding = new System.Windows.Data.Binding("Genre")
                    {
                        Converter = new GenreConverter()
                    },
                    Width = 150
                };
            }
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    // Простой конвертер жанров
    public class GenreConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ICollection<Genre> genres && genres.Any())
            {
                return string.Join(", ", genres.Select(g => g.GenreName));
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}