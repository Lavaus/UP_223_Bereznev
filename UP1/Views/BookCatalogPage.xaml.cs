using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            cmbGenre.Items.Clear();
            cmbGenre.Items.Add(new ComboBoxItem { Content = "Все жанры", Tag = 0 });
            foreach (var genre in genres)
                cmbGenre.Items.Add(new ComboBoxItem { Content = genre.GenreName, Tag = genre.GenreID });

            cmbGenre.SelectedIndex = 0;
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

            var filtered = allBooks.AsEnumerable();

            var searchText = txtSearch.Text?.ToLower().Trim() ?? "";
            if (!string.IsNullOrEmpty(searchText))
                filtered = filtered.Where(b =>
                    (b.Title?.ToLower().Contains(searchText) ?? false) ||
                    (b.Users?.DisplayName?.ToLower().Contains(searchText) ?? false));

  
            if (cmbGenre.SelectedItem is ComboBoxItem genreItem &&
                genreItem.Tag is int genreId && genreId != 0)
                filtered = filtered.Where(b =>
                    b.Genre != null && b.Genre.Any(g => g.GenreID == genreId));
            filtered = cmbSort.SelectedIndex == 1
                ? filtered.OrderBy(b => b.Users?.DisplayName)
                : filtered.OrderBy(b => b.Title);

            var result = filtered.ToList();

            booksControl.ItemsSource = result;

            tbEmpty.Visibility = result.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnBookCard_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Book book)
                NavigationService.Navigate(new BookDetailsPage(book));
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void CmbGenre_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadBooks();
    }
}
