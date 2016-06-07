using System;
using System.Collections.Generic;

namespace MyCase.Classes
{
    public class Case
    {
        private string caseRegNumber;
        private DateTime caseRegDate;
        private string caseBasicInfo;
        private string sourceName;
        private int documentTypeValue;
        private bool isFinished;
        private DateTime workPeriodAlowed;
        List<Request> requestsMade;

        private string prosecutionCaseNumber;
        private DateTime prosecutionCaseDate;
        private string prosecutionOfficeName;

        public Case(string registrationNumber, DateTime registrationDate,
            string basicInformation, string caseSource, int documentTypeValue,
            DateTime workPeriod, string prosecutionRegNumber, DateTime prosecutionRegDate,
            string prosecutionOfficeName, bool isFinished = false)
        {
            CaseDocumentType = documentTypeValue;
            CaseRegistrationNumber = registrationNumber;
            CaseRegistrationDate = registrationDate;
            CaseBasicInformation = basicInformation;
            CaseSource = caseSource;
            FinishedState = isFinished;
            WorkPeriodAlowed = workPeriod;
            RequestsMade = new List<Request>();
            ProsecutionRegistrationNumber = prosecutionRegNumber;
            ProsecutionRegistrationDate = prosecutionRegDate;
            ProsecutionOfficeName = prosecutionOfficeName;
        }

        public string CaseRegistrationNumber
        {
            get
            {
                return caseRegNumber;
            }
            set
            {
                if (!DataValidator.ValidateRegNumberString(value, documentTypeValue))
                {
                    throw new Exceptions.CaseInvalidRegistrationNumberException(
                        string.Format("New registration number:\nInvalid Fromat.\nex.:{0}", 
                        (documentTypeValue == 0 || documentTypeValue == 3) ? 
                        "1234В-12345; 1234P-12345" : "123400-12345"));
                }
                caseRegNumber = value;
            }
        }

        public DateTime CaseRegistrationDate
        {
            get
            {
                return caseRegDate;
            }
            set
            {
                if (!DataValidator.ValidateRegNumberDate(value))
                {
                    throw new Exceptions.CaseClassException(
                        "Invalid new registration date:\n" +
                        "Date cannot be a future moment.");
                }
                caseRegDate = value;
            }
        }

        public string CaseBasicInformation
        {
            get
            {
                return caseBasicInfo;
            }
            set
            {
                if (!DataValidator.ValidateBasicInformationString(value))
                {
                    throw new Exceptions.CaseClassException("Invalid new basic information.\n" +
                        "Data restriction: 11-250 characters.");
                }
                caseBasicInfo = value;
            }
        }

        public string CaseSource
        {
            get
            {
                return sourceName;
            }
            set
            {
                if (!DataValidator.ValidateSourceName(value))
                {
                    throw new Exceptions.CaseClassException("Invalid new source name.\n" +
                        "Size: 3-25 characters.\n" +
                        "Only lat./cyr. letters,digits,! and -");
                }
                sourceName = value;
            }
        }

        public int CaseDocumentType
        {
            get
            {
                return documentTypeValue;
            }
            private set
            {
                var docTypeCount = Enum.GetNames(
                    typeof(Enumerations.DocumentTypeEnum)).Length;

                if (value >= docTypeCount || value < 0)
                {
                    string docIndexes =
                        "0: Сигнал от ДМОС\n" +
                        "1: Постановление на РП/следовател\n" +
                        "2: Прокурорско постановление\n" +
                        "3: Искане от ДМОС\n" +
                        "4: Преписка на предварителна проверка\n";
                    throw new Exceptions.CaseClassException(
                        string.Format("Wrong document type index.\n", docIndexes));
                }
                documentTypeValue = value;
            }
        }

        public bool FinishedState
        {
            get
            {
                return isFinished;
            }
            set
            {
                isFinished = value;
            }
        }

        public DateTime WorkPeriodAlowed
        {
            get
            {
                return workPeriodAlowed;
            }
            set
            {
                DateTime maxDayTime = DateTime.ParseExact(
                    "31.12.2100", "dd.MM.yyyy", 
                    System.Globalization.CultureInfo.InvariantCulture);
                DateTime minDayTime = DateTime.ParseExact(
                    "01.01.1900", "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture);

                if (value.Date < minDayTime || value.Date > maxDayTime)
                {
                    throw new Exceptions.CaseClassException(
                        "Invalid deadline value:\n" +
                        "Deadline can't be set\nto a past moment or\n" +
                        "after 31.12.2100.");
                }
                workPeriodAlowed = value;
            }
        }

        public List<Request> RequestsMade
        {
            get
            {
                return requestsMade;
            }
            set
            {
                requestsMade = value;
            }
        }

        public string ProsecutionRegistrationNumber
        {
            get
            {
                return prosecutionCaseNumber;
            }
            set
            {
                if (documentTypeValue == (int)Enumerations.DocumentTypeEnum.ProsecutionDecree)
                {
                    if (!DataValidator.ValidateProsecutionRegNumString(value))
                    {
                        throw new Exceptions.CaseClassException(
                            "Invalid prosecution number.\nOnly numbers in range:\n1-99999");
                    }

                    prosecutionCaseNumber = value;
                }
                else
                {
                    prosecutionCaseNumber = null;
                }
            }
        }

        public DateTime ProsecutionRegistrationDate
        {
            get
            {
                return prosecutionCaseDate;
            }
            set
            {
                if (!DataValidator.ValidateRegNumberDate(value))
                {
                    throw new Exceptions.CaseClassException("Prosecution registration date:\n" +
                        "Registration date cannot\nbe a future moment.");
                }
                prosecutionCaseDate = value;
            }
        }

        public string ProsecutionOfficeName
        {
            get
            {
                return prosecutionOfficeName;
            }
            set
            {
                if (documentTypeValue == (int)Enumerations.DocumentTypeEnum.ProsecutionDecree)
                {
                    if (!DataValidator.ValidationProsecutionOfficeName(value))
                    {
                        throw new Exceptions.CaseClassException("Invalid prosecution\noffice name.");
                    }
                    prosecutionOfficeName = DataModificator.TextCleaner(value);
                }
                else
                {
                    prosecutionOfficeName = null;
                }
            }
        }
    }
}