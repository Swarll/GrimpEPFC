using PRBD_Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace prbd_1920_A02
{
    public class Post : EntityBase<Model>
    {
        [Key]
        public int PostId { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Body { get; set; }
        public bool IsParent { get; set; }
        public virtual Member Author { get; set; }
        public virtual ForumThread Thread { get; set; }

        public void Delete()
        {
            if (!IsParent)
            {
                Post temp = this;
                Author.AuthorPosts.Remove(this);
                // Supprime le post du thread
                Thread.Posts.Remove(temp);
                // Supprime le post lui-même
                Model.Posts.Remove(temp);
            }
            else
            {
                ForumThread temp = Thread;
                // Supprime les posts de la liste des posts des membres
                foreach (Post post in Thread.Posts)
                {
                    if(!post.Author.Equals(Author))
                        post.Author.AuthorPosts.Remove(post);
                }
                // Supprime le post lui-même et les posts liés au même thread
                Thread.Posts.Clear();
                Model.Threads.Remove(temp);
            }
        }

    }
}
