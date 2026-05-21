using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP1;
using UP1.Windows;

namespace UP1.Views
{
    public partial class BookDetailsPage : Page
    {
        private Book currentBook;

        public BookDetailsPage(Book book)
        {
            InitializeComponent();
            currentBook = book;

            // Защита от замороженных пользователей
            if (MainWindow.CurrentUser?.IsFrozen == true)
            {
                MessageBox.Show("Ваш аккаунт заморожен. Просмотр книг недоступен.", "Доступ ограничен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NavigationService.GoBack();
                return;
            }

            LoadBookInfo();
            LoadReviews();
        }

        private void LoadBookInfo()
        {
            // Обложка
            if (!string.IsNullOrEmpty(currentBook.CoverPath))
            {
                
                tbCover.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(currentBook.CoverPath));
                
  
            }

            tbTitle.Text = currentBook.Title ?? "Без названия";
            tbAuthor.Text = $"Автор: {currentBook.Users?.DisplayName ?? "Неизвестен"}";

            tbDescription.Text = currentBook.Description ?? "Описание отсутствует.";
        }

        private void LoadReviews()
        {
            var reviews = App.Db.Review
                .Where(r => r.BookID == currentBook.BookID)
                .Include(r => r.Users)
                .OrderByDescending(r => r.Created)
                .ToList();

  
        }

        // ==================== ОБРАБОТЧИКИ ====================

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            new ReadBookWindow(currentBook).ShowDialog();
        }

        private void BtnReportBook_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Жалоба на книгу отправлена администратору.", "Жалоба отправлена");
        }

        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.CurrentUser?.Role?.RoleName?.ToLower() != "Администратор")
            {
                MessageBox.Show("Только администратор может замораживать книги.");
                return;
            }

            MessageBox.Show($"Книга «{currentBook.Title}» заморожена.", "Готово");
            // Здесь можно добавить логику заморозки книги
        }

        private void CmbRating_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можно оставить пустым или добавить логику
        }

        private void BtnPublishReview_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReviewText.Text))
            {
                MessageBox.Show("Введите текст отзыва!");
                return;
            }

            var review = new Review
            {
                BookID = currentBook.BookID,
                UserID = MainWindow.CurrentUser?.UserID ?? 0,
                ReviewText = txtReviewText.Text.Trim(),
                Rating = 5 - cmbRating.SelectedIndex,
                Created = DateTime.Now
            };

            App.Db.Review.Add(review);
            App.Db.SaveChanges();

            MessageBox.Show("Отзыв успешно опубликован!");
            txtReviewText.Clear();
            LoadReviews();
        }
    }
}