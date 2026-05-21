using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP1;
using UP1.Windows;

namespace UP1.Views
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserInfo();
        }

        private void LoadUserInfo()
        {
            var user = MainWindow.CurrentUser;
            if (user == null) return;

            tbFullName.Text = $"Имя: {user.DisplayName}";
            tbLogin.Text = $"Логин: {user.Login}";
            tbEmail.Text = $"Email: {user.Email}";
            tbRole.Text = $"Роль: {user.Role?.RoleName ?? "Читатель"}";

            // Кнопка заявки на автора — только у обычных пользователей
            bool isRegularUser = user.Role?.RoleName?.ToLower() == "user" || string.IsNullOrEmpty(user.Role?.RoleName);
            btnApplyAuthor.Visibility = isRegularUser && !user.IsFrozen ? Visibility.Visible : Visibility.Collapsed;

            if (user.IsFrozen)
            {
                tbFreezeWarning.Visibility = Visibility.Visible;
                tbFreezeWarning.Text = $"⚠️ Аккаунт заморожен!";
            }
        }

        private void BtnApplyAuthor_Click(object sender, RoutedEventArgs e)
        {
            var user = MainWindow.CurrentUser;
            if (user == null) return;

            // Проверяем, нет ли уже активной заявки
            var existingRequest = App.Db.RoleRequest
                .FirstOrDefault(r => r.UserID == user.UserID && r.Status == "Pending");

            if (existingRequest != null)
            {
                MessageBox.Show("У вас уже есть активная заявка на роль Автора.\nОжидайте рассмотрения администратором.", "Информация");
                return;
            }

            var request = new RoleRequest
            {
                UserID = user.UserID,
                RoleID = 2, // Предполагаем, что RoleID = 2 — это "Author"
                Status = "Pending",
                Created = DateTime.Now
            };

            App.Db.RoleRequest.Add(request);
            App.Db.SaveChanges();

            MessageBox.Show("Заявка на роль Автора успешно отправлена!\nАдминистратор рассмотрит её в ближайшее время.", "Успешно");
        }
    }
}