using MyCase.Exceptions;
using System.Text.RegularExpressions;

namespace MyCase.Classes
{
    public class Report
    {
        private readonly int MAX_REPORT_COUNT = int.MaxValue;

        private string operativeName = null;
        private string searchPeriod = "All";
        private int prosecutionDecreeCount = 0;
        private int policeCheckCount = 0;
        private int investigationOfficerDecreeCount = 0;
        private int signalFromDMOSCount = 0;
        private int requestFromDMOSCount = 0;

        public Report()
        {

        }

        public Report(string operativeName, string searchPeriod = "All")
        {
            OperativeName = operativeName;
            SearchPeriod = searchPeriod;
        }

        public string SearchPeriod
        {
            get
            {
                return searchPeriod;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ReportArgumentFormatException(
                        "Search period cannot be empty.");
                }
                else
                {
                    if (value == "All")
                    {
                        searchPeriod = value;
                    }
                    else
                    {
                        string pattern = @"^\d{2}\.\d{2}\.\d{4}\-\d{2}\.\d{2}\.\d{4}$";
                        if (!Regex.IsMatch(value, pattern))
                        {
                            throw new ReportArgumentFormatException("Invalid period string format.");
                        }
                        searchPeriod = value;
                    }
                }
            }
        }
        public int ProsecutionDecreeCount
        {
            get
            {
                return prosecutionDecreeCount;
            }
            set
            {
                if (!CountValidator(value))
                {
                    throw new ReportValueOutOfRangeException(
                        "Negative or to big prosecution decree count value.");
                }
                prosecutionDecreeCount = value;
            }
        }
        public int PoliceCheckCount
        {
            get
            {
                return policeCheckCount;
            }
            set
            {
                if (!CountValidator(value))
                {
                    throw new ReportValueOutOfRangeException(
                        "Negative or to big prosecution decree count value.");
                }
                policeCheckCount = value;
            }
        }
        public int InvestigationOfficerDecreeCount
        {
            get
            {
                return investigationOfficerDecreeCount;
            }
            set
            {
                if (!CountValidator(value))
                {
                    throw new ReportValueOutOfRangeException(
                        "Negative or to big prosecution decree count value.");
                }
                investigationOfficerDecreeCount = value;
            }
        }
        public int SignalFromDMOSCount
        {
            get
            {
                return signalFromDMOSCount;
            }
            set
            {
                if (!CountValidator(value))
                {
                    throw new ReportValueOutOfRangeException(
                        "Negative or to big prosecution decree count value.");
                }
                signalFromDMOSCount = value;
            }
        }
        public int RequestFromDMOSCount
        {
            get
            {
                return requestFromDMOSCount;
            }
            set
            {
                if (!CountValidator(value))
                {
                    throw new ReportValueOutOfRangeException(
                        "Negative or to big prosecution decree count value.");
                }
                requestFromDMOSCount = value;
            }
        }
        public string OperativeName
        {
            get
            {
                return operativeName;
            }
            set
            {
                if (NameValidator(value.Trim()))
                {
                    operativeName = value.Trim();
                }
            }
        }
        private bool NameValidator(string value)
        {
            string matchPattern = @"^(\s?[A-Z][a-z]{2,8}){1,3}$";
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ReportArgumentFormatException(
                    "Name cannot be empty.");
            }
            else
            {
                if (!Regex.IsMatch(value, matchPattern))
                {
                    throw new ReportArgumentFormatException(
                        "Invalid name fromat.");
                }
                else
                {
                    return true;
                }
            }
        }
        private bool CountValidator(int value)
        {
            bool isValid = false;
            if (value >= 0 &&
                value <= MAX_REPORT_COUNT)
            {
                isValid = true;
            }
            return isValid;
        }
    }
}
