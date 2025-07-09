using System.ComponentModel.DataAnnotations;

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
            InvalidEmailOrPassword = 3,
            AccountNotFound = 4,
            AccountAlreadyExists = 5,

        }

        public enum RoleType
        {
            Admin = 0,
            Reviewer = 1,
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
            NameAscending = 0,
            NameDescending = 1,
            BookCountAscending = 2,
            BookCountDescending = 3,
            CreatedTimeAscending = 4,
            CreatedTimeDescending = 5,
        }

        public enum BookSearchType
        {
            AllBooks = 0,
            TopBooks = 1,
            NewBooks = 2,
        }
        public enum BookSortType
        {
            GenreAscending = 0,
            GenreDescending = 1,
            TitleAscending = 2,
            TitleDescending = 3,
            AuthorAscending = 4,
            AuthorDescending = 5,
            RatingAscending = 6,
            RatingDescending = 7,
            SeriesAscending = 8,
            SeriesDescending = 9,
            CreatedTimeAscending = 10,
            CreatedTimeDescending = 11,
        }
    }
}
