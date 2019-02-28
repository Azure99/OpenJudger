﻿using System;

namespace Judger.Entity
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