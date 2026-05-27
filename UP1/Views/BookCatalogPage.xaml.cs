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
            LoadGenresFilter();
            LoadBooks();
        }

        private void LoadGenresFilter()
        {
            var genres = App.Db.Genre.OrderBy(g => g.GenreName).ToList();

            cmbSort.Items.Clear();
            cmbSort.Items.Add(new ComboBoxItem { Content = "Все жанры", Tag = 0 });
            foreach (var genre in genres)
            {
                cmbSort.Items.Add(new ComboBoxItem { Content = genre.GenreName, Tag = genre.GenreID });
            }
            cmbSort.SelectedIndex = 0;
        }

        private void LoadBooks()
        {
            allBooks = App.Db.Book
                .Include(b => b.Users)
                .Include(b => b.Genre)
                .Where(b => !b.IsFrozen)
                .ToList();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (allBooks == null) return;

            var searchText = txtSearch.Text?.ToLower().Trim() ?? "";

            // Фильтр по поисковому тексту
            var filtered = allBooks.Where(b =>
                string.IsNullOrEmpty(searchText) ||
                (b.Title?.ToLower().Contains(searchText) ?? false) ||
                (b.Users?.DisplayName?.ToLower().Contains(searchText) ?? false)
            );

            // Фильтр по жанру
            if (cmbSort.SelectedItem is ComboBoxItem selected && selected.Tag is int genreId && genreId != 0)
            {
                filtered = filtered.Where(b => b.Genre != null && b.Genre.Any(g => g.GenreID == genreId));
            }

            dgBooks.ItemsSource = filtered.ToList();
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
            ApplyFilters();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

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
            ApplyFilters();
        }
    }

    public class GenreConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ICollection<Genre> genres && genres.Any())
                return string.Join(", ", genres.Select(g => g.GenreName));
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
