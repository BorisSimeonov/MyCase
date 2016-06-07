using System;

namespace MyCase.Exceptions
{
    class ZeroOrTooManyResultsFoundException : Exception
    {
        public ZeroOrTooManyResultsFoundException()
        {

        }

        public ZeroOrTooManyResultsFoundException(string message)
            : base(message)
        {

        }

        public ZeroOrTooManyResultsFoundException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
