using System;
using MyCase.Classes;
using System.Data.SQLite;
using System.Windows.Forms;

namespace MyCase.Interfaces
{
    interface IDataManipulator
    {
        //From case extract
        Case ExtractCase(string regNumber, DateTime regDate);

        Case extractFromReader(SQLiteDataReader reader, bool extractForChange);

        string ExtractCaseID(string regNumber, DateTime regDate);
        
        string InsertToDatabase(Case myCase);

        int UpdateModifiedCase(Case modCase, string setPartQuery,
            string oldCaseRegNumber, DateTime oldCaseRegDate);

        bool CheckIfRegistrationNumberAndYearExist(
            string number, DateTime date, bool isForModification);

        //request part
        bool RequestRegistrationNumberExist(string registrationNumber);

        int UploadRequestToDatabase(Request newRequest, string caseID);

        bool DeleteRequestByRegistrationNumber(string registrationNumber);

        int DeleteAllRequestsOnCase(string caseId);

        int ChangeRequestAnswerRecievedStatus(string requestRegistrationNumber);
        //Delete case

        int DeleteCaseAndRelatedRequestsFromDatabase(string registrationNumber, DateTime registrationDate);

        //Display to gridView

        void GetDataToGrid(string SQLiteCommand,
            DataGridView dataGrid);

        //Reports

        Tuple<Report, Report> PopulateReport(string statement, string period);
    }
}
