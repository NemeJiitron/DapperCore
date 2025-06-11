using Dapper;
using Microsoft.Data.Sqlite;
using DapperCore.Entities;
using System.Reflection;
using DapperCore.HomeWork;

namespace DapperCore
{
    public class Program
    {
        static void Main(string[] args)
        {
            // connect to db
            string connectionString = "Data Source=shelter.db";

            using SqliteConnection connection = new SqliteConnection(connectionString);
            connection.Open();
            AppDbContext db = new AppDbContext();
            db.initDb(connection);

            db.Menu(connection);
            

            
        }

        private static void ReadVisitorWithLoans(SqliteConnection connection)
        {
            string visitorsWithLoans = @"
                    select v.Id, v.Name, v.PhoneNumber, v.DateOfBirth,
                        b.Id, b.Title, b.AuthorId,
                        l.LoanDate, l.ReturnDate
                        from Books b
                        join Loans l on l.BookId = b.Id
                        join Visitors v on l.VisitorId = v.Id";

            var visitorMap = new Dictionary<int, Visitor>();

            var visitors = connection.Query<Visitor, Book, string, string, Visitor>(
                    visitorsWithLoans,
                    (visitor, book, loanDate, returnDate) =>
                    {
                        if (!visitorMap.TryGetValue(visitor.Id, out var v))
                        {
                            v = visitor;
                            v.Loans = new List<LoanInfo>();
                            visitorMap.Add(visitor.Id, v);
                        }
                        v.Loans.Add
                        (
                            new LoanInfo
                            {
                                Book = book,
                                LoanDate = DateTime.Parse(loanDate),
                                ReturnDate = string.IsNullOrEmpty(returnDate) ? null : DateTime.Parse(returnDate)
                            }
                        );
                        return v;
                    },
                    splitOn: "Id,LoanDate,ReturnDate"
                    ).Distinct().ToList();

            foreach (Visitor v in visitors)
            {
                Console.WriteLine($"{v.Id}. {v.Name} - {v.Phonenumber}");
                foreach (var l in v.Loans)
                {
                    Console.WriteLine($"{l.Book.ToString()}, LoanDate: {l.LoanDate}, ReturnDate: {(string.IsNullOrEmpty(l.ReturnDate.ToString()) ? "Not returned" : l.ReturnDate.ToString())}");
                }
            }
        }

        private static void AddLoan(SqliteConnection connection)
        {
            string loanBook = @"insert into Loans (VisitorId, BookId, LoanDate, ReturnDate)
                                values (@VisitorId, @BookId, @LoanDate, @ReturnDate)";
            connection.Execute(loanBook,
                new
                {
                    VisitorId = 1,
                    BookId = 1,
                    LoanDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    ReturnDate = (DateTime?)null
                });
        }

        private static void ReadAuthorsAndBooks(SqliteConnection connection)
        {
            string authorsQuery = @"
                    select a.Id, a.Name, a.DateOfBirth, b.Id, b.Title, b.AuthorId from Authors a
                    left join Books b on b.AuthorId = a.Id;";

            var authorMap = new Dictionary<int, Author>();

            var authors = connection.Query<Author, Book, Author>(
                authorsQuery,
                (a, b) =>
                {
                    if (!authorMap.TryGetValue(a.Id, out Author author))
                    {
                        author = a;
                        author.Books = new List<Book>();
                        authorMap.Add(author.Id, author);
                    }
                    if(b != null)
                        author.Books.Add(b);

                    return author;
                },
                splitOn: "Id"
                ).Distinct().ToList();
            foreach (var author in authors)
            {
                Console.WriteLine(author.Id.ToString() + ". " + author.Name + ": ");
                foreach (var book in author.Books)
                {
                    Console.WriteLine(book.ToString());
                }
            }
        }

