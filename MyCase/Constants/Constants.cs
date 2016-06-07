using System.Collections.Generic;
namespace MyCase.Constants
{
    public class Constants
    {
        //Validation constants
        public const int ALLOWED_WORKING_PERIOD = 20;

        public const string DEFFAULT_DATE = "1900-01-01";

        public static readonly string[] SOURCE_STRING_VALIDATION =
        {
            //Personal name in format I.Petrov - english and bulgarian
            @"^[A-Z]\.[A-Z][a-z]{3,12}$",
            @"^[А-Я]\.[А-Я][а-я]{3,12}$",
            //Firm name pattern
            @"^[A-Za-z0-9\s\-\!]+?[A-Za-z0-9]+$",
            @"^[А-Яа-я0-9\s\-\!]+?[А-Яа-я0-9]+$"
        };
        public const string DMOS_REGISTRATION_NUMBER_REGEX =
            @"^[1-9]\d{3}[BP]\-[1-9]\d{0,4}$";
        public const string BASIC_REGISTRATION_NUMBER_REGEX =
            @"^[1-9]\d{5}\-[1-9]\d{0,4}$";
        public const string BASIC_REQUEST_NUMBER_AND_DATE_REGEX =
            @"^[1-9]\d{3}(\d{2}|P)\-[1-9]\d{0,4}\/\d{2}$";
        public const string PROSECUTION_REG_NUMBER_REGEX =
            @"^[1-9]\d{0,4}$";
        public const string ProsecutionOfficeNameValidation =
            @"^([a-zA-ZА-Яа-я]{3,}\s?)+$";
        //Warning constants
        public const string INVALID_REG_NUMBER_FORMAT_WARNING =
            "Registration number: Invalid format.\n" +
            "  ex.: 1234В-1234; 123400-12345";
        public const string NO_DOCTYPE_SELECTED_WARNING =
            "Type: Select a document type from the menu.";
        public const string FUTURE_DATE_PICKED_WARNING =
            "Registration date: Cannot be a future moment.";
        public const string PROSECUTION_NUMBER_FORMAT_WARNING =
            "Prosecution number: Invalid  format. Ex.: 12345";
        public const string INVALID_SOURCE_NAME_WARNING =
            "Source name: Invalid or too short name.\n" +
            "  ex.:P.Petrov/И.Иванов/MY Firm Name-1990";
        public const string INVALID_BASIC_INFO_FORMAT_WARNING =
            "Basic Info: Invalid length or format.";
        public const string INVALID_PROSECUTION_OFFICE_WARNING =
            "Prosecution office: Invalid name format";
        //UPDATE Panel constants
        public static readonly object[] UPDATE_BASIC_DATAFIELDS_LIST = {
            "Промяна и добавяне на основна информация",
            "Корекция на регистрационен номер и дата",
            "Изваждане от или добавяне към отработени",
            "Промяна на предоставения срок за водене",
            "Промяна на името на подателя",
            "Добавяне на отправено искане",
            "Изстриване на отправено искане",
            "Промяна на статуса на получаване "
                + " на отговор по отправено искане"
        };

        public static readonly object[] UPDATE_PROSECUTION_DATAFIELDS_LIST = {
           "Промяна и добавяне на основна информация",
            "Корекция на регистрационен номер и дата",
            "Изваждане от или добавяне към отработени",
            "Промяна на предоставения срок за водене",
            "Промяна на името на подателя",
            "Добавяне на отправено искане",
            "Изстриване на отправено искане",
            "Промяна на статуса на получаване"
                + " на отговор по отправено искане",
            "Корекция на регистрационен номер и дата на прокурорска преписка",
            "Промяна на името на компетентната прокуратура"
        };
        //Database Constants
        public static readonly string DATABASE_CONNECTION_STRING = 
            string.Format(string.Format(@"Data Source = {0}\LocalDatabases\MyCaseDB.db; Version=3;",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)));
        public const string INSERT_NEW_CASE_QUERY =
            "INSERT INTO Cases (registration_number,registration_date,basic_info,allowed_work_period,source_name,is_finished,prosecution_number,prosecution_date,prosecution_office_name,document_type_enumeration)" +
            "VALUES (@RegNum, @RegDate, @BasicInfo, @WorkPeriod, @SourceName, @FinishState, @ProsecNum, @ProsecNumDate, @ProsecutionOffice, @DocType)";
        public const string STARTUP_CASES_TABLE_GRID_DISPLAY =
            "SELECT " +
            "registration_number AS 'Registration Number'," +
            "registration_date AS 'Registration Date'," +
            "source_name AS 'Sender'," +
            "allowed_work_period AS 'Deadline'," +
            "basic_info AS 'Basic Information'" +
            " FROM Cases " +
            "WHERE is_finished = 0;";
        public const string DEFAULT_REQUEST_TABLE_GRID_DISPLAY = 
            "SELECT " +
            "Cases.registration_number AS 'Case', " +
            "Requests.request_info AS 'Sent to', " +
            "Requests.registration_number AS 'Reg.Number' " +
            "FROM Requests " +
            "JOIN Cases ON Cases.id = Requests.case_id " +
            "WHERE Requests.answer_received = 0";

        public const string CHECK_NAME_AND_YEAR_QUERY =
            "SELECT registration_number, registration_date FROM Cases WHERE registration_number = @RegNumber";
        public const string DELETE_CASE_FROM_DATABASE =
            "DELETE FROM Cases WHERE (registration_number = @Number AND strftime('%Y', registration_date) = @Year)";
        public const string DEFAULT_REPORT_STATEMENT =
            "SELECT document_type_enumeration, is_finished FROM Cases ";
    }
}
