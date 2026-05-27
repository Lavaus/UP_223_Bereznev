using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP1;
using UP1.Windows;

namespace UP1.Views
{
    public partial class BookListsPage : Page
    {
        private string currentShelf = "В планах";
        private List<Book> currentBooks;

        public BookListsPage()
        {
            InitializeComponent();
            LoadShelf("В планах");
        }

        private void LoadShelf(string shelf)
        {
            currentShelf = shelf;
            var user = MainWindow.CurrentUser;
            if (user == null) return;

            currentBooks = App.Db.ReadingList
                .Where(r => r.UserID == user.UserID && r.Status == shelf)
                .Include(r => r.Book)
                .Include(r => r.Book.Users)
                .Select(r => r.Book)
                .ToList();

            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (currentBooks == null) return;

            var filtered = currentBooks.AsEnumerable();

            // Поиск
            var search = txtSearchLists.Text?.ToLower().Trim() ?? "";
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(b =>
                    (b.Title?.ToLower().Contains(search) ?? false) ||
                    (b.Users?.DisplayName?.ToLower().Contains(search) ?? false));

            // Сортировка
            if (cmbSortLists.SelectedIndex == 1)
                filtered = filtered.OrderBy(b => b.Users?.DisplayName);
            else
                filtered = filtered.OrderBy(b => b.Title);

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

        private void TxtSearchLists_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void CmbSortLists_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void BtnPlan_Click(object sender, RoutedEventArgs e) => LoadShelf("В планах");
        private void BtnReading_Click(object sender, RoutedEventArgs e) => LoadShelf("Читаю");
        private void BtnFinished_Click(object sender, RoutedEventArgs e) => LoadShelf("Прочитано");
        private void BtnDropped_Click(object sender, RoutedEventArgs e) => LoadShelf("Заброшено");
    }
}
