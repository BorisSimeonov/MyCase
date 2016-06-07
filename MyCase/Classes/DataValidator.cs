using System.Text.RegularExpressions;
using MyCase.Enumerations;
using System;
using MyCase.Exceptions;

namespace MyCase.Classes
{
    class DataValidator
    {
        public static bool ValidateRegNumberString(string regString, int type)
        {
            string pattern;
            if (type == (int)DocumentTypeEnum.DMOSSignal ||
                type == (int)DocumentTypeEnum.DMOSRequest)
            {
                pattern = Constants.Constants
                    .DMOS_REGISTRATION_NUMBER_REGEX;
            }
            else
            {
                if (type == (int)DocumentTypeEnum.PoliceCheck && Regex.IsMatch(regString, Constants.Constants
                    .DMOS_REGISTRATION_NUMBER_REGEX) == true)
                {
                    return true;
                }
                else
                {
                    pattern = Constants.Constants
                        .BASIC_REGISTRATION_NUMBER_REGEX;
                }
            }

            return Regex.IsMatch(regString, pattern) ? true : false;
        }
        public static bool ValidateProsecutionRegNumString(string regString)
        {
            return Regex.IsMatch(regString, Constants.Constants
                .PROSECUTION_REG_NUMBER_REGEX) ? true : false;
        }
        public static bool ValidateRegNumberDate(DateTime date)
        {
            return (date.Date > DateTime.Now.Date) ?
                false : true;
        }
        public static bool ValidateSourceName(string sourceName)
        {
            bool isValid = false;
            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return false;
            }
            else if (sourceName.Length <= 25 && sourceName.Length >= 3)
            {
                foreach (string pattern in Constants.Constants.SOURCE_STRING_VALIDATION)
                {
                    isValid = (isValid || Regex.IsMatch(
                        sourceName, pattern));
                    if (isValid)
                    {
                        return true;
                    }
                }
            }

            return isValid;
        }
        public static bool ValidateBasicInformationString(string infoText)
        {
            if (string.IsNullOrWhiteSpace(infoText)) return false;
            infoText = DataModificator.TextCleaner(infoText);
            return (infoText.Length > 10 && infoText.Length <= 250) ?
                true : false;
        }
        public static bool ValidationProsecutionOfficeName(string officeName)
        {
            if (string.IsNullOrWhiteSpace(officeName)) { return false; }
            officeName = DataModificator.TextCleaner(officeName);
            if (officeName.Length < 3 && officeName.Length < 35) { return false; }
            return (Regex.IsMatch(officeName, Constants.Constants
                .ProsecutionOfficeNameValidation)) ? true : false;
        }
        public static bool ValidateSearchRegistrationNumber(string number)
        {
            bool validNumber = false;
            if (
                Regex.IsMatch(number, Constants.Constants.DMOS_REGISTRATION_NUMBER_REGEX) ||
                Regex.IsMatch(number, Constants.Constants.BASIC_REGISTRATION_NUMBER_REGEX)
                )
            {
                validNumber = true;
            }

            return validNumber;
        }
        public static bool ValidateOfficerNameForReport(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ReportArgumentFormatException("Name cannot be empty.");
            }
            else
            {
                string matchPattern = @"^(\s?[A-Z][a-z]{2,8}){1,3}$";
                if (!Regex.IsMatch(name, matchPattern))
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
    }
}
