using Microsoft.Win32;
using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace prbd_1920_A02
{
    public partial class MemberDetailView : UserControlBase
    {

        public Member Member { get; set; }
        private ImageHelper imageHelper;

        public string Pseudo
        {
            get { return Member.Pseudo; }
            set
            {
                Member.Pseudo = value;
                RaisePropertyChanged(nameof(Pseudo));
                App.NotifyColleagues(AppMessages.MSG_PSEUDO_CHANGED,
                                     string.IsNullOrEmpty(value) ? "<new member>" : value);
            }
        }

        public string PicturePath
        {
            get { return Member.AbsolutePicturePath; }
            set
            {
                Member.PicturePath = value;
                RaisePropertyChanged(nameof(PicturePath));
            }
        }

        public string LastName
        {
            get { return Member.LastName; }
            set
            {
                Member.LastName = value;
                RaisePropertyChanged(nameof(LastName));
            }
        }

        public string FirstName
        {
            get { return Member.FirstName; }
            set
            {
                Member.FirstName = value;
                RaisePropertyChanged(nameof(FirstName));
            }
        }

        public DateTime? BirthDate
        {
            get { return Member.BirthDate; }
            set
            {
                Member.BirthDate = value;
                RaisePropertyChanged(nameof(BirthDate));
            }
        }

        public int Age
        {
            get { return Member.Age; }
            set
            {
                Member.Age = value;
                RaisePropertyChanged(nameof(Age));
            }
        }

        public bool HasEditRights
        {
            get { return Member.Equals(App.CurrentUser) || App.CurrentUser.Role == Role.Admin; }
        }

        public bool HasNotEditRights
        {
            get { return !Member.Equals(App.CurrentUser) && !(App.CurrentUser.Role == Role.Admin); }
        }

        public ICommand Save { get; set; }
        public ICommand Cancel { get; set; }
        public ICommand ClearImage { get; set; }
        public ICommand Delete { get; set; }
        public ICommand LoadImage { get; set; }

        private void SaveAction()
        {
            imageHelper.Confirm(Member.Pseudo);
            PicturePath = imageHelper.CurrentFile;
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_MEMBER_CHANGED, Member);
        }

        private void CancelAction()
        {
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
            }
            if (!Member.IsUnchanged)
            {
                Member.Reload();
                RaisePropertyChanged(nameof(Pseudo));
                RaisePropertyChanged(nameof(FirstName));
                RaisePropertyChanged(nameof(LastName));
                RaisePropertyChanged(nameof(BirthDate));
                RaisePropertyChanged(nameof(Age));
                RaisePropertyChanged(nameof(PicturePath));
            }
        }

        private void ClearImageAction()
        {
            imageHelper.Clear();
            PicturePath = imageHelper.CurrentFile;
        }

        private bool CanSaveOrCancelAction()
        {
            var change = (from c in App.Model.ChangeTracker.Entries<Member>()
                          where c.Entity == Member
                          select c).FirstOrDefault();
            return change != null && change.State != EntityState.Unchanged;
        }

        private void DeleteAction()
        {
            Member temp = Member;
            List<Course> temp2 = new List<Course>();
            if (temp.Role.Equals(Role.Professor))
            {
                foreach (Course course in App.Model.Courses)
                {
                    if (course.Professor.Equals(temp))
                    {
                        temp2.Add(course);
                    }
                }
            }
            Member.Delete();
            App.Model.Members.Remove(Member);
            App.Model.SaveChanges();
            App.NotifyColleagues(AppMessages.MSG_MEMBER_DELETED, temp);
            App.NotifyColleagues(AppMessages.MSG_COURSE_DELETED, temp2);
        }

        private void LoadImageAction()
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() == true)
            {
                var filename = fd.FileName;
                if (filename != null && File.Exists(filename))
                {
                    imageHelper.Load(fd.FileName);
                    PicturePath = imageHelper.CurrentFile;
                }
            }
        }
        public MemberDetailView(Member member)
        {
            InitializeComponent();

            DataContext = this;
            Member = member;
            imageHelper = new ImageHelper(App.IMAGE_PATH, Member.PicturePath);
            Save = new RelayCommand(SaveAction, CanSaveOrCancelAction);
            Cancel = new RelayCommand(CancelAction, CanSaveOrCancelAction);
            ClearImage = new RelayCommand(ClearImageAction);
            Delete = new RelayCommand(DeleteAction);
            LoadImage = new RelayCommand(LoadImageAction);
        }

        public override void Dispose()
        {
            base.Dispose();
            if (imageHelper.IsTransitoryState)
            {
                imageHelper.Cancel();
                PicturePath = imageHelper.CurrentFile;
            }
        }
    }
}

