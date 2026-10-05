using LibraryApp.Models;

namespace LibraryApp.Tests.Models;

[TestFixture]
public class BookTests
{
    [Test]
    public void Contructor_TrimsTitleAndAuthor()
    {
        var book = new Book("  Book1  ", "  Author1  ", 10);

        Assert.That(book.Title, Is.EqualTo("Book1"));
        Assert.That(book.Author, Is.EqualTo("Author1"));
        Assert.That(book.Pages, Is.EqualTo(10));
        Assert.That(book.ToString(), Is.EqualTo("Author1, Book1, 10 pages"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Contructor_RejectsMissingTitle(string? title)
    {
        ArgumentException exception = Assert.Catch<ArgumentException>(() => new Book(title!, "Author1", 10))!;

        if (title != null)
            Assert.That(exception.Message, Does.StartWith("Title is required"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void Contructor_RejectsMissingAuthor(string? author)
    {
        ArgumentException exception = Assert.Catch<ArgumentException>(() => new Book("Book1", author!, 10))!;

        if (author != null)
            Assert.That(exception.Message, Does.StartWith("Author is required"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Contructor_RejectsNonPositivePageCount(int pages)
    {
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new Book("Book1", "Author1", pages))!;

        Assert.That(exception.Message, Does.Contain("Pages amount must be greater than 0"));
    }

    [Test]
    public void Equals_CompareTitleAuthorAndPages()
    {
        var first = new Book("Book1", "Author1", 10);
        var same = new Book("Book1", "Author1", 10);
        var differentPages = new Book("Book1", "Author1", 11);

        Assert.That(first, Is.EqualTo(same));
        Assert.That(first, Is.Not.EqualTo(differentPages));
    }
}