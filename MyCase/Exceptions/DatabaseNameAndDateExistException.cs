using System;

namespace MyCase.Exceptions
{
    public class DatabaseNameAndDateExistException : Exception
    {
        public DatabaseNameAndDateExistException()
        {
        }

        public DatabaseNameAndDateExistException(string message)
            : base(message)
        {
        }

        public DatabaseNameAndDateExistException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
