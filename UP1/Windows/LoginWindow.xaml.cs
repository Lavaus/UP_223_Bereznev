using System.Linq;
using System.Windows;
using System.Data.Entity;
using UP1;

namespace UP1.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login    = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Поиск по логину ИЛИ email
            var user = App.Db.Users
                           .Include(u => u.Role)
                           .FirstOrDefault(u => u.Login == login || u.Email == login);

            if (user == null || user.Password != password)
            {
                MessageBox.Show("Неверный логин или пароль!", "Ошибка входа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Замороженный пользователь — показываем окно апелляции
            if (user.IsFrozen)
            {
                // Проверяем наличие активной апелляции (используем существующее поле Status)
                bool hasPending = App.Db.UnfreezeRequest
                    .Any(r => r.UserId == user.UserID && r.Status == "Pending");

                string info = hasPending
                    ? "Ваша апелляция уже отправлена и ожидает рассмотрения администратором."
                    : "Вы можете подать апелляцию для разморозки аккаунта.";

                var result = MessageBox.Show(
                    $"⚠️ Ваш аккаунт заморожен.\n\n{info}\n\nОткрыть страницу апелляции?",
                    "Аккаунт заморожен",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    new FrozenAppealWindow(user).Show();
                    this.Close();
                }
                return;
            }

            new MainWindow(user).Show();
            this.Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            new RegisterWindow().Show();
            this.Close();
        }
    }
}
