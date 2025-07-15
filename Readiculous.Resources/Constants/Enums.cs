using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Readiculous.Resources.Constants
{
    /// <summary>
    /// Class for enumerated values
    /// </summary>
    public class Enums
    {
        /// <summary>
        /// API Result Status
        /// </summary>
        public enum Status
        {
            Success,
            Error,
            CustomErr,
        }

        /// <summary>
        /// Login Result
        /// </summary>
        public enum LoginResult
        {
            Success = 0,
            Failed = 1,
            InvalidRole = 2,
        }

        public enum RoleType
        {
            Reviewer = 0,
            Admin = 1,
        }

        public enum UserSortType
        {
            [Display(Name = "Latest")]
            Latest = 0,
            [Display(Name = "Oldest")]
            Oldest = 1,
            [Display(Name = "Username: A-Z")]
            UsernameAscending = 2,
            [Display(Name = "Username: Z-A")]
            UsernameDescending = 3,
        }

        public enum AccessStatus
        {
            FirstTime = 0,
            Verified = 1,
            Blocked = 2,
        }

        public enum GenreSortType
        {
            [Display(Name = "Latest")]
            Latest = 0,
            [Display(Name = "Oldest")]
            Oldest = 1,
            [Display(Name = "Genre: A-Z")]
            NameAscending = 2,
            [Display(Name = "Genre: Z-A")]
            NameDescending = 3,
            [Display(Name = "Book Count: 🡡")]
            BookCountAscending = 4,
            [Display(Name = "Book Count: 🡣")]
            BookCountDescending = 5,
        }

        public enum BookSearchType
        {
            [Display(Name = "All Books")]
            AllBooks = 0,
            [Display(Name = "Top Books")]
            TopBooks = 1,
            [Display(Name = "New Books")]
            NewBooks = 2,
        }
        public enum BookSortType
        {
            [Display(Name = "Latest")]
            Latest = 0,
            [Display(Name = "Oldest")]
            Oldest = 1,
            [Display(Name = "Title: A-Z")]
            TitleAscending = 2,
            [Display(Name = "Title: Z-A")]
            TitleDescending = 3,
            [Display(Name = "Author: A-Z")]
            AuthorAscending = 4,
            [Display(Name = "Author: Z-A")]
            AuthorDescending = 5,
            [Display(Name = "Top Books: 🡡")]
            RatingAscending = 6,
            [Display(Name = "Top Books: 🡣")]
            RatingDescending = 7,
            [Display(Name = "New Books: 🡡")]
            NewBooksAscending = 8,
            [Display(Name = "New Books: 🡣")]
            NewBooksDescending = 9,
        }
    }
}
