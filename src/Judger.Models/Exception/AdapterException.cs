namespace Judger.Models.Exception
{
    /// <summary>
    /// Adapter异常
    /// </summary>
    public class AdapterException : BaseException
    {
        /// <summary>
        /// Adapter异常
        /// </summary>
        /// <param name="message">Message</param>
        public AdapterException(string message) : base(message)
        { }
    }
}