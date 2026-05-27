using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using UP1;

namespace UP1.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string displayName = txtDisplayName.Text.Trim();
            string login       = txtLogin.Text.Trim();
            string email       = txtEmail.Text.Trim();
            string password    = txtPassword.Password;
            string confirm     = txtPasswordConfirm.Password;

            // --- Валидация ---
            if (string.IsNullOrWhiteSpace(displayName) ||
                string.IsNullOrWhiteSpace(login)       ||
                string.IsNullOrWhiteSpace(email)       ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный Email.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (App.Db.Users.Any(u => u.Login == login))
            {
                MessageBox.Show("Этот логин уже занят. Выберите другой.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (App.Db.Users.Any(u => u.Email == email))
            {
                MessageBox.Show("Этот Email уже зарегистрирован.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ищем роль читателя по имени (без изменения БД — роли уже существуют)
            var readerRole = App.Db.Role.FirstOrDefault(r =>
                r.RoleName.ToLower() == "reader"   ||
                r.RoleName.ToLower() == "user"     ||
                r.RoleName.ToLower() == "читатель");

            // Если имена не совпали — берём роль с наименьшим RoleID
            // (предполагается, что читатель — первая роль)
            if (readerRole == null)
                readerRole = App.Db.Role.OrderBy(r => r.RoleID).FirstOrDefault();

            if (readerRole == null)
            {
                MessageBox.Show(
                    "В базе данных не найдена роль читателя.\nОбратитесь к администратору.",
                    "Ошибка конфигурации", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newUser = new Users
            {
                DisplayName = displayName,
                Login       = login,
                Email       = email,
                Password    = password,
                RoleID      = readerRole.RoleID,
                IsFrozen    = false,
                Created     = DateTime.Now
            };

            App.Db.Users.Add(newUser);
            App.Db.SaveChanges();

            MessageBox.Show(
                $"Аккаунт успешно создан!\nДобро пожаловать, {displayName}!",
                "Регистрация завершена", MessageBoxButton.OK, MessageBoxImage.Information);

            new LoginWindow().Show();
            this.Close();
        }

        private void BtnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
