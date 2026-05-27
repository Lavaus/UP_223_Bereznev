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
            if (!string.IsNullOrEmpty(currentBook.CoverPath))
                tbCover.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(currentBook.CoverPath));

            tbTitle.Text = currentBook.Title ?? "Без названия";
            tbAuthor.Text = $"Автор: {currentBook.Users?.DisplayName ?? "Неизвестен"}";
            tbDescription.Text = currentBook.Description ?? "Описание отсутствует.";

            LoadReadingListStatus();
        }

        private void LoadReadingListStatus()
        {
            var userId = MainWindow.CurrentUser?.UserID ?? 0;
            var entry = App.Db.ReadingList
                .FirstOrDefault(r => r.UserID == userId && r.BookID == currentBook.BookID);

            if (entry == null)
            {
                cmbReadingList.SelectedIndex = 0; 
                btnAddToList.Content = "Добавить в список";
                return;
            }

            switch (entry.Status)
            {
                case "В планах": cmbReadingList.SelectedIndex = 1; break;
                case "Читаю": cmbReadingList.SelectedIndex = 2; break;
                case "Прочитано": cmbReadingList.SelectedIndex = 3; break;
                case "Заброшено": cmbReadingList.SelectedIndex = 4; break;
                default: cmbReadingList.SelectedIndex = 0; break;
            }
            btnAddToList.Content = "Обновить список";
        }

        private void LoadReviews()
        {
            var reviews = App.Db.Review
                .Where(r => r.BookID == currentBook.BookID)
                .Include(r => r.Users)
                .OrderByDescending(r => r.Created)
                .ToList();

            reviewsControl.ItemsSource = reviews;
            tbNoReviews.Visibility = reviews.Count == 0
                ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            new ReadBookWindow(currentBook).ShowDialog();
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (cmbReadingList.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите список для добавления.", "Не выбрано",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var statuses = new[] { "", "В планах", "Читаю", "Прочитано", "Заброшено" };
            string selectedStatus = statuses[cmbReadingList.SelectedIndex];
            int userId = MainWindow.CurrentUser?.UserID ?? 0;

            var existing = App.Db.ReadingList
                .FirstOrDefault(r => r.UserID == userId && r.BookID == currentBook.BookID);

            if (existing != null)
            {
                existing.Status = selectedStatus;
            }
            else
            {
                App.Db.ReadingList.Add(new ReadingList
                {
                    UserID = userId,
                    BookID = currentBook.BookID,
                    Status = selectedStatus,
                    Added = DateTime.Now
                });
            }

            App.Db.SaveChanges();
            btnAddToList.Content = "Обновить список";
            MessageBox.Show($"Книга добавлена в «{selectedStatus}».", "Готово");
        }

        private void BtnReportBook_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReasonInputWindow("Жалоба на книгу", "Укажите причину жалобы на книгу:");
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Reason))
            {
                App.Db.Complain.Add(new Complain
                {
                    UserID = MainWindow.CurrentUser?.UserID ?? 0,
                    BookId = currentBook.BookID,
                    Reason = dialog.Reason.Trim(),
                    Created = DateTime.Now
                });
                App.Db.SaveChanges();
                MessageBox.Show("Жалоба на книгу отправлена администратору.", "Жалоба отправлена");
            }
        }

        private void BtnReportReview_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is Review review)
            {
                var dialog = new ReasonInputWindow("Жалоба на отзыв", "Укажите причину жалобы на отзыв:");
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.Reason))
                {
                    App.Db.Complain.Add(new Complain
                    {
                        UserID = MainWindow.CurrentUser?.UserID ?? 0,
                        ReviewId = review.ReviewID,
                        Reason = dialog.Reason.Trim(),
                        Created = DateTime.Now
                    });
                    App.Db.SaveChanges();
                    MessageBox.Show("Жалоба на отзыв отправлена администратору.", "Жалоба отправлена");
                }
            }
        }

        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            string roleName = (MainWindow.CurrentUser?.Role?.RoleName ?? "").ToLower().Trim();
            if (roleName != "администратор" && roleName != "administrator" && roleName != "admin")
            {
                MessageBox.Show("Только администратор может замораживать книги.");
                return;
            }
            currentBook.IsFrozen = true;
            App.Db.SaveChanges();
            MessageBox.Show($"Книга «{currentBook.Title}» заморожена.", "Готово");
        }

        private void CmbRating_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void BtnPublishReview_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReviewText.Text))
            {
                MessageBox.Show("Введите текст отзыва!");
                return;
            }

            App.Db.Review.Add(new Review
            {
                BookID = currentBook.BookID,
                UserID = MainWindow.CurrentUser?.UserID ?? 0,
                ReviewText = txtReviewText.Text.Trim(),
                Rating = 10 - cmbRating.SelectedIndex,
                Created = DateTime.Now
            });
            App.Db.SaveChanges();

            MessageBox.Show("Отзыв успешно опубликован!");
            txtReviewText.Clear();
            LoadReviews();
        }
    }
}
