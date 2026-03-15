using Dapper;
using EFP48.DapperCore.Entity;
using EFP48.DapperCore;
using Microsoft.Data.SqlClient;
using System.Data;

public class Program
{
    public static void Main(string[] args)
    {
        string connectionString = @"
Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=efp48;Integrated Security=True;Encrypt=False
";
        using IDbConnection connection = new SqlConnection(connectionString);

        string query = @"
DROP TABLE IF EXISTS CommentLikes;
DROP TABLE IF EXISTS PostLikes;
DROP TABLE IF EXISTS Comments;
DROP TABLE IF EXISTS Posts;
DROP TABLE IF EXISTS Users;

CREATE TABLE Users
(
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Surname NVARCHAR(255) NOT NULL,
    Age INT NOT NULL
);
CREATE TABLE Posts
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Title NVARCHAR(200),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    DeletedAt DATETIME DEFAULT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
CREATE TABLE Comments
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Content NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    PostId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
CREATE TABLE PostLikes
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PostId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (PostId) REFERENCES Posts(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    UNIQUE (PostId, UserId)
);
CREATE TABLE CommentLikes
(
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CommentId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    FOREIGN KEY (CommentId) REFERENCES Comments(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    UNIQUE (CommentId, UserId)
);
";
        connection.Execute(query);

        /*1 завдання*/
        var userRepo = new UserRepository(connection);
        var postRepo = new PostRepository(connection);

        userRepo.Create(new User { Id = Guid.Parse("53650437-bf01-459f-88ac-4da60f0e00ff"), Name = "Tom", Surname = "Due", Age = 25 });
        userRepo.Create(new User { Id = Guid.Parse("ccfca5d4-9084-4edf-9e1f-17bb36f23b81"), Name = "Alex", Surname = "Jackson", Age = 25 });
        userRepo.Create(new User { Id = Guid.NewGuid(), Name = "Bill", Surname = "White", Age = 25 });

        postRepo.Create(new Post { UserId = Guid.Parse("53650437-bf01-459f-88ac-4da60f0e00ff"), Title = "Oh this a great day" });
        postRepo.Create(new Post { UserId = Guid.Parse("53650437-bf01-459f-88ac-4da60f0e00ff"), Title = "Another post by Tom" });
        postRepo.Create(new Post { UserId = Guid.Parse("ccfca5d4-9084-4edf-9e1f-17bb36f23b81"), Title = "Hello world" });

        Console.WriteLine("-------- Користувачі ----");
        var users = userRepo.GetAll();
        foreach (var user in users)
        {
            Console.WriteLine(user);
        }

        Console.WriteLine("\n--- Пости ---");
        var posts = postRepo.GetAll();
        foreach (var post in posts)
        {
            Console.WriteLine($"{post.Title} | Автор: {post.User?.Name} {post.User?.Surname}");
        }

        /*2 завдання*/
        var postIds = connection.Query<Guid>("SELECT Id FROM Posts").ToList();
        var comments = new[]
        {
            new { Content = "Great post!", PostId = postIds[0], UserId = Guid.Parse("ccfca5d4-9084-4edf-9e1f-17bb36f23b81") },
            new { Content = "Nice!", PostId = postIds[0], UserId = Guid.Parse("53650437-bf01-459f-88ac-4da60f0e00ff") },
            new { Content = "Hello!", PostId = postIds[1], UserId = Guid.Parse("53650437-bf01-459f-88ac-4da60f0e00ff") },
            new { Content = "Cool!", PostId = postIds[2], UserId = Guid.Parse("53650437-bf01-459f-88ac-4da60f0e00ff") },
        }.ToList();
        connection.Execute(@"INSERT INTO Comments(Content, PostId, UserId) VALUES(@Content, @PostId, @UserId)", comments);

        var commentIds = connection.Query<Guid>("SELECT Id FROM Comments").ToList();
        var commentLikes = new[]
        {
            new { CommentId = commentIds[0], UserId = Guid.Parse("53650437-bf01-459f-88ac-4da60f0e00ff") },
            new { CommentId = commentIds[3], UserId = Guid.Parse("ccfca5d4-9084-4edf-9e1f-17bb36f23b81") },
            new { CommentId = commentIds[1], UserId = Guid.Parse("ccfca5d4-9084-4edf-9e1f-17bb36f23b81") },
            new { CommentId = commentIds[2], UserId = Guid.Parse("ccfca5d4-9084-4edf-9e1f-17bb36f23b81") },
        }.ToList();
        connection.Execute(@"INSERT INTO CommentLikes(CommentId, UserId) VALUES(@CommentId, @UserId)", commentLikes);

        Console.WriteLine("\n-----Коментарі по постах ----");
        query = @"
SELECT p.Title, COUNT(c.Id) as CommentCount
FROM Posts p
LEFT JOIN Comments c ON p.Id = c.PostId
GROUP BY p.Id, p.Title
";
        var commentCount_result = connection.Query<PostCommentCount>(query).ToList();
        foreach (var item in commentCount_result)
        {
            Console.WriteLine($"{item.Title} -> comments: {item.CommentCount}");
        }

        Console.WriteLine("\n-----Топ коментарі по лайках---------");
        query = @"
SELECT p.Title, c.Content, COUNT(cl.Id) as LikeCount
FROM Posts p
LEFT JOIN Comments c ON p.Id = c.PostId
LEFT JOIN CommentLikes cl ON c.Id = cl.CommentId
GROUP BY p.Id, p.Title, c.Id, c.Content
ORDER BY p.Title, LikeCount DESC
";
        var result = connection.Query<PostTopComment>(query).ToList();
        foreach (var item in result)
        {
            Console.WriteLine($"Пост: {item.Title}, Коментар: {item.Content},Лайків: {item.LikeCount}");
        }
    }
}

class PostCommentCount
{
    public string Title { get; set; }
    public int CommentCount { get; set; }
}

class UserPostCount
{
    public string FullName { get; set; }
    public int PostCount { get; set; }
}

class UserCard
{
    public string Name { get; set; }
    public string Surname { get; set; }
}

class PostTopComment
{
    public string Title { get; set; }
    public string Content { get; set; }
    public int LikeCount { get; set; }
}