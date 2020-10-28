using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prbd_1920_A02
{
    public class ForumThread : EntityBase<Model>
    {
        [Key]
        public int ThreadId { get; set; }
        public string Title { get; set; }
        public int ParentId { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();

    }
}
