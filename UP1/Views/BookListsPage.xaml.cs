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

            var readingLists = App.Db.ReadingList
                .Where(r => r.UserID == user.UserID)
                .Include(r => r.Book)
                .ToList();

            var books = readingLists.Select(r => r.Book).ToList();

            DisplayBooks(books);
        }

        private void DisplayBooks(System.Collections.Generic.List<Book> books)
        {
            listsBooksPanel.Children.Clear();

            foreach (var book in books)
            {
                var card = CreateBookCard(book);
                listsBooksPanel.Children.Add(card);
            }
        }

        private Border CreateBookCard(Book book)
        {
            var border = new Border
            {
                Width = 170,
                Height = 260,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 48)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(12),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var stack = new StackPanel { Margin = new Thickness(10) };

            var cover = new TextBlock { Text = "📖", FontSize = 60, HorizontalAlignment = HorizontalAlignment.Center };
            var title = new TextBlock
            {
                Text = book.Title ?? "Без названия",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = System.Windows.Media.Brushes.White
            };
            var author = new TextBlock
            {
                Text = book.Users?.DisplayName ?? "Автор",
                TextAlignment = TextAlignment.Center,
                Foreground = System.Windows.Media.Brushes.LightGray
            };

            stack.Children.Add(cover);
            stack.Children.Add(title);
            stack.Children.Add(author);
            border.Child = stack;

            border.MouseLeftButtonUp += (s, e) =>
            {
                NavigationService.Navigate(new BookDetailsPage(book));
            };

            return border;
        }

        private void BtnPlan_Click(object sender, RoutedEventArgs e) => LoadShelf("В планах");
        private void BtnReading_Click(object sender, RoutedEventArgs e) => LoadShelf("Читаю");
        private void BtnFinished_Click(object sender, RoutedEventArgs e) => LoadShelf("Прочитано");
        private void BtnDropped_Click(object sender, RoutedEventArgs e) => LoadShelf("Заброшено");
    }
}