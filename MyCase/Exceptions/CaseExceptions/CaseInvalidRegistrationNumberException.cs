using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyCase.Exceptions
{
    class CaseInvalidRegistrationNumberException : CaseClassException
    {
        public CaseInvalidRegistrationNumberException()
        {
        }

        public CaseInvalidRegistrationNumberException(string message) : base(message)
        {
        }

        public CaseInvalidRegistrationNumberException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
