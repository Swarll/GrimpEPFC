using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1920_A02
{
    public class Course : EntityBase<Model>
    {
        [Key]
        public int CourseId { get; set; }
        public int PeriodTime { get; set; }
        public DateTime? Beginning { get; set; }
        public string PeriodFormat { get => Beginning.Value.Hour.ToString() + "h - " + (PeriodTime + Beginning.Value.Hour).ToString() + "h"; set { } }
        public int MaxParticipants { get; set; }
        public String Location { get; set; }
        public String ProfessorPseudo { get; set; }
        // au format E.g : "15-25"
        public String RangeOfAge { get; set; }
        public String Name { get => ToString(); set{ }}
        //Chaque cours est dispensé par un seul prof
        public virtual Member Professor { get; set; }
        public virtual ICollection<Member> Participants { get; set; } = new HashSet<Member>();

        public bool AddParticipant(Member participant)
        {
            string[] str = RangeOfAge.Split('-');
            int minAge = int.Parse(str[0]), maxAge = int.Parse(str[1]);
            if (Participants.Count() < MaxParticipants && participant.Age >= minAge && participant.Age <= maxAge && participant.HasCoursePass() && (participant.Subscription.Cast<Subscription>().Any(s => s.PassType.Equals(PassType.Course)) ? participant.Subscription.Cast<Subscription>().First(s => s.PassType.Equals(PassType.Course)).IsValid() : false))
            {
                participant.Follows.Add(this);
                Participants.Add(participant);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveParticipant(Member participant)
        {
            if (Participants.Contains(participant))
            {
                Participants.Remove(participant);
                participant.Follows.Remove(this);
                return true;
            }
            else
            {
                return false;
            }
        }

        // Méthode pour changer le professeur dispensant le cours
        public bool UpdateProfessor(Member professor)
        {
            if (professor.Role == Role.Professor)
            {
                // Ajoute le nouveau prof
                Professor = professor;
                Professor.Pseudo = professor.Pseudo;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Delete()
        {
            // Supprime le cours de la liste des cours du prof et des participants
            foreach (Member member in Participants)
            {
                member.Follows.Remove(this);
            }
            // Supprime les participants du cours
            Participants.Clear();
        }

        public override string ToString()
        {
            return this.Beginning.Value.DayOfWeek.ToString() + " " + this.PeriodFormat + " Course";
        }

        public DateTime Ends()
        {
            DateTime temp = Beginning.Value;
            return temp.AddHours(PeriodTime);
        }
    }
}
