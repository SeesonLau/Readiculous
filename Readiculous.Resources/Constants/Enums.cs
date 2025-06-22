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
            Admin = 0,
            Reviewer = 1,
        }

        public enum UserSortType
        {
            UsernameAscending = 0,
            UsernameDescending = 1,
            CreatedTimeAscending = 2,
            CreatedTimeDescending = 3,
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
    }
}
