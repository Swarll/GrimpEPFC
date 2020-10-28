//using prbd_1920_A02.Views;
using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace prbd_1920_A02 {
    public partial class MainView : WindowBase {

        public ICommand Logout { get; set; }

        public ICommand Profiles { get; set; }

        public ICommand Courses { get; set; }
        public ICommand Competitions { get; set; }
        public ICommand Subscriptions { get; set; }

        public MainView() {
            InitializeComponent();
            
            DataContext = this;

            App.Register<Member>(this, AppMessages.MSG_DISPLAY_MEMBER, m => {
                TabOfMember(m, false);
            });

            App.Register(this, AppMessages.MSG_DISPLAY_MEMBERS, () => Members());

            App.Register<string>(this, AppMessages.MSG_PSEUDO_CHANGED, (s) => {
                (tabControl.SelectedItem as TabItem).Header = s;
            });

            App.Register<Member>(this, AppMessages.MSG_MEMBER_DELETED, m => {
                tabControl.Items.Remove(tabControl.SelectedItem);
            });

            App.Register(this, AppMessages.MSG_DISPLAY_COURSES, () => TabCourses());

            App.Register<Course>(this, AppMessages.MSG_DISPLAY_COURSE, c => {
                TabOfCourse(c, false);
            });
            App.Register<PassType>(this, AppMessages.MSG_DISPLAY_BUY_SUBSCRIPTION, c => {
                TabOfBuySubscription(c);
            });

            App.Register<ICollection<Course>>(this, AppMessages.MSG_COURSE_DELETED, c => {
                IEnumerable<TabItem> items = tabControl.Items.Cast<TabItem>().Where(x => c.Cast<Course>().Any(co => co.Name.Equals(x.Header.ToString())));
                foreach (TabItem item in items.ToList())
                {
                    tabControl.Items.Remove(item);
                }
            });

            App.Register<string>(this, AppMessages.MSG_COURSE_NAME_CHANGED, (s) => {
                (tabControl.SelectedItem as TabItem).Header = s;
            });

            App.Register(this, AppMessages.MSG_DISPLAY_COMPETITIONS, () => TabCompetitions());

            App.Register<Competition>(this, AppMessages.MSG_DISPLAY_COMPETITION, c => {
                TabOfCompetition(c, false);
            });
            App.Register<Competition>(this, AppMessages.MSG_DISPLAY_NEW_COMPETITION, c => {
                TabOfCompetition(c, true);
            });

            App.Register<Course>(this, AppMessages.MSG_DISPLAY_NEW_COURSE, c => {
                TabOfCourse(c, true);
            });

            App.Register(this, AppMessages.MSG_COMPETITION_DELETED, () => {
                tabControl.Items.Remove(tabControl.SelectedItem);
            });

            App.Register(this, AppMessages.MSG_CLOSE_TAB, () => {
                tabControl.Items.Remove(tabControl.SelectedItem);
            });

            App.Register<string>(this, AppMessages.MSG_COMPETITION_NAME_CHANGED, (s) => {
                (tabControl.SelectedItem as TabItem).Header = s;
            });

            App.Register(this, AppMessages.MSG_DISPLAY_SUBSCRIPTIONS, () => TabSubscriptions());


            Logout = new RelayCommand(LogoutAction);

            Profiles = new RelayCommand(() => { App.NotifyColleagues(AppMessages.MSG_DISPLAY_MEMBERS); });

            Courses = new RelayCommand(() => { App.NotifyColleagues(AppMessages.MSG_DISPLAY_COURSES); });

            Competitions = new RelayCommand(() => { App.NotifyColleagues(AppMessages.MSG_DISPLAY_COMPETITIONS); });

            Subscriptions = new RelayCommand(() => { App.NotifyColleagues(AppMessages.MSG_DISPLAY_SUBSCRIPTIONS); });
        }

        public string User
        {
            get { return "Connected as : " + App.CurrentUser.Pseudo; }
            set
            {
            }
        }

        private void TabOfMember(Member m, bool isNew) {
            foreach (TabItem t in tabControl.Items) {
                if (t.Header.ToString().Equals(m.Pseudo)) {
                    Dispatcher.InvokeAsync(() => t.Focus());
                    return;
                }
            }
            var tab = new TabItem() {
                Header = isNew ? "<new member>" : m.Pseudo,
                Content = new MemberDetailView(m)
            };
            tabControl.Items.Add(tab);
            CloseTab(tab);
            // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        private void TabOfCourse(Course c, bool isNew)
        {
            if (c != null)
            {
                foreach (TabItem t in tabControl.Items)
                {
                    if (t.Header.ToString().Equals(c.ToString()))
                    {
                        Dispatcher.InvokeAsync(() => t.Focus());
                        return;
                    }
                }
                var tab = new TabItem()
                {
                    Header = isNew ? "<new course>" : c.ToString(),
                    Content = new CourseDetailView(c)
                };
                tabControl.Items.Add(tab);
                CloseTab(tab);
                // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
                Dispatcher.InvokeAsync(() => tab.Focus());
            }
        }

        private void Login() {
            var loginView = new LoginView();
            Visibility = Visibility.Hidden;
            var res = loginView.ShowDialog();
            if (res == true) {
                Visibility = Visibility.Visible;
            }
            else {
                Close();
            }
        }

        private void TabCourses()
        {
            var str = Properties.Resources.Menu_Courses;
            foreach (TabItem t in tabControl.Items)
            {
                if (t.Header.ToString().Equals(str))
                {
                    Dispatcher.InvokeAsync(() => t.Focus());
                    return;
                }
            }
            var tab = new TabItem()
            {
                Header = str,
                Content = new CoursesView()
            };
            tabControl.Items.Add(tab);
            CloseTab(tab);
            // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        private void Members()
        {
            var str = Properties.Resources.Menu_Profiles;
            foreach (TabItem t in tabControl.Items)
            {
                if (t.Header.ToString().Equals(str))
                {
                    Dispatcher.InvokeAsync(() => t.Focus());
                    return;
                }
            }
            var tab = new TabItem()
            {
                Header = str,
                Content = new MembersView()
            };
            tabControl.Items.Add(tab);
            CloseTab(tab);
            // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        private void TabCompetitions()
        {
            var str = Properties.Resources.Menu_Competitions;
            foreach (TabItem t in tabControl.Items)
            {
                if (t.Header.ToString().Equals(str))
                {
                    Dispatcher.InvokeAsync(() => t.Focus());
                    return;
                }
            }
            var tab = new TabItem()
            {
                Header = str,
                Content = new CompetitionsView()
            };
            tabControl.Items.Add(tab);
            CloseTab(tab);
            // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        private void TabOfCompetition(Competition c, bool isNew)
        {
                if (!isNew && c != null)
                {
                    var tab = new TabItem();
                    foreach (TabItem t in tabControl.Items)
                    {
                        if (t.Header.ToString().Equals(c.ToString()))
                        {
                            Dispatcher.InvokeAsync(() => t.Focus());
                            return;
                        }
                    }
                    tab = new TabItem()
                    {
                        Header = c.ToString(),
                        Content = new CompetitionDetailView(c)
                    };
                    tabControl.Items.Add(tab);
                    CloseTab(tab);
                    // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
                    Dispatcher.InvokeAsync(() => tab.Focus());
            }
                else if (App.CurrentUser.Pseudo.CompareTo("admin") == 0)
                {
                    var tab = new TabItem();
                    tab = new TabItem()
                    {
                        Header = "<new competition>",
                        Content = new CompetitionCreateView()
                    };
                    tabControl.Items.Add(tab);
                    CloseTab(tab);
                    // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
                Dispatcher.InvokeAsync(() => tab.Focus());
            }
        }

        private void TabSubscriptions()
        {
            var str = Properties.Resources.Menu_Subscriptions;
            foreach (TabItem t in tabControl.Items)
            {
                if (t.Header.ToString().Equals(str))
                {
                    Dispatcher.InvokeAsync(() => t.Focus());
                    return;
                }
            }
            var tab = new TabItem()
            {
                Header = str,
                Content = new SubscriptionsView()
            };
            tabControl.Items.Add(tab);
            CloseTab(tab);
            // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        private void TabOfBuySubscription(PassType selectedPass) 
        {
            var tab = new TabItem();
            foreach (TabItem t in tabControl.Items)
            {
                if (t.Header.ToString().Equals(selectedPass.ToString()))
                {
                    Dispatcher.InvokeAsync(() => t.Focus());
                    return;
                }
            }
            tab = new TabItem()
            {
                Header = Properties.Resources.Menu_Buy_Subscription + " " + selectedPass.ToString(),
                Content = new BuySubscriptionView(selectedPass)
            };
            tabControl.Items.Add(tab);
            CloseTab(tab);
            // exécute la méthode Focus() de l'onglet pour lui donner le focus (càd l'activer)
            Dispatcher.InvokeAsync(() => tab.Focus());
        }

        private void CloseTab(TabItem tab)
        {
            // Pour fermer un tab ctrl + w ou clic de la molette de la souris
            tab.MouseDown += (o, e) => {
                if (e.ChangedButton == MouseButton.Middle &&
                    e.ButtonState == MouseButtonState.Pressed)
                {
                    tabControl.Items.Remove(o);
                    (tab.Content as UserControlBase).Dispose();
                }
            };
            tab.PreviewKeyDown += (o, e) => {
                if (e.Key == Key.W && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    tabControl.Items.Remove(o);
                    (tab.Content as UserControlBase).Dispose();
                }
            };
        }

        private void LogoutAction() {
            App.CurrentUser = null;
            for (int i = tabControl.Items.Count - 1; i > 0; i--) 
                tabControl.Items.RemoveAt(i);
            Login();
        }
    }
}
