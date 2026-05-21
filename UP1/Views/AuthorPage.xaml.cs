using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using UP1;
using UP1.Windows;

namespace UP1.Views
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            LoadAuthorBooks();
        }

        private void LoadAuthorBooks()
        {
            authorBooksPanel.Children.Clear();

            var user = MainWindow.CurrentUser;
            if (user == null) return;

            // Загружаем только книги текущего автора
            var myBooks = App.Db.Book
                .Include(b => b.Users)
                .Include(b => b.Genre)
                .Where(b => b.Users.UserID == user.UserID)
                .ToList();

            foreach (var book in myBooks)
            {
                var card = CreateBookCard(book);
                authorBooksPanel.Children.Add(card);
            }

            if (myBooks.Count == 0)
            {
                var tb = new TextBlock
                {
                    Text = "У вас пока нет опубликованных книг",
                    Foreground = Brushes.Gray,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(50)
                };
                authorBooksPanel.Children.Add(tb);
            }
        }

        private Border CreateBookCard(Book book)
        {
            var border = new Border
            {
                Width = 170,
                Height = 290,
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(12),
                Cursor = Cursors.Hand
            };

            var stack = new StackPanel { Margin = new Thickness(10) };

            // Обложка
            var image = new Image
            {
                Height = 160,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(0, 0, 0, 10)
            };

            if (!string.IsNullOrEmpty(book.CoverPath))
            {
                try
                {
                    image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(book.CoverPath));
                }
                catch { }
            }
            else
            {
                image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("https://via.placeholder.com/150?text=📖"));
            }

            var title = new TextBlock
            {
                Text = book.Title ?? "Без названия",
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            };

            var author = new TextBlock
            {
                Text = "Моя книга",
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.LightGreen,
                FontSize = 13
            };

            var btnEdit = new Button
            {
                Content = "✏️ Редактировать",
                Height = 32,
                Margin = new Thickness(0, 8, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                Foreground = Brushes.White
            };

            stack.Children.Add(image);
            stack.Children.Add(title);
            stack.Children.Add(author);
            stack.Children.Add(btnEdit);

            border.Child = stack;

            // Открыть книгу
            border.MouseLeftButtonUp += (s, e) =>
            {
                if (s != btnEdit)
                    NavigationService.Navigate(new BookDetailsPage(book));
            };
            // Редактировать — ТОЛЬКО свою книгу
            btnEdit.Click += (s, e) =>
            {
                if (book.Users?.UserID == MainWindow.CurrentUser?.UserID)
                {
                    var editWindow = new EditBookWindow(book);
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadAuthorBooks(); // обновляем список
                    }
                }
                else
                {
                    MessageBox.Show("Вы можете редактировать только свои книги!", "Ошибка доступа",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            return border;
        }

        private void BtnAddNewBook_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser?.IsFrozen == true)
            {
                MessageBox.Show("Замороженный аккаунт не может публиковать книги.", "Доступ запрещён");
                return;
            }

            var addWindow = new AddBookWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadAuthorBooks();
            }
        }
    }
}