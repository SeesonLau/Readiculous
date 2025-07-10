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
            [Display(Name = "Username: A-Z")]
            UsernameAscending = 0,
            [Display(Name = "Username: Z-A")]
            UsernameDescending = 1,
            [Display(Name = "Latest")]
            Latest = 2,  
            [Display(Name = "Oldest")]
            Oldest = 3
        }

        public enum AccessStatus
        {
            FirstTime = 0,
            Verified = 1,
            Blocked = 2,
        }

        public enum GenreSortType
        {
            [Display(Name = "Username: A-Z")]
            NameAscending = 0,
            [Display(Name = "Username: Z-A")]
            NameDescending = 1,
            [Display(Name = "Book Count: 🡡")]
            BookCountAscending = 2,
            [Display(Name = "Book Count: 🡣")]
            BookCountDescending = 3,
            [Display(Name = "Oldest")]
            Oldest = 4,
            [Display(Name = "Latest")]
            Latest = 5,
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
            [Display(Name = "Title: A-Z")]
            TitleAscending = 0,
            [Display(Name = "Title: Z-A")]
            TitleDescending = 1,
            [Display(Name = "Author: A-Z")]
            AuthorAscending = 2,
            [Display(Name = "Author: Z-A")]
            AuthorDescending = 3,
            [Display(Name = "Rating: 🡡")]
            RatingAscending = 4,
            [Display(Name = "Rating: 🡣")]
            RatingDescending = 5,
            [Display(Name = "Oldest")]
            Oldest = 6,
            [Display(Name = "Latest")]
            Latest = 7,
        }
    }
}
