using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCase.Exceptions
{
    class ReportArgumentFormatException : Exception
    {
        public ReportArgumentFormatException()
        {
        }

        public ReportArgumentFormatException(string message)
            : base(message)
        {
        }

        public ReportArgumentFormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
