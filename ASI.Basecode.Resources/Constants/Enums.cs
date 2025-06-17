namespace ASI.Basecode.Resources.Constants
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

        public enum UserSearchType
        {
            UsernameAscending = 0,
            UsernameDescending = 1,
            IDAscending = 2,
            IDDescending = 3,
        }
    }
}
