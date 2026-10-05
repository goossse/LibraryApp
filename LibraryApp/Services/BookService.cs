using LibraryApp.Helpers;
using LibraryApp.Models;

namespace LibraryApp.Services;

public class BookService
{
    private readonly List<Book> _books = new();

    public IReadOnlyList<Book> Books => _books;

    public void Load(string filePath)
    {
        List<Book> loaded = BookXmlHelper.Read(filePath);

        _books.Clear();

        _books.AddRange(loaded);
    }

    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);

        _books.Add(book);
    }

    public void Sort()
    {
        if (_books.Count < 2)
            return;

        var items = _books.ToArray();
        var buffer = new Book[items.Length];

        MergeSort(items, buffer, 0, items.Length);

        for (int i = 0; i < items.Length; i++)
            _books[i] = items[i];
    }

    public IReadOnlyList<Book> Search(string titlePart)
    {
        if (string.IsNullOrWhiteSpace(titlePart))
            throw new ArgumentException(Resources.Messages.SearchTextIsRequired, nameof(titlePart));

        string part = titlePart.Trim();
        var matches = new List<Book>();

        foreach (Book book in _books)
        {
            if (TitleContains(book.Title, part))
                matches.Add(book);
        }

        return matches;
    }

    public void Save(string filePath)
    {
        BookXmlHelper.Write(filePath, _books);
    }

    private static void MergeSort(Book[] items, Book[] buffer, int start, int end)
    {
        int length = end - start;

        if (length <= 1)
            return;

        int middle = start + (length / 2);

        MergeSort(items, buffer, start, middle);
        MergeSort(items, buffer, middle, end);
        Merge(items, buffer, start, middle, end);
    }

    private static void Merge(Book[] items, Book[] buffer, int start, int midle, int end)
    {
        int left = start;
        int right = midle;
        int index = start;

        while (left < midle && right < end)
        {
            if (Compare(items[left], items[right]) <= 0)
                buffer[index++] = items[left++];
            else
                buffer[index++] = items[right++];
        }

        while (left < midle)
            buffer[index++] = items[left++];

        while (right < end)
            buffer[index++] = items[right++];

        for (int i = start; i < end; i++)
            items[i] = buffer[i];
    }

    private static int Compare(Book left, Book right)
    {
        int byAuthor = string.Compare(left.Author, right.Author, StringComparison.OrdinalIgnoreCase);

        if (byAuthor != 0)
            return byAuthor;

        return string.Compare(left.Title, right.Title, StringComparison.OrdinalIgnoreCase);
    }


    private static bool TitleContains(string title, string part)
    {
        if (part.Length > title.Length)
            return false;

        int lastStart = title.Length - part.Length;

        for (int start = 0; start <= lastStart; start++)
        {
            if (title.AsSpan(start, part.Length).Equals(part, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
