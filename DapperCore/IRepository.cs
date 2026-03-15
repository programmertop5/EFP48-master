using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EFP48.DapperCore.Entity;
using System.Data;

namespace EFP48.DapperCore
{
    public interface IRepository<T> where T : class
    {
        void Create(T entity);
        void Update(T entity);
        void Delete(Guid id);
        List<T> GetAll();
    }

    public class UserRepository : IRepository<User>
    {
        private readonly IDbConnection _connection;

        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Create(User entity)
        {
            string sql = @"INSERT INTO Users (Id, Name, Surname, Age) VALUES (@Id, @Name, @Surname, @Age)";
            _connection.Execute(sql, entity);
        }

        public void Update(User entity)
        {
            string sql = @"UPDATE Users SET Name=@Name, Surname=@Surname, Age=@Age WHERE Id=@Id";
            _connection.Execute(sql, entity);
        }

        public void Delete(Guid id)
        {
            string sql = @"DELETE FROM Users WHERE Id=@Id";
            _connection.Execute(sql, new { Id = id });
        }

        public List<User> GetAll()
        {
            string sql = @"SELECT * FROM Users";
            return _connection.
                Query<User>(sql)
                .ToList();
        }
    }



    public class PostRepository : IRepository<Post>
    {
        private readonly IDbConnection _connection;

        public PostRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public void Create(Post entity)
        {
            string sql = @"INSERT INTO Posts (UserId, Title, CreatedAt, DeletedAt) 
                           VALUES (@UserId, @Title, @CreatedAt, @DeletedAt)";
            _connection.Execute(sql, entity);
        }

        public void Update(Post entity)
        {
            string sql = @"UPDATE Posts SET Title=@Title, DeletedAt=@DeletedAt WHERE Id=@Id";
            _connection.Execute(sql, entity);
        }

        public void Delete(Guid id)
        {
            string sql = @"DELETE FROM Posts WHERE Id=@Id";
            _connection.Execute(sql, new { Id = id });
        }

        public List<Post> GetAll()
        {
            string sql = @"SELECT p.Id, p.UserId, p.Title, p.CreatedAt, p.DeletedAt,
                                  u.Id, u.Name, u.Surname, u.Age
                           FROM Posts p
                           JOIN Users u ON p.UserId = u.Id";

            var posts = _connection.Query<Post, User, Post>(
                sql,
                (post, user) =>
                {
                    post.User = user;
                    return post;
                },
                splitOn: "Id"
            ).ToList();

            return posts;
        }
    }
}