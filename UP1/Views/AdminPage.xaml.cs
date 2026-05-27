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
            // Пользователи — все
            dgUsers.ItemsSource = App.Db.Users
                .Include(u => u.Role)
                .OrderBy(u => u.UserID)
                .ToList();

            // Жалобы — все (у Complain нет поля Status в БД, показываем всё)
            dgComplaints.ItemsSource = App.Db.Complain
                .Include(c => c.Book)
                .Include(c => c.Book.Users)   // автор книги
                .Include(c => c.Users)         // кто подал жалобу
                .OrderByDescending(c => c.Created)
                .ToList();

            // Апелляции — только ожидающие (Status есть в UnfreezeRequest)
            dgAppeals.ItemsSource = App.Db.UnfreezeRequest
                .Include(r => r.Users)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.Created)
                .ToList();

            // Заявки на роль автора — только ожидающие
            dgRoleRequests.ItemsSource = App.Db.RoleRequest
                .Include(r => r.Users)
                .Where(r => r.Status == "Pending")
                .ToList();
        }

        // ==================== ПОЛЬЗОВАТЕЛИ ====================

        private void BtnFreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgUsers.SelectedItem is Users user))
            {
                MessageBox.Show("Выберите пользователя в таблице.", "Подсказка");
                return;
            }

            if (user.IsFrozen)
            {
                MessageBox.Show("Аккаунт уже заморожен.", "Информация");
                return;
            }

            if (MessageBox.Show($"Заморозить аккаунт «{user.DisplayName}»?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                user.IsFrozen = true;
                App.Db.SaveChanges();
                MessageBox.Show("Аккаунт заморожен.", "Готово");
                LoadData();
            }
        }

        private void BtnUnfreezeUser_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgUsers.SelectedItem is Users user))
            {
                MessageBox.Show("Выберите пользователя в таблице.", "Подсказка");
                return;
            }

            if (!user.IsFrozen)
            {
                MessageBox.Show("Аккаунт уже активен.", "Информация");
                return;
            }

            if (MessageBox.Show($"Разморозить аккаунт «{user.DisplayName}»?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                user.IsFrozen = false;
                App.Db.SaveChanges();
                MessageBox.Show("Аккаунт разморожен.", "Готово");
                LoadData();
            }
        }

        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgUsers.SelectedItem is Users user))
            {
                MessageBox.Show("Выберите пользователя в таблице.", "Подсказка");
                return;
            }

            var roles = App.Db.Role.ToList();

            // Строим простой диалог выбора роли через InputBox-стиль
            var win = new Window
            {
                Title  = $"Смена роли — {user.DisplayName}",
                Width  = 320, Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode  = ResizeMode.NoResize,
                Background  = new System.Windows.Media.SolidColorBrush(
                                  System.Windows.Media.Color.FromRgb(45, 45, 45))
            };

            var panel = new StackPanel { Margin = new Thickness(20) };

            panel.Children.Add(new TextBlock
            {
                Text       = $"Пользователь: {user.DisplayName}\nТекущая роль: {user.Role?.RoleName ?? "—"}",
                Foreground = System.Windows.Media.Brushes.White,
                Margin     = new Thickness(0, 0, 0, 14)
            });

            panel.Children.Add(new TextBlock
            {
                Text       = "Новая роль:",
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin     = new Thickness(0, 0, 0, 4)
            });

            var combo = new ComboBox
            {
                Height            = 34,
                ItemsSource       = roles,
                DisplayMemberPath = "RoleName",
                SelectedValuePath = "RoleID",
                SelectedValue     = user.RoleID,
                Background        = new System.Windows.Media.SolidColorBrush(
                                        System.Windows.Media.Color.FromRgb(60, 60, 60)),
                Foreground        = System.Windows.Media.Brushes.White,
                Margin            = new Thickness(0, 0, 0, 16)
            };

            var btnSave = new Button
            {
                Content    = "Сохранить",
                Height     = 38,
                Background = new System.Windows.Media.SolidColorBrush(
                                 System.Windows.Media.Color.FromRgb(76, 175, 80)),
                Foreground = System.Windows.Media.Brushes.White
            };

            btnSave.Click += (s, ev) =>
            {
                if (combo.SelectedItem is Role selected)
                {
                    user.RoleID = selected.RoleID;
                    user.Role   = selected;
                    win.DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Выберите роль.", "Ошибка");
                }
            };

            panel.Children.Add(combo);
            panel.Children.Add(btnSave);
            win.Content = panel;

            if (win.ShowDialog() == true)
            {
                App.Db.SaveChanges();
                MessageBox.Show($"Роль пользователя «{user.DisplayName}» изменена.", "Готово");
                LoadData();
            }
        }

        // ==================== ЖАЛОБЫ ====================

        private void BtnFreezeBookAuthor_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgComplaints.SelectedItem is Complain complaint))
            {
                MessageBox.Show("Выберите жалобу в таблице.", "Подсказка");
                return;
            }

            var author = complaint.Book?.Users;
            if (author == null)
            {
                MessageBox.Show("Не удалось определить автора книги.", "Ошибка");
                return;
            }

            if (author.IsFrozen)
            {
                MessageBox.Show($"Аккаунт автора «{author.DisplayName}» уже заморожен.", "Информация");
                return;
            }

            if (MessageBox.Show(
                    $"Заморозить аккаунт автора «{author.DisplayName}»\nпо жалобе на книгу «{complaint.Book.Title}»?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                author.IsFrozen = true;
                App.Db.SaveChanges();
                MessageBox.Show("Аккаунт автора заморожен.", "Готово");
                LoadData();
            }
        }

        private void BtnFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgComplaints.SelectedItem is Complain complaint))
            {
                MessageBox.Show("Выберите жалобу в таблице.", "Подсказка");
                return;
            }

            var book = complaint.Book;
            if (book == null)
            {
                MessageBox.Show("Книга не найдена.", "Ошибка");
                return;
            }

            if (book.IsFrozen)
            {
                MessageBox.Show($"Книга «{book.Title}» уже заморожена.", "Информация");
                return;
            }

            if (MessageBox.Show(
                    $"Заморозить книгу «{book.Title}»?\nОна будет скрыта из каталога.",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                book.IsFrozen = true;
                App.Db.SaveChanges();
                MessageBox.Show("Книга заморожена и скрыта из каталога.", "Готово");
                LoadData();
            }
        }

        // ==================== АПЕЛЛЯЦИИ ====================

        private void BtnApproveAppeal_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgAppeals.SelectedItem is UnfreezeRequest appeal))
            {
                MessageBox.Show("Выберите апелляцию в таблице.", "Подсказка");
                return;
            }

            var user = appeal.Users;
            if (user == null) return;

            if (MessageBox.Show(
                    $"Принять апелляцию и разморозить аккаунт «{user.DisplayName}»?",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                user.IsFrozen  = false;
                appeal.Status  = "Approved";
                App.Db.SaveChanges();
                MessageBox.Show("Аккаунт разморожен. Апелляция принята.", "Готово");
                LoadData();
            }
        }

        private void BtnRejectAppeal_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgAppeals.SelectedItem is UnfreezeRequest appeal))
            {
                MessageBox.Show("Выберите апелляцию в таблице.", "Подсказка");
                return;
            }

            if (MessageBox.Show(
                    "Отклонить апелляцию? Аккаунт останется замороженным.",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                appeal.Status = "Rejected";
                App.Db.SaveChanges();
                MessageBox.Show("Апелляция отклонена.", "Готово");
                LoadData();
            }
        }

        // ==================== ЗАЯВКИ НА АВТОРА ====================

        private void BtnApproveRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgRoleRequests.SelectedItem is RoleRequest request))
            {
                MessageBox.Show("Выберите заявку в таблице.", "Подсказка");
                return;
            }

            // Ищем роль автора
            var authorRole = App.Db.Role.FirstOrDefault(r =>
                r.RoleName.ToLower() == "author" ||
                r.RoleName.ToLower() == "автор");

            // Если по имени не нашли — берём RoleID = 2
            if (authorRole == null)
                authorRole = App.Db.Role.FirstOrDefault(r => r.RoleID == 2);

            if (authorRole == null)
            {
                MessageBox.Show("Роль «Автор» не найдена в базе данных.", "Ошибка");
                return;
            }

            request.Status       = "Approved";
            request.Users.RoleID = authorRole.RoleID;
            App.Db.SaveChanges();

            MessageBox.Show(
                $"Заявка одобрена. Пользователь «{request.Users.DisplayName}» теперь Автор.",
                "Готово");
            LoadData();
        }

        private void BtnRejectRole_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgRoleRequests.SelectedItem is RoleRequest request))
            {
                MessageBox.Show("Выберите заявку в таблице.", "Подсказка");
                return;
            }

            request.Status = "Rejected";
            App.Db.SaveChanges();
            MessageBox.Show("Заявка отклонена.", "Готово");
            LoadData();
        }
    }
}
