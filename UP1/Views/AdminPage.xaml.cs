using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using UP1;

namespace UP1.Views
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            dgUsers.ItemsSource = App.Db.Users.Include(u => u.Role).ToList();
            dgComplaints.ItemsSource = App.Db.Complain.Include(c => c.Book).Include(c => c.Users).ToList();
            dgRoleRequests.ItemsSource = App.Db.RoleRequest
                .Include(r => r.Users)
                .Where(r => r.Status == "Pending")
                .ToList();
        }

        private void BtnApproveRole_Click(object sender, RoutedEventArgs e)
        {
            if (dgRoleRequests.SelectedItem is RoleRequest request)
            {
                request.Status = "Approved";
                request.Users.RoleID = 2; // 2 = Author
                App.Db.SaveChanges();
                MessageBox.Show("Заявка одобрена. Пользователь теперь Автор.");
                LoadData();
            }
        }

        private void BtnRejectRole_Click(object sender, RoutedEventArgs e)
        {
            if (dgRoleRequests.SelectedItem is RoleRequest request)
            {
                request.Status = "Rejected";
                App.Db.SaveChanges();
                MessageBox.Show("Заявка отклонена.");
                LoadData();
            }
        }

        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is Users user)
                MessageBox.Show($"Смена роли для {user.DisplayName} (в разработке)");
        }

        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (dgUsers.SelectedItem is Users user)
            {
                user.IsFrozen = true;
                App.Db.SaveChanges();
                LoadData();
                MessageBox.Show("Пользователь заморожен");
            }
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}