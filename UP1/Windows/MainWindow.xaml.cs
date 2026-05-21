using System.Windows;
using UP1.Views;

namespace UP1.Windows
{
    public partial class MainWindow : Window
    {
        public static Users CurrentUser { get; private set; }

        public MainWindow(Users user)
        {
            InitializeComponent();
            CurrentUser = user;
            ApplyRoleVisibility();
            LoadDefaultPage();
        }

        private void ApplyRoleVisibility()
        {
            if (CurrentUser == null) return;

            string roleName = CurrentUser.Role?.RoleName?.ToLower() ?? "";

            // Кнопки ролей
            btnAuthor.Visibility = (roleName == "author" || roleName == "administrator")
                ? Visibility.Visible : Visibility.Collapsed;

            btnAdmin.Visibility = (roleName == "administrator")
                ? Visibility.Visible : Visibility.Collapsed;

            // Заморозка аккаунта
            if (CurrentUser.IsFrozen)
            {
                btnFreezeWarning.Visibility = Visibility.Visible;
                btnCatalog.IsEnabled = false;
                btnLists.IsEnabled = false;
                btnAuthor.IsEnabled = false;
            }
            else
            {
                btnFreezeWarning.Visibility = Visibility.Collapsed;
                btnCatalog.IsEnabled = true;
                btnLists.IsEnabled = true;
                btnAuthor.IsEnabled = true;
            }
        }

        private void LoadDefaultPage()
        {
            ContentFrame.Navigate(new BookCatalogPage());
            tbPageTitle.Text = "Каталог книг";
        }

        // Обработчики кнопок (оставляем)
        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser?.IsFrozen == true) return;
            ContentFrame.Navigate(new BookCatalogPage());
            tbPageTitle.Text = "Каталог книг";
        }

        private void BtnLists_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser?.IsFrozen == true) return;
            ContentFrame.Navigate(new BookListsPage());
            tbPageTitle.Text = "Мои списки книг";
        }

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser?.IsFrozen == true) return;
            ContentFrame.Navigate(new AuthorPage());
            tbPageTitle.Text = "Мои книги";
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdminPage());
            tbPageTitle.Text = "Администрирование";
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ProfilePage());
            tbPageTitle.Text = "Профиль";
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из аккаунта?", "Выход", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                new LoginWindow().Show();
                this.Close();
            }
        }

        private void BtnFreezeWarning_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Аккаунт заморожен!\nОбратитесь к администратору.",
                "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}