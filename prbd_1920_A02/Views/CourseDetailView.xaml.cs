using Microsoft.Win32;
using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
    /// Logique d'interaction pour CourseDetailView.xaml
    /// </summary>
    public partial class CourseDetailView : UserControlBase
    {
        public Member Member { get; set; }
        public Course Course { get; set; }
        private ImageHelper imageHelper;

        private ObservableCollection<Member> members;
        public ObservableCollection<Member> Members
        {
            get => members;
            set
            {
                SetProperty(ref members, value);
                RaisePropertyChanged(nameof(ParticipantsView));
            }
        }

        private CollectionView participantsView = null;
        public CollectionView ParticipantsView
        {
            get
            {
                participantsView = (CollectionView)CollectionViewSource.GetDefaultView(Members);
                if (participantsView != null && participantsView.SortDescriptions.Count == 0)
                    participantsView.SortDescriptions.Add(new SortDescription("Pseudo", ListSortDirection.Descending));
                return participantsView;
            }
        }

        public ICollection<Member> Professors
        {
            get => App.Model.Members.Cast<Member>().Where(m => m.Role == Role.Professor).ToHashSet();
            set
            {
                Professors = value;
                RaisePropertyChanged(nameof(Professors));
            }
        }

        public DayOfWeek Day
        {
            get { return Course.Beginning.Value.DayOfWeek; }
            set
            {
                DateTime? temp = Course.Beginning;
                int dayOfWeek = (int)temp.Value.DayOfWeek;
                temp = temp.Value.AddDays(-dayOfWeek);
                temp = temp.Value.AddDays((int)value);
                Course.Beginning = temp;
                RaisePropertyChanged(nameof(Day));
                RaisePropertyChanged(nameof(CourseName));
                App.NotifyColleagues(AppMessages.MSG_COURSE_NAME_CHANGED,
                                     Course.ToString());
            }
        }

        public ICollection<DayOfWeek> DaysOfWeek
        {
            get
            {
                HashSet<DayOfWeek> days = new HashSet<DayOfWeek>
                {
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday,
                    DayOfWeek.Saturday,
                    DayOfWeek.Sunday
                };
                return days;
            }
            set { }
        }

        public ICollection<int> Hours
        {
            get
            {
                int[] hours = new int[13];
                for (int i = 8; i < 21; i++)
                {
                    hours[i - 8] = i;
                }
                return hours;
            }
            set { }
        }

        public ICollection<int> PeriodHours
        {
            get
            {
                int[] hours = new int[3];
                for (int i = 0; i < 3; i++)
                {
                    hours[i] = i + 1;
                }
                return hours;
            }
            set { }
        }

        public int StartHour
        {
            get { return Course.Beginning.Value.Hour; }
            set
            {
                Course.Beginning = Course.Beginning.Value.AddHours(-Course.Beginning.Value.Hour);
                Course.Beginning = Course.Beginning.Value.AddHours(value);
                RaisePropertyChanged(nameof(StartHour));
                RaisePropertyChanged(nameof(CourseName));
                App.NotifyColleagues(AppMessages.MSG_COURSE_NAME_CHANGED,
                                     Course.ToString());
            }
        }

        public int PeriodTime
        {
            get { return Course.PeriodTime; }
            set
            {
                Course.PeriodTime = value;
                RaisePropertyChanged(nameof(PeriodTime));
                RaisePropertyChanged(nameof(CourseName));
                App.NotifyColleagues(AppMessages.MSG_COURSE_NAME_CHANGED,
                                     Course.ToString());
            }
        }

        public string CourseName
        {
            get { return Course.ToString(); }
            set
            {
                RaisePropertyChanged(nameof(CourseName));
            }
        }

        public int MaxParticipants
        {
            get { return Course.MaxParticipants; }
            set
            {
                Course.MaxParticipants = value;
                RaisePropertyChanged(nameof(MaxParticipants));
            }
        }

        public int ParticipantsCount
        {
            get { return Course.Participants.Count(); }
            set
            {
                ParticipantsCount = value;
                RaisePropertyChanged(nameof(ParticipantsCount));
            }
        }

        public bool HasEditRights
        {
            get { return Member.Role != Role.Climber; }
        }

        private Member tempProfessor;

        public Member Professor
        {
            get { return Course.Professor; }
            set
            {
                Course.Professor = value;
                RaisePropertyChanged(nameof(Professor));
                RaisePropertyChanged(nameof(PicturePath));
            }
        }

        public string RangeOfAge
        {
            get { return Course.RangeOfAge; }
            set
            {
                Course.RangeOfAge = value;
                RaisePropertyChanged(nameof(RangeOfAge));
            }
        }

        public string PicturePath
        {
            get { return Course.Professor.AbsolutePicturePath; }
            set
            {
                Course.Professor.PicturePath = value;
                RaisePropertyChanged(nameof(PicturePath));
            }
        }

        public string SubscriptionText
        {
            get
            {
                if (!Member.HasCoursePass())
                {
                    return Properties.Resources.CourseDetailView_Subscription_Message_1;
                }
                else
                {
                    if (Member.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Course)).IsValid())
                    {
                        return Properties.Resources.CourseDetailView_Subscription_Message_4;
                    }
                    else
                    {
                        if (Member.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Course)).ExpirationDate < DateTime.Now)
                        {
                            return Properties.Resources.CourseDetailView_Subscription_Message_2;
                        }
                        else
                        {
                            return Properties.Resources.CourseDetailView_Subscription_Message_3;
                        }
                    }
                };
            }
            set
            {
                SubscriptionText = value;
                RaisePropertyChanged(nameof(SubscriptionText));
            }
        }

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand Sub { get; set; }
        public ICommand UnSub { get; set; }

        public ICommand Delete { get; set; }

        private void SaveAction()
        {
            tempProfessor = Professor;
            imageHelper.Confirm(Professor.Pseudo);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_COURSE_CHANGED, Course);
        }

        private void CancelAction()
        {
            var find = (from c in App.Model.Courses where c.CourseId == Course.CourseId
                         select c).Any();
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
            }
            if (!Course.IsUnchanged && find)
            {
                Course.Reload();
                RaisePropertyChanged(nameof(Day));
                RaisePropertyChanged(nameof(PeriodTime));
                RaisePropertyChanged(nameof(StartHour));
                RaisePropertyChanged(nameof(MaxParticipants));
                RaisePropertyChanged(nameof(ParticipantsCount));
                RaisePropertyChanged(nameof(Professor));
                RaisePropertyChanged(nameof(RangeOfAge));
                RaisePropertyChanged(nameof(PicturePath));
                RaisePropertyChanged(nameof(SubscriptionText));
                RaisePropertyChanged(nameof(Professors));
                RaisePropertyChanged(nameof(Course));
            }
        }

        private bool CanSaveOrCancelAction()
        {
            if (Professor != null)
            {

                var change = (from c in App.Model.ChangeTracker.Entries<Course>()
                              where c.Entity == Course
                              select c).FirstOrDefault();
                return change != null && change.State != EntityState.Unchanged || !Professor.Equals(tempProfessor);
            }
            return false;
        }

        private bool CanSubAction()
        {
            string[] str = Course.RangeOfAge.Split('-');
            int minAge = int.Parse(str[0]), maxAge = int.Parse(str[1]);
            return Course != null && Professor != null && !Member.Pseudo.Equals(Professor.Pseudo) && !Course.Participants.Contains(Member) && Course.Participants.Count() < MaxParticipants && Member.Age >= minAge && Member.Age <= maxAge && (Member.Subscription != null && Member.HasCoursePass() && (Member.Subscription.Cast<Subscription>().Any(s => s.PassType.Equals(PassType.Course)) ? Member.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Course)).IsValid() : false));
        }

        private bool CanUnSubAction()
        {
            return Course.Participants.Contains(Member);
        }

        private void SubAction()
        {
            Course modelCourse = App.Model.Courses.Find(Course.CourseId);
            modelCourse.AddParticipant(Member);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_PARTICIPANTS_LIST_CHANGED);
        }

        private void UnSubAction()
        {
            Course modelCourse = App.Model.Courses.Find(Course.CourseId);
            modelCourse.RemoveParticipant(Member);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_PARTICIPANTS_LIST_CHANGED);
        }

        private void SetProfessorValue(Course course)
        {
            tempProfessor = course.Professor;
        }

        private void DeleteAction()
        {
            List<Course> temp2 = new List<Course>();
            temp2.Add(Course);
            App.Model.Courses.Remove(Course);
            Course.Delete();
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_COURSE_DELETED, temp2);
        }

        private bool CanDeleteAction() { return Member.Role == Role.Admin || Member.Role == Role.Professor; }

        public CourseDetailView(Course course)
        {
            InitializeComponent();

            SetProfessorValue(course);

            DataContext = this;
            Course = course;
            Member = App.CurrentUser;
            imageHelper = new ImageHelper(App.IMAGE_PATH, Course.Professor.PicturePath);
            Save = new RelayCommand(SaveAction, CanSaveOrCancelAction);
            Cancel = new RelayCommand(CancelAction, CanSaveOrCancelAction);
            Delete = new RelayCommand(DeleteAction, CanDeleteAction);
            Sub = new RelayCommand(SubAction, CanSubAction);
            UnSub = new RelayCommand(UnSubAction, CanUnSubAction);
            App.Register(this, AppMessages.MSG_PARTICIPANTS_LIST_CHANGED, () => Refresh());
            App.Register<Member>(this, AppMessages.MSG_MEMBER_DELETED, (m) =>
            {
                if (Members.Contains(m) || m.Role == Role.Professor)
                {
                    Refresh();
                };
            });
            Refresh();
        }

        private void Refresh()
        {
            if (App.Model.Courses.Cast<Course>().FirstOrDefault(c => c.CourseId == Course.CourseId) != null)
            {
                RaisePropertyChanged(nameof(ParticipantsCount));
                RaisePropertyChanged(nameof(Professors));
                Members = new ObservableCollection<Member>(App.Model.Courses.Find(Course.CourseId).Participants.OrderBy(m => m.Pseudo));
            }
        }

    }
}
