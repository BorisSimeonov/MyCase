using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCase.Exceptions
{
    class ReportValueOutOfRangeException : Exception
    {
        public ReportValueOutOfRangeException()
        {
        }

        public ReportValueOutOfRangeException(string message)
            : base(message)
        {
        }

        public ReportValueOutOfRangeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
