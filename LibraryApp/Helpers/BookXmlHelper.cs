using LibraryApp.Models;
using System.Globalization;
using System.Xml.Linq;

namespace LibraryApp.Helpers;

public static class BookXmlHelper
{
    private const string XmlVersion = "1.0";
    private const string XmlEncoding = "utf-8";
    private const string BooksElement = "books";
    private const string BookElement = "book";
    private const string TitleElement = "title";
    private const string AuthorElement = "author";
    private const string PagesElement = "pages";


    public static List<Book> Read(string filePath)
    {
        string path = RequirePath(filePath);
        XDocument document = XDocument.Load(path);


        if (document.Root == null || document.Root.Name != BooksElement)
            throw new InvalidDataException(Format(Resources.Messages.XmlRootMustBe, BooksElement));

        var books = new List<Book>();
        int index = 0;

        foreach (XElement element in document.Root.Elements(BookElement))
        {
            index++;
            books.Add(ReadBook(element, index));
        }

        return books;
    }

    internal static void Write(string filePath, IReadOnlyList<Book> books)
    {
        string path = RequirePath(filePath);
        ArgumentNullException.ThrowIfNull(books);

        var document = new XDocument(
            new XDeclaration(XmlVersion, XmlEncoding, null),
            new XElement(BooksElement,
                books.Select(book => new XElement(
                    BookElement,
                    new XElement(TitleElement, book.Title),
                    new XElement(AuthorElement, book.Author),
                    new XElement(PagesElement, book.Pages.ToString(CultureInfo.InvariantCulture))))));

        document.Save(path);
    }

    private static Book ReadBook(XElement element, int index)
    {
        string? title = element.Element(TitleElement)?.Value;
        string? author = element.Element(AuthorElement)?.Value;
        string? pagesText = element.Element(PagesElement)?.Value;

        if (title == null || author == null || pagesText == null)
            throw new InvalidDataException(Format(Resources.Messages.BookFieldsRequired, index));

        if (!int.TryParse(pagesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pages))
            throw new InvalidDataException(Format(Resources.Messages.BookPageAmountInvalid, index));

        try
        {
            return new Book(title, author, pages);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException(Format(Resources.Messages.BookIsInvalid, index, exception.Message), exception);
        }
    }

    private static string RequirePath(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(Resources.Messages.FilePathIsRequired, nameof(filePath));

        return filePath;
    }

    private static string Format(string template, params object?[] args) =>
        string.Format(CultureInfo.CurrentCulture, template, args);
}
