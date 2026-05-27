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

            bool isRegularUser = user.Role?.RoleName?.ToLower() == "user" || string.IsNullOrEmpty(user.Role?.RoleName);
           

        }

   
        
    }
}