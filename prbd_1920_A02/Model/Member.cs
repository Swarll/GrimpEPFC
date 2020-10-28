using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PRBD_Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Core.Common.EntitySql;

namespace prbd_1920_A02
{

    public class Member : EntityBase<Model>
    {
        [Key]
        public int MemberId { get; set; }
        public string Pseudo { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime? BirthDate { get; set; }
        public int Age { get; set; }
        public string PicturePath { get; set; }
        [NotMapped]
        public string AbsolutePicturePath
        {
            get { return PicturePath != null ? App.IMAGE_PATH + "\\" + PicturePath : null; }
        }
        public Role Role { get; set; }
        public virtual ICollection<Subscription> Subscription { get; set; } = new HashSet<Subscription>();
        public virtual ICollection<Post> AuthorPosts { get; set; } = new HashSet<Post>();
        public virtual ICollection<Course> Follows { get; set; } = new HashSet<Course>();
        public virtual ICollection<Competition> Competitions { get; set; } = new HashSet<Competition>();
        public virtual ICollection<CompetitionResult> CompetitionsWon { get; set; } = new HashSet<CompetitionResult>();

        protected Member() { }

        public bool HasCoursePass()
        {
            return this.Subscription.Cast<Subscription>().Any(s => s.PassType.Equals(PassType.Course));
        }

        public bool HasCompetitionPass()
        {
            return this.Subscription.Cast<Subscription>().Any(s => s.PassType.Equals(PassType.Competition));
        }

        public bool HasNormalPass()
        {
            return this.Subscription.Cast<Subscription>().Any(s => s.PassType.Equals(PassType.Pass));
        }

        public override string ToString()
        {
            return FirstName;
        }

        public ForumThread CreateThread(string title, string body, bool isParent = true)
        {
            return Model.CreateThread(this, title, body, isParent);
        }

        public Post Answer(ForumThread thread, string body, bool isParent = false)
        {
            return Model.CreatePost(this, thread, body, isParent);
        }

        public Course CreateCourse(int periodTime, DateTime? beginning, int maxParticipants, string location, string range)
        {
            return Model.CreateCourse(this, periodTime, beginning, maxParticipants, location, range);
        }

        public Competition CreateCompetition(DateTime? beginning, int maxParticipants, string location, CompetitionType type, string range)
        {
            return Model.CreateCompetition(beginning, maxParticipants, location, type, range);
        }

        public bool SubToCourse(Course course)
        {
            return course.AddParticipant(this);
        }

        public bool UnsubFromCourse(Course course)
        {
            return course.RemoveParticipant(this);
        }

        public bool SubToCompetition(Competition competition)
        {
            return competition.AddParticipant(this);
        }

        public bool UnsubFromCompetition(Competition competition)
        {
            return competition.RemoveParticipant(this);
        }

        public void CancelSubscription(Subscription sub)
        {
            Subscription temp = sub;
            Subscription.Remove(sub);
            Model.Subscriptions.Remove(temp);
        }

        public void Delete()
        {
            ICollection<Post> temp = AuthorPosts;
            List<Post> temp2 = new List<Post>();
            // Supprime les posts du membre
            foreach (Post post in AuthorPosts)
            {
                if (post.IsParent)
                {
                    temp2.AddRange(post.Thread.Posts);
                    post.Delete();
                }
            }
            Model.Posts.RemoveRange(temp);
            Model.Posts.RemoveRange(temp2);
            AuthorPosts.Clear();
            if (Role == Role.Professor)
            {
                List<Course> temp3 = new List<Course>();
                foreach (Course course in Model.Courses)
                { // Supprime le professeur du cours
                    if (course.Professor.Equals(this))
                    {
                        temp3.Add(course);
                        course.Delete();
                    }
                }
                Model.Courses.RemoveRange(temp3);
            }
            Follows.Clear();
            Model.Subscriptions.RemoveRange(this.Subscription);
            foreach (Competition compet in Model.Competitions)
            { // Supprime le membre dans les competitions 
                if (compet.Participants.Contains(this))
                    compet.Participants.Remove(this);
            }
            // Supprime le membre lui-même
            Model.Members.Remove(this);
        }

        public static Member GetMemberByPseudo(String pseudo)
        {
            var query = (from m in App.Model.Members
                         where m.Pseudo == pseudo
                         select m).Distinct();
            query.FirstOrDefault();
            Member memb = new Member();
            foreach (var q in query)
            {
                memb.MemberId = q.MemberId;
                memb.Pseudo = q.Pseudo;
                memb.Password = q.Password;
                memb.FirstName = q.FirstName;
                memb.Email = q.Email;
                memb.BirthDate = q.BirthDate;
                memb.Age = q.Age;
                memb.PicturePath = q.PicturePath;
                memb.Role = q.Role;
                memb.Subscription = q.Subscription;
                memb.AuthorPosts = q.AuthorPosts;
                memb.Follows = q.Follows;
                memb.Competitions = q.Competitions;
                memb.CompetitionsWon = q.CompetitionsWon;
            }
            return memb;
        }
    }
}