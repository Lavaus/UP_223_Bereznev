using System;
using System.Linq;
using System.Windows;
using UP1;

namespace UP1.Windows
{
    public partial class FrozenAppealWindow : Window
    {
        private readonly Users _user;

        public FrozenAppealWindow(Users user)
        {
            InitializeComponent();
            _user = user;
            Refresh();
        }

        private void Refresh()
        {
            // Ищем последнюю апелляцию пользователя со статусом Pending
            var existing = App.Db.UnfreezeRequest
                .Where(r => r.UserId == _user.UserID && r.Status == "Pending")
                .OrderByDescending(r => r.Created)
                .FirstOrDefault();

            if (existing != null)
            {
                // Апелляция уже есть — показываем её, скрываем форму
                tbStatusText.Text =
                    "Ваш аккаунт заморожен администратором.\n" +
                    "Апелляция уже отправлена — ожидайте ответа.";

                panelForm.Visibility     = Visibility.Collapsed;
                panelExisting.Visibility = Visibility.Visible;

                tbAppealReason.Text = $"Ваше обоснование: {existing.Reason}";
                tbAppealDate.Text   = $"Дата подачи: {existing.Created:dd.MM.yyyy HH:mm}";
            }
            else
            {
                // Апелляции нет — показываем форму
                tbStatusText.Text =
                    "Ваш аккаунт заморожен администратором.\n" +
                    "Если вы считаете это ошибкой — подайте апелляцию.";

                panelForm.Visibility     = Visibility.Visible;
                panelExisting.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string reason = txtReason.Text.Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("Заполните поле обоснования.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (reason.Length < 10)
            {
                MessageBox.Show("Опишите ситуацию подробнее (минимум 10 символов).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём запись в существующей таблице UnfreezeRequest
            // Поля: UserId, BookId (null — апелляция не по книге), Reason, Status, Created
            var appeal = new UnfreezeRequest
            {
                UserId  = _user.UserID,
                BookId  = null,
                Reason  = reason,
                Status  = "Pending",
                Created = DateTime.Now
            };

            App.Db.UnfreezeRequest.Add(appeal);
            App.Db.SaveChanges();

            MessageBox.Show(
                "Апелляция успешно отправлена!\nАдминистратор рассмотрит её в ближайшее время.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

            Refresh();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
