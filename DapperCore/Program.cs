using Dapper;
using Microsoft.Data.Sqlite;
using DapperCore.Entities;
using System.Reflection;

namespace DapperCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            // connect to db
            string connectionString = "Data Source=library.db";

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            //AgregationFuncs
            //int bookCount = connection.ExecuteScalar<int>("select count(*) from Books;");

            //multiquery
            //var multiquery = connection.QueryMultiple(@"
            //            select * from Books;
            //            select count(*) from Books;");

            //var allbooks = multiquery.Read<Book>().ToList();
            //var count = multiquery.ReadSingle<int>();
            //Console.WriteLine(allbooks[0]);
            //Console.WriteLine(count);

            int choice1 = 1;
            while (choice1 != 0)
            {
                Console.WriteLine("1 - Add Book\n2 - Read all books\n3 - Search by...\n4 - Delete by...\n5 - Update book\n0 - Quit");
                choice1 = int.Parse(Console.ReadLine());
                switch (choice1)
                {
                    case 1:
                        AddBook(connection, new Book { Author = Console.ReadLine(), Title = Console.ReadLine() });
                        break;
                    case 2:
                        ReadBooks(connection);
                        break;
                    case 3:
                        Console.WriteLine("Search by:\n\t1 - Author\n\t2 - Title\n\t3 - Id");
                        int choice2 = int.Parse(Console.ReadLine());
                        switch (choice2)
                        {
                            case 1:
                                string author = Console.ReadLine();
                                connection.Query<Book>("Select * from Books where Author = @Author;", new { Author = author })
                                    .ToList()
                                    .ForEach(Console.WriteLine);
                                break;
                            case 2:
                                string title = Console.ReadLine();
                                connection.Query<Book>("Select * from Books where Title = @Title';", new { Title = title })
                                    .ToList()
                                    .ForEach(Console.WriteLine);
                                break;
                            case 3:
                                int id = int.Parse(Console.ReadLine());
                                connection.Query<Book>("Select * from Books where Id = @Id;", new { Id = id })
                                    .ToList()
                                    .ForEach(Console.WriteLine);
                                break;
                        }
                        break;
                    case 4:
                        Console.WriteLine("Delete by:\n\t1 - Author\n\t2 - Title");
                        int choice3 = int.Parse(Console.ReadLine());
                        switch (choice3)
                        {
                            case 1:
                                string author = Console.ReadLine();
                                connection.Execute("delete from Books where Author = @Author", new { Author = author });
                                break;
                            case 2:
                                string title = Console.ReadLine();
                                connection.Execute("delete from Books where Title = @Title", new { Title = title });
                                break;
                        }
                        break;
                    case 5:
                        int bookId = int.Parse(Console.ReadLine());
                        if(connection.Query<Book>("select * from Books where Id = @Id", new { Id = bookId}).Count() != 0)
                        {
                            Console.WriteLine("Update\n1 - Author\n2 - Title");
                            int choice4 = int.Parse(Console.ReadLine());
                            switch(choice4)
                            {
                                case 1:
                                    UpdateBookAuthor(connection, bookId);
                                    break;
                                case 2:
                                    UpdateBookTitle(connection, bookId);
                                    break;
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private static void UpdateBookTitle(SqliteConnection connection, int bookId)
        {
            string title = Console.ReadLine();
            string updateQuery = @"
                    update Books
                    set Author = @Author
                    where Title = @Title";

            connection.Execute(updateQuery, new { Id = bookId, Title = title });
        }

        private static void UpdateBookAuthor(SqliteConnection connection, int bookId)
        {
            string author = Console.ReadLine();
            string updateQuery = @"
                    update Books
                    set Author = @Author
                    where Id = @Id";

            connection.Execute(updateQuery, new { Id = bookId, Author = author });
        }

        private static void DeleteBookById(SqliteConnection connection)
        {
            int BookId = int.Parse(Console.ReadLine());
            connection.Execute("delete from Books where Id = @Id", new { Id = BookId });
        }

        private static void UpdateAuthor(SqliteConnection connection, string author, string title)
        {
            string updateQuery = @"
                    update Books
                    set Author = @Author
                    where Title = @Title";

            connection.Execute(updateQuery, new { Title = title, Author = author });
        }

        private static void ReadBooks(SqliteConnection connection)
        {
            connection.Query<Book>("select * from Books").ToList().ForEach(Console.WriteLine);
        }

        private static void AddBook(SqliteConnection connection, Book book)
        {

            string insertBook = @"
                insert into Books(Title, Author)
                values (@Title, @Author);";

            connection.Execute(insertBook, book);
        }

        static void initDB(SqliteConnection connection)
        {
            connection.Execute(@"
            Create table if not exists Books
            (
                Id integer primary key autoincrement,
                Title text not null,
                Author text not null
            );");
        }
    }
}
