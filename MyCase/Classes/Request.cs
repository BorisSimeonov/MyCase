using System;
using System.Text.RegularExpressions;

namespace MyCase.Classes
{
    public class Request
    {
        private string requestNumber;
        private string requestInformation;
        private bool answerRecieved;

        public Request(string requestNumber, string requestInformation,
            bool answerIsResieved = false)
        {
            RequestNumber = requestNumber;
            RequestInformation = requestInformation;
            AnswerRecieved = answerIsResieved;
        }

        public string RequestNumber
        {
            get
            {
                return requestNumber;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exceptions.CaseClassException("Request number cannot\nbe empty string.");
                }
                else
                {
                    if (!Regex.IsMatch(value, Constants.Constants.BASIC_REQUEST_NUMBER_AND_DATE_REGEX))
                    {
                        throw new Exceptions.CaseClassException("Invalid argument format.\nAlowed format: \n123456-12345/12");
                    }
                    requestNumber = value;
                }
            }
        }
        public string RequestInformation {
            get
            {
                return requestInformation;
            }
            private set
            {
                if (value.Trim().Length > 25
                    || value.Trim().Length < 5)
                {
                    throw new Exceptions.CaseClassException("Request information must be\nin range 5-25 sqmbols.");
                }
                requestInformation = value.Trim();
            }
        }
        public bool AnswerRecieved
        {
            get
            {
                return answerRecieved;
            }
            set
            {
                answerRecieved = value;
            }
        }
    }
}
