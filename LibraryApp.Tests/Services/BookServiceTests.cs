using LibraryApp.Models;
using LibraryApp.Services;
using System.Xml.Linq;

namespace LibraryApp.Tests.Services;

[TestFixture]
public class BookServiceTests
{
    private string _directory = null;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), "BooksLibraryTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public void Add_AppendsBooksInTheOrderTheyAreAdded()
    {
        var list = new BookService();
        var book1 = new Book("Book1", "Author1", 10);
        var book111 = new Book("Book111", "Author2", 20);

        list.Add(book1);
        list.Add(book111);

        Assert.That(list.Books, Is.EqualTo(new[] { book1, book111 }));
    }

    [Test]
    public void Add_RejectsNullBook()
    {
        var list = new BookService();

        Assert.Throws<ArgumentNullException>(() => list.Add(null!));
    }

    [Test]
    public void SortAndSearch_UseTheStoredList()
    {
        var list = new BookService();
        list.Add(new Book("Book1", "Author2", 10));
        list.Add(new Book("Book111", "Author1", 20));
        list.Add(new Book("Book2", "Author1", 30));

        list.Sort();

        Book[] sorted =
        [
            new Book("Book111", "Author1", 20),
            new Book("Book2", "Author1", 30),
            new Book("Book1", "Author2", 10)
        ];
        Assert.That(list.Books, Is.EqualTo(sorted));

        Assert.That(list.Search("111"), Is.EqualTo(new[] { new Book("Book111", "Author1", 20) }));
        Assert.That(list.Books, Is.EqualTo(sorted));
    }

    [Test]
    public void SaveAndLoad_RoundTripBooksInTheCurrentOrder()
    {
        var list = new BookService();
        list.Add(new Book("Book1", "Author1", 10));
        list.Add(new Book("Book2", "Author2", 20));
        list.Add(new Book("Book111", "Author3", 30));
        string path = NewFile();

        list.Save(path);

        var loaded = new BookService();
        loaded.Load(path);

        Assert.That(loaded.Books, Is.EqualTo(list.Books));
    }

    [Test]
    public void Save_WritesTitleAuthorAndPagesElements()
    {
        var list = new BookService();
        list.Add(new Book("Book1", "Author1", 10));
        string path = NewFile();

        list.Save(path);

        XDocument document = XDocument.Load(path);
        Assert.That(document.Root, Is.Not.Null);
        Assert.That(document.Root!.Name.LocalName, Is.EqualTo("books"));

        XElement book = document.Root.Elements("book").Single();
        Assert.That(book.Element("title")!.Value, Is.EqualTo("Book1"));
        Assert.That(book.Element("author")!.Value, Is.EqualTo("Author1"));
        Assert.That(book.Element("pages")!.Value, Is.EqualTo("10"));
    }

    [Test]
    public void Load_ReplacesTheCurrentList()
    {
        string path = NewFile();
        File.WriteAllText(path, """
            <books>
              <book>
                <pages>10</pages>
                <author>  Author1  </author>
                <title>Book1</title>
              </book>
            </books>
            """);

        var list = new BookService();
        list.Add(new Book("Book2", "Author2", 20));

        list.Load(path);

        Assert.That(list.Books, Is.EqualTo(new[] { new Book("Book1", "Author1", 10) }));
    }

    [Test]
    public void Load_ReadsAnEmptyCatalog()
    {
        string path = NewFile();
        File.WriteAllText(path, "<books></books>");
        var list = new BookService();

        list.Load(path);

        Assert.That(list.Books, Is.Empty);
    }

    [Test]
    public void Load_WhenTheFileIsInvalid_KeepsTheCurrentList()
    {
        string path = NewFile();
        File.WriteAllText(path, "<books><book><title>Book1</title></book></books>");
        var list = new BookService();
        list.Add(new Book("Book2", "Author2", 20));

        Assert.Throws<InvalidDataException>(() => list.Load(path));

        Assert.That(list.Books, Is.EqualTo(new[] { new Book("Book2", "Author2", 20) }));
    }

    [Test]
    public void Load_RejectsAMissingFileWrongRootAndBadPageCount()
    {
        var list = new BookService();

        Assert.Throws<FileNotFoundException>(() => list.Load(NewFile()));

        string wrongRoot = NewFile();
        File.WriteAllText(wrongRoot, "<library></library>");
        InvalidDataException wrongRootException = Assert.Throws<InvalidDataException>(() => list.Load(wrongRoot))!;
        Assert.That(wrongRootException.Message, Is.EqualTo("The XML root element must be 'books'"));

        string badPages = NewFile();
        File.WriteAllText(badPages, """
            <books>
              <book>
                <title>Book1</title>
                <author>Author1</author>
                <pages>many</pages>
              </book>
            </books>
            """);
        Assert.Throws<InvalidDataException>(() => list.Load(badPages));

        string zeroPages = NewFile();
        File.WriteAllText(zeroPages, """
            <books>
              <book>
                <title>Book1</title>
                <author>Author1</author>
                <pages>0</pages>
              </book>
            </books>
            """);
        Assert.Throws<InvalidDataException>(() => list.Load(zeroPages));
    }

    [Test]
    public void LoadAndSave_RejectAMissingPath()
    {
        var list = new BookService();

        Assert.Throws<ArgumentNullException>(() => list.Load(null!));
        ArgumentException pathException = Assert.Throws<ArgumentException>(() => list.Save("   "))!;
        Assert.That(pathException.Message, Does.StartWith("File path is required"));
    }

    [Test]
    public void Sort_OrdersByAuthorThenTitleIgnoringCase()
    {
        BookService service = ServiceWith(
            new Book("Book1", "Author2", 1),
            new Book("Book111", "Author1", 2),
            new Book("Book2", "Author1", 3),
            new Book("Book3", "author1", 4));

        service.Sort();

        Assert.That(service.Books, Is.EqualTo(new[]
        {
            new Book("Book111", "Author1", 2),
            new Book("Book2", "Author1", 3),
            new Book("Book3", "author1", 4),
            new Book("Book1", "Author2", 1)
        }));
    }

    [Test]
    public void Sort_KeepsEqualBooksInOriginalOrder()
    {
        BookService service = ServiceWith(
            new Book("book1", "Author2", 1),
            new Book("Book111", "Author1", 2),
            new Book("Book1", "author2", 3),
            new Book("Book2", "Author2", 4));

        service.Sort();

        Assert.That(service.Books, Is.EqualTo(new[]
        {
            new Book("Book111", "Author1", 2),
            new Book("book1", "Author2", 1),
            new Book("Book1", "author2", 3),
            new Book("Book2", "Author2", 4)
        }));
    }

    [Test]
    public void Sort_LeavesEmptyAndSingleItemListsUnchanged()
    {
        var empty = new BookService();
        empty.Sort();
        Assert.That(empty.Books, Is.Empty);

        BookService single = ServiceWith(new Book("Book1", "Author1", 10));
        single.Sort();
        Assert.That(single.Books, Is.EqualTo(new[] { new Book("Book1", "Author1", 10) }));
    }

    [Test]
    public void Search_MatchesPartOfTheTitleAndIgnoresCase()
    {
        Book book2 = new("Book2", "Author3", 30);
        Book book1 = new("Book1", "Author1", 10);
        Book book111 = new("Book111", "Author2", 20);
        BookService service = ServiceWith(book2, book1, book111);

        Assert.That(service.Search("111"), Is.EqualTo(new[] { book111 }));
        Assert.That(service.Search("K11"), Is.EqualTo(new[] { book111 }));
        Assert.That(service.Search("Book"), Is.EqualTo(new[] { book2, book1, book111 }));
        Assert.That(service.Search("Author1"), Is.Empty);
        Assert.That(service.Search("Book9"), Is.Empty);
        Assert.That(service.Books, Is.EqualTo(new[] { book2, book1, book111 }));
    }
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Search_RejectsMissingText(string? titlePart)
    {
        BookService service = ServiceWith(new Book("Book1", "Author1", 10));

        ArgumentException exception = Assert.Catch<ArgumentException>(() => service.Search(titlePart!))!;

        if (titlePart != null)
            Assert.That(exception.Message, Does.StartWith("Search text is required"));
    }

    private static BookService ServiceWith(params Book[] books)
    {
        var service = new BookService();

        foreach (Book book in books)
            service.Add(book);

        return service;
    }

    private string NewFile() => Path.Combine(_directory, $"{Guid.NewGuid():N}.xml");
}

