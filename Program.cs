using Npgsql;
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        var db = new BookStoreDatabase("localhost", "Library", "postgres", "");

        try
        {
            if (!db.TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return;
            }

            bool exitRequested = false;
            while (!exitRequested)
            {
                Console.Clear();
                Console.WriteLine("=== Система управления книжным магазином ===");
                Console.WriteLine("1. Показать все книги с авторами");
                Console.WriteLine("2. Добавить книгу");
                Console.WriteLine("3. Добавить автора");
                Console.WriteLine("4. Найти книгу по автору");
                Console.WriteLine("5. Обновить информацию о книге");
                Console.WriteLine("6. Удалить книгу");
                Console.WriteLine("7. Показать всех авторов");
                Console.WriteLine("8. Выход");
                Console.Write("Выберите действие: ");

                var input = Console.ReadLine();
                Console.Clear();

                try
                {
                    switch (input)
                    {
                        case "1":
                            PrintBooksTableWithAuthors(db.GetBooksWithAuthors());
                            break;
                        case "2":
                            AddNewBook(db);
                            break;
                        case "3":
                            AddNewAuthor(db);
                            break;
                        case "4":
                            SearchBooksByAuthor(db);
                            break;
                        case "5":
                            UpdateBookInfo(db);
                            break;
                        case "6":
                            DeleteBook(db);
                            break;
                        case "7":
                            ShowAllAuthors(db);
                            break;
                        case "8":
                            exitRequested = true;
                            continue;
                        default:
                            throw new InvalidOperationException("Некорректный пункт меню. Введите число от 1 до 8.");
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ошибка: {ex.Message}");
                    Console.ResetColor();
                }

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Критическая ошибка: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("\nНажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }

    static void ShowAllAuthors(BookStoreDatabase db)
    {
        try
        {
            var authors = db.GetAllAuthors();
            if (authors.Count == 0)
            {
                throw new InvalidOperationException("В базе нет авторов.");
            }

            Console.WriteLine("Список авторов:");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("| {0,-5} | {1,-20} | {2,-20} |", "ID", "Имя", "Фамилия");
            Console.WriteLine(new string('-', 50));

            foreach (var author in authors)
            {
                Console.WriteLine("| {0,-5} | {1,-20} | {2,-20} |",
                                author.AuthorId,
                                author.FirstName,
                                author.LastName);
            }
            Console.WriteLine(new string('-', 50));
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при получении списка авторов", ex);
        }
    }

    static void AddNewAuthor(BookStoreDatabase db)
    {
        Console.WriteLine("=== Добавление нового автора ===");

        try
        {
            Console.Write("Имя: ");
            var firstName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("Имя автора не может быть пустым.");

            Console.Write("Фамилия: ");
            var lastName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Фамилия автора не может быть пустой.");

            Console.Write("Биография (необязательно): ");
            var bio = Console.ReadLine();

            var authorId = db.AddAuthor(firstName, lastName, bio);
            Console.WriteLine($"\nАвтор успешно добавлен с ID: {authorId}");
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при добавлении автора", ex);
        }
    }

    static void AddNewBook(BookStoreDatabase db)
    {
        Console.WriteLine("=== Добавление новой книги ===");

        try
        {
            var authors = db.GetAllAuthors();
            if (authors.Count == 0)
                throw new InvalidOperationException("Сначала нужно добавить авторов!");

            Console.WriteLine("\nСписок доступных авторов:");
            ShowAllAuthors(db);

            Console.Write("\nНазвание: ");
            var title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название книги не может быть пустым.");

            Console.Write("ISBN: ");
            var isbn = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN не может быть пустым.");

            Console.Write("Цена: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price) || price <= 0)
                throw new ArgumentException("Цена должна быть положительным числом.");

            Console.Write("Издательство (необязательно): ");
            var publisher = Console.ReadLine();

            Console.Write("Дата публикации (ГГГГ-ММ-ДД, необязательно): ");
            DateTime? pubDate = null;
            var dateInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(dateInput))
            {
                if (!DateTime.TryParse(dateInput, out var tempDate))
                    throw new ArgumentException("Некорректный формат даты. Используйте ГГГГ-ММ-ДД.");
                pubDate = tempDate;
            }

            Console.Write("ID автора: ");
            if (!int.TryParse(Console.ReadLine(), out var authorId) || !authors.Exists(a => a.AuthorId == authorId))
                throw new ArgumentException("Некорректный ID автора.");

            var bookId = db.AddBook(title, isbn, price, publisher, pubDate, authorId);
            Console.WriteLine($"\nКнига успешно добавлена с ID: {bookId}");
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при добавлении книги", ex);
        }
    }

    static void SearchBooksByAuthor(BookStoreDatabase db)
    {
        Console.WriteLine("=== Поиск книг по автору ===");

        try
        {
            var authors = db.GetAllAuthors();
            if (authors.Count == 0)
                throw new InvalidOperationException("В базе нет авторов.");

            ShowAllAuthors(db);
            Console.Write("\nВведите ID автора: ");
            if (!int.TryParse(Console.ReadLine(), out var authorId))
                throw new ArgumentException("Некорректный ID автора.");

            var author = authors.Find(a => a.AuthorId == authorId);
            if (author == null)
                throw new KeyNotFoundException("Автор с таким ID не найден.");

            var books = db.GetBooksByAuthorId(authorId);
            if (books.Count > 0)
            {
                Console.WriteLine($"\nКниги автора {author.FirstName} {author.LastName}:");
                PrintBooksTable(books);
            }
            else
            {
                throw new InvalidOperationException("\nУ этого автора нет книг в базе.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при поиске книг", ex);
        }
    }

    static void UpdateBookInfo(BookStoreDatabase db)
    {
        Console.WriteLine("=== Обновление информации о книге ===");

        try
        {
            var books = db.GetBooksWithAuthors();
            if (books.Count == 0)
                throw new InvalidOperationException("В базе нет книг.");

            PrintBooksTableWithAuthors(books);

            Console.Write("\nВведите ID книги для обновления: ");
            if (!int.TryParse(Console.ReadLine(), out var bookId))
                throw new ArgumentException("Некорректный ID книги.");

            Console.Write("Новое название: ");
            var title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Название книги не может быть пустым.");

            Console.Write("Новая цена: ");
            if (!decimal.TryParse(Console.ReadLine(), out var price) || price <= 0)
                throw new ArgumentException("Цена должна быть положительным числом.");

            if (db.UpdateBook(bookId, title, price))
            {
                Console.WriteLine("\nИнформация о книге успешно обновлена.");
                PrintBooksTableWithAuthors(db.GetBooksWithAuthors());
            }
            else
            {
                throw new KeyNotFoundException("Книга с указанным ID не найдена.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при обновлении книги", ex);
        }
    }

    static void DeleteBook(BookStoreDatabase db)
    {
        Console.WriteLine("=== Удаление книги ===");

        try
        {
            var books = db.GetBooksWithAuthors();
            if (books.Count == 0)
                throw new InvalidOperationException("В базе нет книг.");

            PrintBooksTableWithAuthors(books);

            Console.Write("\nВведите ID книги для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out var bookId))
                throw new ArgumentException("Некорректный ID книги.");

            if (db.DeleteBook(bookId))
            {
                Console.WriteLine("\nКнига успешно удалена.");
                PrintBooksTableWithAuthors(db.GetBooksWithAuthors());
            }
            else
            {
                throw new KeyNotFoundException("Книга с указанным ID не найдена.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при удалении книги", ex);
        }
    }

    static void PrintBooksTableWithAuthors(List<BookWithAuthor> books)
    {
        if (books.Count == 0)
            throw new InvalidOperationException("В базе данных нет книг.");

        Console.WriteLine("\nСписок книг в магазине (с авторами):");
        Console.WriteLine(new string('-', 110));
        Console.WriteLine("| {0,-5} | {1,-25} | {2,-15} | {3,-10} | {4,-15} | {5,-20} |",
                         "ID", "Название", "ISBN", "Цена", "Издательство", "Автор");
        Console.WriteLine(new string('-', 110));

        foreach (var book in books)
        {
            Console.WriteLine("| {0,-5} | {1,-25} | {2,-15} | {3,-10:C2} | {4,-15} | {5,-20} |",
                            book.BookId,
                            Truncate(book.Title, 25),
                            book.ISBN,
                            book.Price,
                            Truncate(book.Publisher ?? "-", 15),
                            $"{book.AuthorFirstName} {book.AuthorLastName}");
        }
        Console.WriteLine(new string('-', 110));
    }

    static void PrintBooksTable(List<Book> books)
    {
        if (books.Count == 0)
            throw new InvalidOperationException("Нет книг для отображения.");

        Console.WriteLine("\nСписок книг:");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("| {0,-5} | {1,-30} | {2,-15} | {3,-10} | {4,-15} |",
                         "ID", "Название", "ISBN", "Цена", "Издательство");
        Console.WriteLine(new string('-', 80));

        foreach (var book in books)
        {
            Console.WriteLine("| {0,-5} | {1,-30} | {2,-15} | {3,-10:C2} | {4,-15} |",
                            book.BookId,
                            Truncate(book.Title, 30),
                            book.ISBN,
                            book.Price,
                            Truncate(book.Publisher ?? "-", 15));
        }
        Console.WriteLine(new string('-', 80));
    }

    static string Truncate(string value, int maxLength)
    {
        return string.IsNullOrEmpty(value)
            ? value
            : value.Length <= maxLength
                ? value
                : value.Substring(0, maxLength - 3) + "...";
    }
}

public class BookStoreDatabase
{
    private readonly string _connectionString;

    public BookStoreDatabase(string host, string database, string username, string password)
    {
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Неверные параметры подключения к базе данных");

        _connectionString = $"Host={host};Database={database};Username={username};" +
                          (string.IsNullOrWhiteSpace(password) ? "" : $"Password={password};");
    }

    public bool TestConnection()
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка подключения к базе данных", ex);
        }
    }

    public int AddBook(string title, string isbn, decimal price, string publisher = null,
                      DateTime? publicationDate = null, int authorId = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название книги не может быть пустым");
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не может быть пустым");
        if (price <= 0)
            throw new ArgumentException("Цена должна быть положительной");
        if (authorId <= 0)
            throw new ArgumentException("Неверный ID автора");

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        using var transaction = connection.BeginTransaction();

        try
        {
            var sql = @"INSERT INTO books (title, isbn, price, publisher, publication_date) 
                        VALUES (@title, @isbn, @price, @publisher, @publicationDate)
                        RETURNING book_id;";

            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@isbn", isbn);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@publisher", publisher ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@publicationDate", publicationDate ?? (object)DBNull.Value);

            var bookId = (int)cmd.ExecuteScalar();

            sql = @"INSERT INTO book_authors (book_id, author_id) 
                    VALUES (@bookId, @authorId);";

            using var authorCmd = new NpgsqlCommand(sql, connection, transaction);
            authorCmd.Parameters.AddWithValue("@bookId", bookId);
            authorCmd.Parameters.AddWithValue("@authorId", authorId);
            authorCmd.ExecuteNonQuery();

            transaction.Commit();
            return bookId;
        }
        catch (PostgresException ex) when (ex.SqlState == "23503") // Ошибка внешнего ключа
        {
            transaction.Rollback();
            throw new Exception("Автор с указанным ID не существует", ex);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public List<BookWithAuthor> GetBooksWithAuthors()
    {
        try
        {
            var result = new List<BookWithAuthor>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = @"SELECT b.book_id, b.title, b.isbn, b.price, b.publisher, b.publication_date,
                               a.author_id, a.first_name, a.last_name
                        FROM books b
                        JOIN book_authors ba ON b.book_id = ba.book_id
                        JOIN authors a ON ba.author_id = a.author_id
                        ORDER BY b.book_id;";

            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new BookWithAuthor
                {
                    BookId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    ISBN = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    Publisher = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PublicationDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                    AuthorId = reader.GetInt32(6),
                    AuthorFirstName = reader.GetString(7),
                    AuthorLastName = reader.GetString(8)
                });
            }

            if (result.Count == 0)
                throw new InvalidOperationException("В базе нет книг с авторами");

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при получении списка книг с авторами", ex);
        }
    }

    public List<Book> GetBooksByAuthorId(int authorId)
    {
        if (authorId <= 0)
            throw new ArgumentException("Неверный ID автора");

        try
        {
            var books = new List<Book>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = @"SELECT b.book_id, b.title, b.isbn, b.price, b.publisher, b.publication_date
                        FROM books b
                        JOIN book_authors ba ON b.book_id = ba.book_id
                        WHERE ba.author_id = @authorId
                        ORDER BY b.title;";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@authorId", authorId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                books.Add(new Book
                {
                    BookId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    ISBN = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    Publisher = reader.IsDBNull(4) ? null : reader.GetString(4),
                    PublicationDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                });
            }

            return books;
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при поиске книг по автору", ex);
        }
    }

    public bool UpdateBook(int bookId, string title, decimal price)
    {
        if (bookId <= 0)
            throw new ArgumentException("Неверный ID книги");
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название книги не может быть пустым");
        if (price <= 0)
            throw new ArgumentException("Цена должна быть положительной");

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = "UPDATE books SET title = @title, price = @price WHERE book_id = @bookId;";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@title", title);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@bookId", bookId);

            return cmd.ExecuteNonQuery() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при обновлении книги", ex);
        }
    }

    public bool DeleteBook(int bookId)
    {
        if (bookId <= 0)
            throw new ArgumentException("Неверный ID книги");

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = "DELETE FROM book_authors WHERE book_id = @bookId;";
                using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@bookId", bookId);
                cmd.ExecuteNonQuery();

                sql = "DELETE FROM books WHERE book_id = @bookId;";
                using var deleteCmd = new NpgsqlCommand(sql, connection, transaction);
                deleteCmd.Parameters.AddWithValue("@bookId", bookId);
                var result = deleteCmd.ExecuteNonQuery() > 0;

                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при удалении книги", ex);
        }
    }

    public List<Author> GetAllAuthors()
    {
        try
        {
            var authors = new List<Author>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT author_id, first_name, last_name, biography FROM authors ORDER BY last_name, first_name;";

            using var cmd = new NpgsqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                authors.Add(new Author
                {
                    AuthorId = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Biography = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return authors;
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при получении списка авторов", ex);
        }
    }

    public int AddAuthor(string firstName, string lastName, string biography = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Имя автора не может быть пустым");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Фамилия автора не может быть пустой");

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = @"INSERT INTO authors (first_name, last_name, biography) 
                        VALUES (@firstName, @lastName, @biography)
                        RETURNING author_id;";

            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@firstName", firstName);
            cmd.Parameters.AddWithValue("@lastName", lastName);
            cmd.Parameters.AddWithValue("@biography", biography ?? (object)DBNull.Value);

            return (int)cmd.ExecuteScalar();
        }
        catch (Exception ex)
        {
            throw new Exception("Ошибка при добавлении автора", ex);
        }
    }
}

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public string ISBN { get; set; }
    public decimal Price { get; set; }
    public string Publisher { get; set; }
    public DateTime? PublicationDate { get; set; }
}

public class BookWithAuthor : Book
{
    public int AuthorId { get; set; }
    public string AuthorFirstName { get; set; }
    public string AuthorLastName { get; set; }
}

public class Author
{
    public int AuthorId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Biography { get; set; }
}