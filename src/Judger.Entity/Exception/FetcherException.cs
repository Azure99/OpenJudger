namespace Judger.Entity.Exception
{
    /// <summary>
    /// Fetcher异常
    /// </summary>
    public class FetcherException : System.Exception
    {
        /// <summary>
        /// Fetcher异常
        /// </summary>
        /// <param name="message">Message</param>
        public FetcherException(string message) : base(message)
        { }
    }
}