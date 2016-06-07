using System;

namespace MyCase.Exceptions
{
    public class CaseClassException : Exception
    {
        public CaseClassException()
        {
        }

        public CaseClassException(string message)
            : base(message)
        {
        }

        public CaseClassException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
