namespace Readiculous.Resources.Constants
{
    /// <summary>
    /// Class for variables with constant values
    /// </summary>
    public class Const
    {
        /// <summary>
        ///API result success
        /// </summary>
        public const string ApiResultSuccess = "Success";

        /// <summary>
        ///API result error
        /// </summary>
        public const string ApiResultError = "Error";

        /// <summary>
        /// System
        /// </summary>
        public const string System = "sys";

        /// <summary>
        /// Api Key Header Name
        /// </summary>
        public const string ApiKey = "X-Basecode-API-Key";

        /// <summary>
        /// authentication scheme Name
        /// </summary>
        public const string AuthenticationScheme = "Readiculous_Basecode";

        /// <summary>
        /// authentication Issuer
        /// </summary>
        public const string Issuer = "Readiculous";

        /// <summary>
        /// Bucket name for file storage in Readiculous
        /// </summary>
        public const string BucketName = "readiculous-bucket";

        /// <summary>
        /// Represents the directory name for storing user-related data.
        /// </summary>
        public const string UserDirectory = "users";

        /// <summary>
        /// Represents the directory name for storing book-related data.
        /// </summary>
        public const string BookDirectory = "books";

        /// <summary>
        /// Represents the path in the storage where files are accessible publicly.
        /// </summary>
        public const string StoragePath = $"/storage/v1/object/public/{BucketName}/";
    }
}