        private static void AddAuthorAndBooks(SqliteConnection connection, out int authorId, out string bookInsert)
        {
            string authorInsert = @"
                insert into Authors (Name, DateOfBirth)
                values (@Name, @DateOfBirth);
                select last_insert_rowid();";

            authorId = connection.ExecuteScalar<int>(authorInsert, new { Name = "Taras Shevchenko", DateOfBirth = new DateTime(1813, 3, 9) });
            bookInsert = @"
                insert into Books (Title, AuthorId)
                values (@Title, @AuthorId);";
            connection.Execute(bookInsert,
                new[]
                {
                    new { Title = "Kobzar", AuthorId = authorId},
                    new { Title = "Son", AuthorId = authorId}
                }
                );
        }

        private static void ReadVisitors(SqliteConnection connection)
        {
            string visitorsQuery = @"
                    select v.Id, v.Name, v.Phonenumber, v.DateOfBirth, p.PassportNumber, p.VisitorId from Visitors v
                    join Passports p on p.VisitorId = v.Id;";
            var visitors = connection.Query<Visitor, Passport, Visitor>(
                visitorsQuery,
                (v, p) =>
                {
                    v.Passport = p;
                    p.Visitor = v;
                    return v;
                },
                splitOn: "PassportNumber"
                ).ToList();

            foreach (var visitor in visitors)
            {
                Console.WriteLine($"{visitor.Id}. {visitor.Name} --- {visitor.Passport.PassportNumber}");
            }
        }

        private static void AddVisitor(SqliteConnection connection)
        {
            using SqliteTransaction transaction = connection.BeginTransaction();

            string insertVisitor = @"insert into Visitors ([Name], [Phonenumber], [DateOfBirth])
                 values (@Name, @PhoneNumber, @DateOfBirth);
                select last_insert_rowid();";
            string insertPassport = "insert into Passports ([PassportNumber], [VisitorId]) values (@PassportNumber, @VisitorId);";

            int visitorId = connection.ExecuteScalar<int>(insertVisitor, new
            {
                Name = "Nick",
                PhoneNumber = "131123421",
                DateOfBirth = DateTime.Now
            }, transaction);

            connection.Execute(insertPassport, new { PassportNumber = "131312", VisitorId = visitorId }, transaction);
            transaction.Commit();
        }

        private static void Menu(SqliteConnection connection)
        {
            int choice1 = 1;
            while (choice1 != 0)
            {
                Console.WriteLine("1 - Add Book\n2 - Read all books\n3 - Search by...\n4 - Delete by...\n5 - Update book\n0 - Quit");
                choice1 = int.Parse(Console.ReadLine());
                switch (choice1)
                {
                    case 1:
                        AddBook(connection, new Book { AuthorId = int.Parse(Console.ReadLine()), Title = Console.ReadLine() });
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
                        if (connection.Query<Book>("select * from Books where Id = @Id", new { Id = bookId }).Count() != 0)
                        {
                            Console.WriteLine("Update\n1 - Author\n2 - Title");
                            int choice4 = int.Parse(Console.ReadLine());
                            switch (choice4)
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

            create table if not exists Authors
            (
                Id integer primary key autoincrement,
                Name text not null,
                DateOfBirth text not null
            );
            create table if not exists Books
            (
                Id integer primary key autoincrement,
                Title text not null,
                AuthorId integer not null,
                foreign key (AuthorId) references Authors(Id)
            );
            create table if not exists Visitors
            (
                Id integer primary key autoincrement,
                Name text not null,
                Phonenumber text not null,
                DateOfBirth text not null
            );
            create table if not exists Passports
            (
                Id integer primary key autoincrement,
                PassportNumber text not null,
                VisitorId integer not null unique,
                foreign key (VisitorId) references Visitors(Id)
            );
            create table if not exists Loans
            (
                VisitorId integer not null,
                BookId integer not null,
                LoanDate text not null,
                ReturnDate text,
                primary key (VisitorId, BookId, LoanDate),
                foreign key (VisitorId) references Visitors(Id),
                foreign key (BookId) references Books(Id)
            );
            ");

        }
    }
}
