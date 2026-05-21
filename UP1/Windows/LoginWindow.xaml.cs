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
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка");
                return;
            }
            var user = App.Db.Users
                              .Include(u => u.Role)
                              .FirstOrDefault(u => u.Login == login);

            if (user != null && user.Password == password)
            {

                MainWindow main = new MainWindow(user);
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Регистрация не реализована.");
        }
    }
}