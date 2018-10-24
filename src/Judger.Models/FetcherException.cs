using System;
using System.Collections.Generic;
using System.Text;

namespace Judger.Models
{
    /// <summary>
    /// Fetcher异常
    /// </summary>
    public class FetcherException : Exception
    {
        /// <summary>
        /// Fetcher异常
        /// </summary>
        /// <param name="message">Message</param>
        public FetcherException(string message) : base(message)
        { }
    }
}
