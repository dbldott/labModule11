
using System;
using System.Collections.Generic;
using System.Linq;


{
    public class Book
    {
        public string Isbn { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public bool IsAvailable { get; private set; } = true;

        public Book(string isbn, string title, string author)
        {
            Isbn = isbn ?? throw new ArgumentNullException(nameof(isbn));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
        }

        public void MarkAsLoaned()
        {
            IsAvailable = false;
        }

        public void MarkAsAvailable()
        {
            IsAvailable = true;
        }

        public override string ToString() => $"{Title} ({Author}), ISBN: {Isbn}";
    }

    public class Reader
    {
        public int Id { get; }
        public string Name { get; private set; }
        public string Email { get; private set; }

        public Reader(int id, string name, string email)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? throw new ArgumentNullException(nameof(email));
        }

        public Loan BorrowBook(Book book, Librarian librarian, LibraryService library)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (librarian == null) throw new ArgumentNullException(nameof(librarian));
            if (library == null) throw new ArgumentNullException(nameof(library));

            return librarian.IssueLoan(book, this, library);
        }

        public void ReturnBook(Loan loan, Librarian librarian, LibraryService library)
        {
            if (loan == null) throw new ArgumentNullException(nameof(loan));
            if (librarian == null) throw new ArgumentNullException(nameof(librarian));
            if (library == null) throw new ArgumentNullException(nameof(library));

            librarian.CompleteLoan(loan, library);
        }

        public override string ToString() => $"{Name} (#{Id})";
    }

    public class Librarian
    {
        public int Id { get; }
        public string Name { get; private set; }
        public string Position { get; private set; }

        public Librarian(int id, string name, string position)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        public void AddBook(Book book, LibraryService library)
        {
            library.AddBook(book);
        }

        public void RemoveBook(Book book, LibraryService library)
        {
            library.RemoveBook(book);
        }

        public void RegisterReader(Reader reader, LibraryService library)
        {
            library.RegisterReader(reader);
        }

        public Loan IssueLoan(Book book, Reader reader, LibraryService library)
        {
            return library.IssueLoan(book, reader);
        }

        public void CompleteLoan(Loan loan, LibraryService library)
        {
            library.CompleteLoan(loan);
        }

        public override string ToString() => $"{Name} ({Position})";
    }

    public class Loan
    {
        public int Id { get; }
        public Book Book { get; }
        public Reader Reader { get; }
        public DateTime LoanDate { { get; } }
        public DateTime? ReturnDate { get; private set; }

        public Loan(int id, Book book, Reader reader, DateTime loanDate)
        {
            Id = id;
            Book = book ?? throw new ArgumentNullException(nameof(book));
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            LoanDate = loanDate;
        }

        public void IssueLoan()
        {
            Book.MarkAsLoaned();
        }

        public void CompleteLoan()
        {
            ReturnDate = DateTime.Now;
            Book.MarkAsAvailable();
        }

        public bool IsActive() => !ReturnDate.HasValue;
    }

    public class LibraryService
    {
        private readonly List<Book> _books = new();
        private readonly List<Reader> _readers = new();
        private readonly List<Loan> _loans = new();
        private int _nextLoanId = 1;

        public IReadOnlyCollection<Book> Books => _books.AsReadOnly();
        public IReadOnlyCollection<Reader> Readers => _readers.AsReadOnly();
        public IReadOnlyCollection<Loan> Loans => _loans.AsReadOnly();

        public void AddBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            _books.Add(book);
        }

        public void RemoveBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            _books.Remove(book);
        }

        public void RegisterReader(Reader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            _readers.Add(reader);
        }

        public Loan IssueLoan(Book book, Reader reader)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (!book.IsAvailable) throw new InvalidOperationException("Book is not available.");

            var loan = new Loan(_nextLoanId++, book, reader, DateTime.Now);
            loan.IssueLoan();
            _loans.Add(loan);
            return loan;
        }

        public void CompleteLoan(Loan loan)
        {
            if (loan == null) throw new ArgumentNullException(nameof(loan));
            if (!_loans.Contains(loan)) throw new InvalidOperationException("Loan not found.");
            if (!loan.IsActive()) return;

            loan.CompleteLoan();
        }

        public IEnumerable<Book> GetAvailableBooks()
        {
            return _books.Where(b => b.IsAvailable);
        }

        public IEnumerable<Book> FindBooks(string title = null, string author = null)
        {
            return _books.Where(b =>
                (string.IsNullOrWhiteSpace(title) || b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrWhiteSpace(author) || b.Author.Contains(author, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
