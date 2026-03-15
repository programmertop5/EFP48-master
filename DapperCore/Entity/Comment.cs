namespace EFP48.DapperCore.Entity
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid PostId { get; set; }
        public Post Post { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public List<Like> Likes { get; set; }

        public override string ToString()
        {
            return $@"
--------------------------------
CommentID: {Id}
Content -> {Content}
PostId -> {PostId}
UserId -> {UserId}
CreatedAt -> {CreatedAt}
--------------------------------
";
        }
    }
}