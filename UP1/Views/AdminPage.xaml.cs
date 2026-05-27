using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using UP1;

namespace UP1.Views
{
    public partial class AdminPage : Page
    {
        private List<Book> allAdminBooks;

        public AdminPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Пользователи
            dgUsers.ItemsSource = App.Db.Users
                .Include(u => u.Role)
                .OrderBy(u => u.UserID)
                .ToList();

            // Жалобы
            dgComplaints.ItemsSource = App.Db.Complain
                .Include(c => c.Book)
                .Include(c => c.Book.Users)
                .Include(c => c.Users)
                .OrderByDescending(c => c.Created)
                .ToList();

            // Апелляции
            dgAppeals.ItemsSource = App.Db.UnfreezeRequest
                .Include(r => r.Users)
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.Created)
                .ToList();

            // Заявки на автора
            dgRoleRequests.ItemsSource = App.Db.RoleRequest
                .Include(r => r.Users)
                .Where(r => r.Status == "Pending")
                .ToList();

            // Все книги для вкладки администратора
            allAdminBooks = App.Db.Book
                .Include(b => b.Users)
                .OrderBy(b => b.Title)
                .ToList();

            ApplyBookFilter();
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
                Title = $"Смена роли — {user.DisplayName}",
                Width = 320,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Background = new System.Windows.Media.SolidColorBrush(
                                  System.Windows.Media.Color.FromRgb(45, 45, 45))
            };

            var panel = new StackPanel { Margin = new Thickness(20) };

            panel.Children.Add(new TextBlock
            {
                Text = $"Пользователь: {user.DisplayName}\nТекущая роль: {user.Role?.RoleName ?? "—"}",
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 14)
            });

            panel.Children.Add(new TextBlock
            {
                Text = "Новая роль:",
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 4)
            });

            var combo = new ComboBox
            {
                Height = 34,
                ItemsSource = roles,
                DisplayMemberPath = "RoleName",
                SelectedValuePath = "RoleID",
                SelectedValue = user.RoleID,
                Background = new System.Windows.Media.SolidColorBrush(
                                        System.Windows.Media.Color.FromRgb(60, 60, 60)),
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 16)
            };

            var btnSave = new Button
            {
                Content = "Сохранить",
                Height = 38,
                Background = new System.Windows.Media.SolidColorBrush(
                                 System.Windows.Media.Color.FromRgb(76, 175, 80)),
                Foreground = System.Windows.Media.Brushes.White
            };

            btnSave.Click += (s, ev) =>
            {
                if (combo.SelectedItem is Role selected)
                {
                    user.RoleID = selected.RoleID;
                    user.Role = selected;
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

        private void BtnRejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgComplaints.SelectedItem is Complain complaint))
            {
                MessageBox.Show("Выберите жалобу в таблице.", "Подсказка");
                return;
            }

            if (MessageBox.Show(
                    $"Отказать в заморозке по жалобе на книгу «{complaint.Book?.Title ?? "—"}»?\nЖалоба будет удалена.",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                App.Db.Complain.Remove(complaint);
                App.Db.SaveChanges();
                MessageBox.Show("Жалоба отклонена и удалена.", "Готово");
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
                user.IsFrozen = false;
                appeal.Status = "Approved";
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

            request.Status = "Approved";
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

        // ==================== КНИГИ ====================

        private void ApplyBookFilter()
        {
            if (allAdminBooks == null) return;

            var search = txtBookSearch?.Text?.ToLower().Trim() ?? "";
            var filtered = allAdminBooks.AsEnumerable();

            // Фильтр по тексту
            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(b =>
                    (b.Title?.ToLower().Contains(search) ?? false) ||
                    (b.Users?.DisplayName?.ToLower().Contains(search) ?? false));

            // Фильтр по статусу заморозки
            switch (cmbBookFilter?.SelectedIndex)
            {
                case 1: filtered = filtered.Where(b => !b.IsFrozen); break; // Активные
                case 2: filtered = filtered.Where(b => b.IsFrozen); break; // Замороженные
            }

            dgAdminBooks.ItemsSource = filtered.ToList();
        }

        private void TxtBookSearch_TextChanged(object sender, TextChangedEventArgs e)
            => ApplyBookFilter();

        private void CmbBookFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => ApplyBookFilter();

        private void BtnAdminFreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgAdminBooks.SelectedItem is Book book))
            {
                MessageBox.Show("Выберите книгу в таблице.", "Подсказка");
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

        private void BtnAdminUnfreezeBook_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgAdminBooks.SelectedItem is Book book))
            {
                MessageBox.Show("Выберите книгу в таблице.", "Подсказка");
                return;
            }

            if (!book.IsFrozen)
            {
                MessageBox.Show($"Книга «{book.Title}» уже активна.", "Информация");
                return;
            }

            if (MessageBox.Show(
                    $"Разморозить книгу «{book.Title}»?\nОна снова появится в каталоге.",
                    "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                book.IsFrozen = false;
                App.Db.SaveChanges();
                MessageBox.Show("Книга разморожена и доступна в каталоге.", "Готово");
                LoadData();
            }
        }
    }
}
