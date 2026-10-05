using System.Globalization;

namespace LibraryApp.Models
{
    public record Book
    {
        public Book(string title, string author, int pages)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException(Resources.Messages.TitleIsRequired, nameof(title));

            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException(Resources.Messages.AuthorIsRequired, nameof(author));

            if (pages <= 0)
                throw new ArgumentOutOfRangeException(nameof(pages), pages, Resources.Messages.PagesMustBePositive);

            Title = title.Trim();
            Author = author.Trim();
            Pages = pages;
        }

        public string Title { get; }

        public string Author { get; }

        public int Pages { get; }

        public override string ToString() =>
            string.Format(CultureInfo.CurrentCulture, Resources.Messages.BookDescription, Author, Title, Pages);
    }
}
