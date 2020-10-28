using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace prbd_1920_A02
{
    /// <summary>
    /// Logique d'interaction pour SignupView.xaml
    /// </summary>
    public partial class SignupView : WindowBase
    {
        private string pseudo;
        public string Pseudo
        {
            get => pseudo;
            set => SetProperty<string>(ref pseudo, value, () => Validate());
        }

        private string password;
        public string Password
        {
            get => password;
            set => SetProperty<string>(ref password, value, () => Validate());
        }

        private string password_confirm;

        public string PasswordConfirm
        {
            get => password_confirm;
            set => SetProperty<string>(ref password_confirm, value, () => Validate());
        }

        private string firstName;

        public string FirstName
        {
            get => firstName;
            set => SetProperty<string>(ref firstName, value, () => Validate());
        }

        private string lastName;

        public string LastName
        {
            get => lastName;
            set => SetProperty<string>(ref lastName, value, () => Validate());
        }

        private string email;

        public string Email
        {
            get => email;
            set => SetProperty<string>(ref email, value, () => Validate());
        }

        public override bool Validate()
        {
            ClearErrors();
            if (!ValidateLogin() || !ValidatePwd() || !ValidatePasswordConfirm() || !ValidateFirstName() || !ValidateLastName() || !ValidateEmail())
                RaiseErrors();
            return !HasErrors;
        }

        private bool ValidateLogin()
        {
            if (string.IsNullOrEmpty(Pseudo))
            {
                AddError("Pseudo", Properties.Resources.Error_Required);
            }
            else
            {
                if (Pseudo.Length < 3)
                {
                    AddError("Pseudo", Properties.Resources.Error_LengthGreaterEqual3);
                }
                else
                {
                    if ((from user in App.Model.Members where user.Pseudo == Pseudo select user).Count() > 0)
                    {
                        AddError("Pseudo", Properties.Resources.Error_PseudoTaken);
                    }
                }
            }
            return !HasErrors;
        }

        private bool ValidateFirstName()
        {
            if (string.IsNullOrEmpty(FirstName))
            {
                AddError("FirstName", Properties.Resources.Error_Required);
            }
            else
            {
                if (FirstName.Length < 3)
                {
                    AddError("FirstName", Properties.Resources.Error_LengthGreaterEqual3);
                }
            }
            return !HasErrors;
        }

        private bool ValidateLastName()
        {
            if (string.IsNullOrEmpty(LastName))
            {
                AddError("LastName", Properties.Resources.Error_Required);
            }
            else
            {
                if (LastName.Length < 3)
                {
                    AddError("LastName", Properties.Resources.Error_LengthGreaterEqual3);
                }
            }
            return !HasErrors;
        }

        private bool ValidateEmail()
        {
            var isValid = new EmailAddressAttribute().IsValid(Email);
            if (!isValid)
            {
                AddError("Email", Properties.Resources.Error_WrongMailFormat);
            }
            return !HasErrors;
        }

        private bool ValidatePwd()
        {
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            if (string.IsNullOrEmpty(Password))
            {
                AddError("Password", Properties.Resources.Error_Required);
            }
            else
            {
                if (Password.Length < 8)
                {
                    AddError("Password", Properties.Resources.Error_LengthGreaterEqual8);
                }
                else if (!hasNumber.IsMatch(Password) || !hasUpperChar.IsMatch(Password))
                {
                    AddError("Password", Properties.Resources.Error_Password_Format);
                }
            }
            return !HasErrors;
        }

        private bool ValidatePasswordConfirm()
        {
            if (string.IsNullOrEmpty(PasswordConfirm))
            {
                AddError("PasswordConfirm", Properties.Resources.Error_PasswordMisMatch);
            }
            else if(!PasswordConfirm.Equals(Password))
            {
                AddError("PasswordConfirm", Properties.Resources.Error_PasswordMisMatch);
            }
            return !HasErrors;
        }

        private void SignupAction()
        {
            if (Validate())
            { // si aucune erreurs
                App.Model.CreateMember(Pseudo, Password, FirstName, LastName, Email, DateTime.Now, Role.Climber, null);
                App.Model.SaveChanges();
                var member = (from user in App.Model.Members
                              where user.Pseudo == Pseudo
                              select user).FirstOrDefault();// on recherche le membre 
                App.CurrentUser = member; // le membre connecté devient le membre courant
                ShowMainView(); // ouverture de la fenêtre principale
                Close(); // fermeture de la fenêtre de signup
            }
        }

        private void LoginAction()
        {
            ShowLoginView();
            Close();
        }

        public ICommand Login { get; set; }
        public ICommand Signup { get; set; }
        public ICommand Cancel { get; set; }

        private static void ShowLoginView()
        {
            var loginView = new LoginView();
            loginView.Show();
            Application.Current.MainWindow = loginView;
        }

        private static void ShowMainView()
        {
            var mainView = new MainView();
            mainView.Show();
            Application.Current.MainWindow = mainView;
        }

        public SignupView()
        {
            InitializeComponent();

            DataContext = this;

            Signup = new RelayCommand(SignupAction,
                () => { return pseudo != null && password != null && password_confirm != null && firstName != null 
                    && lastName != null && email != null && !HasErrors; });
            Login = new RelayCommand(LoginAction);

            Cancel = new RelayCommand(() => Close());
        }
    }
}
