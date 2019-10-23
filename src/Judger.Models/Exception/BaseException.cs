using System;

namespace Judger.Models.Exception
{
    public class BaseException : ApplicationException
    {
        protected BaseException(string message) : base(message)
        { }
    }
}