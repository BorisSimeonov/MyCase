using System;
using MyCase.Interfaces;
using MyCase.Classes;
using System.Data.SQLite;
using MyCase.Exceptions;
using System.Windows.Forms;
using System.Data;

namespace MyCase.DataFlow
{
    public class SQLiteDriver : IDataManipulator
    {
        public SQLiteDriver()
        {

        }

        public int ChangeRequestAnswerRecievedStatus(string requestRegistrationNumber)
        {
            int requestsChanged = 0;
            using (SQLiteConnection connection = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "UPDATE Requests SET answer_received = CASE WHEN answer_received = 0 THEN 1 ELSE 0 END WHERE registration_number = @RegistrationNumber";
                    command.Parameters.AddWithValue("@RegistrationNumber", requestRegistrationNumber);
                    connection.Open();
                    requestsChanged = command.ExecuteNonQuery();
                }
            }
            return requestsChanged;
        }

        public bool CheckIfRegistrationNumberAndYearExist(string number, DateTime date, bool isForModification)
        {
            bool hasMatch = false;
            using (SQLiteConnection connection = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand command = new SQLiteCommand(
                    Constants.Constants.CHECK_NAME_AND_YEAR_QUERY, connection))
                {
                    command.Parameters.AddWithValue("@RegNumber", number);
                    connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (Convert.ToDateTime(reader["registration_date"].ToString()).Year == date.Year)
                                {
                                    if (!isForModification ||
                                        (isForModification && reader["registration_number"]
                                        .ToString() == number))
                                    {
                                        hasMatch = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return hasMatch;
        }

        public int DeleteAllRequestsOnCase(string caseId)
        {
            using (SQLiteConnection connection = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM Requests WHERE " +
                        "case_id = @ID;";
                    command.Parameters.AddWithValue("@ID",
                        caseId);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
        }

        public int DeleteCaseAndRelatedRequestsFromDatabase(string registrationNumber, DateTime registrationDate)
        {
            string caseID = ExtractCaseID(registrationNumber, registrationDate);
            DeleteAllRequestsOnCase(caseID);
            int rowsAffected = 0;

            using (SQLiteConnection connection = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = Constants.Constants.DELETE_CASE_FROM_DATABASE;
                    command.Parameters.AddWithValue("@Number", registrationNumber);
                    command.Parameters.AddWithValue("@Year", registrationDate.Year.ToString());
                    command.Parameters.AddWithValue("@CaseID", caseID);
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
            }
            return rowsAffected;
        }

        public bool DeleteRequestByRegistrationNumber(string registrationNumber)
        {
            using (SQLiteConnection connection = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = "DELETE FROM Requests WHERE " +
                        "registration_number = @registrationNumber;";
                    command.Parameters.AddWithValue("@registrationNumber",
                        registrationNumber);
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Case ExtractCase(string regNumber, DateTime regDate)
        {
            Case extractedCase = null;
            using (SQLiteConnection connection = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                string query = "SELECT * FROM Cases WHERE (registration_number = " +
                    "@Number AND strftime('%Y', registration_date) = @Year)";

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Number", regNumber.ToString());
                    command.Parameters.AddWithValue("@Year", regDate.Year.ToString());
                    int resultCounter = 0;
                    connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (resultCounter == 0)
                                {
                                    extractedCase = extractFromReader(reader, true);
                                }
                                resultCounter++;
                            }
                        }
                        else
                        {
                            throw new ZeroOrTooManyResultsFoundException("No rezults found.");
                        }

                        if (resultCounter > 1)
                        {
                            throw new ZeroOrTooManyResultsFoundException(
                               string.Format("{0} results found.\nPlease delete duplicated cases.", resultCounter));
                        }
                    }
                }
            }
            return extractedCase;
        }

        public string ExtractCaseID(string regNumber, DateTime regDate)
        {
            string idHolder = null;
            using (SQLiteConnection conn = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "SELECT id FROM Cases " +
                        "WHERE (registration_number = @Number " +
                        "AND strftime('%Y', registration_date) = @Year)";
                    cmd.Parameters.AddWithValue("@Number", regNumber);
                    cmd.Parameters.AddWithValue("Year", regDate.Year.ToString());
                    conn.Open();
                    using (SQLiteDataReader reader =
                        cmd.ExecuteReader())
                    {
                        int resultCounter = 0;
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (resultCounter == 0)
                                {
                                    idHolder = reader["id"].ToString();
                                }
                                resultCounter++;
                            }
                        }
                        else
                        {
                            throw new ZeroOrTooManyResultsFoundException("No rezults found.");
                        }

                        if (resultCounter > 1)
                        {
                            throw new ZeroOrTooManyResultsFoundException(
                               string.Format("{0} results found.\nPlease delete duplicated cases.", resultCounter));
                        }
                    }
                }
            }
            return idHolder;
        }

        public Case extractFromReader(SQLiteDataReader reader, bool extractForChange)
        {
            Case bufferCase = null;
            bufferCase = new Case(
                reader["registration_number"].ToString(),
                Convert.ToDateTime(reader["registration_date"]),
                reader["basic_info"].ToString(),
                reader["source_name"].ToString(),
                Convert.ToInt32(reader["document_type_enumeration"]),
                (extractForChange) ? DateTime.Now.Date :
                Convert.ToDateTime(reader["allowed_work_period"]),
                reader["prosecution_number"].ToString(),
                Convert.ToDateTime(reader["prosecution_date"]),
                reader["prosecution_office_name"].ToString(),
                (reader["is_finished"].ToString() == "1")
                ? true : false
                );
            return bufferCase;
        }

        public void GetDataToGrid(string SQLiteCommand, DataGridView dataGrid)
        {
            using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(SQLiteCommand,
                    Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommandBuilder commandBuilder = new SQLiteCommandBuilder(adapter))
                {
                    DataTable table = new DataTable();
                    table.Locale = System.Globalization.CultureInfo.InvariantCulture;
                    adapter.Fill(table);
                    BindingSource bindSource = new BindingSource();
                    bindSource.DataSource = table;
                    dataGrid.DataSource = bindSource;
                    adapter.Update(table);
                }

            }
        }

        public string InsertToDatabase(Case myCase)
        {
            string connString = Constants.Constants.DATABASE_CONNECTION_STRING;
            string query = Constants.Constants.INSERT_NEW_CASE_QUERY;
            //Check if the combination of the registration number and 
            //registration year exist in the database
            if (CheckIfRegistrationNumberAndYearExist(myCase.CaseRegistrationNumber,
                myCase.CaseRegistrationDate, false))
            {
                throw new Exceptions.DatabaseNameAndDateExistException(
                    "Cannot insert data. Same registration number and year already exist.");
            }

            //Upload if the combination of registration number and
            //registration year is not found in the database
            using (SQLiteConnection connection = new SQLiteConnection(connString))
            {
                int rowsAffected;
                connection.Open();
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RegNum", myCase.CaseRegistrationNumber);
                    command.Parameters.AddWithValue("@RegDate", myCase.CaseRegistrationDate);
                    command.Parameters.AddWithValue("@BasicInfo", myCase.CaseBasicInformation);
                    command.Parameters.AddWithValue("@WorkPeriod", myCase.WorkPeriodAlowed);
                    command.Parameters.AddWithValue("@SourceName", myCase.CaseSource);
                    command.Parameters.AddWithValue("@FinishState", myCase.FinishedState ? 1 : 0);
                    command.Parameters.AddWithValue("@ProsecNum", myCase.ProsecutionRegistrationNumber);
                    command.Parameters.AddWithValue("@ProsecNumDate", myCase.ProsecutionRegistrationDate);
                    command.Parameters.AddWithValue("@ProsecutionOffice", myCase.ProsecutionOfficeName);
                    command.Parameters.AddWithValue("@DocType", myCase.CaseDocumentType);
                    rowsAffected = command.ExecuteNonQuery();
                }
                return string.Format("\n\n{0} rows successfully uploaded.",
                    rowsAffected);
            }
        }

        public Tuple<Report, Report> PopulateReport(string statement, string period)
        {
            Report activeCasesReport = new Report();
            Report finishedCasesReport = new Report();
            using (SQLiteConnection connection = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = statement;
                    connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            int bufferValue;
                            while (reader.Read())
                            {
                                bufferValue = int.Parse(reader["document_type_enumeration"]
                                    .ToString());

                                switch (bufferValue)
                                {
                                    #region switch body
                                    case 0:
                                        if (reader["is_finished"].ToString() == "0")
                                        {
                                            activeCasesReport.SignalFromDMOSCount += 1;
                                        }
                                        else
                                        {
                                            finishedCasesReport.SignalFromDMOSCount += 1;
                                        }
                                        
                                        break;
                                    case 1:
                                        if (reader["is_finished"].ToString() == "0")
                                        {
                                            activeCasesReport.InvestigationOfficerDecreeCount += 1;
                                        }
                                        else
                                        {
                                            finishedCasesReport.InvestigationOfficerDecreeCount += 1;
                                        }
                                        break;
                                    case 2:
                                        if (reader["is_finished"].ToString() == "0")
                                        {
                                            activeCasesReport.ProsecutionDecreeCount += 1;
                                        }
                                        else
                                        {
                                            finishedCasesReport.ProsecutionDecreeCount += 1;
                                        }
                                        break;
                                    case 3:
                                        if (reader["is_finished"].ToString() == "0")
                                        {
                                            activeCasesReport.RequestFromDMOSCount += 1;
                                        }
                                        else
                                        {
                                            finishedCasesReport.RequestFromDMOSCount += 1;
                                        }
                                        break;
                                    case 4:
                                        if (reader["is_finished"].ToString() == "0")
                                        {
                                            activeCasesReport.PoliceCheckCount += 1;
                                        }
                                        else
                                        {
                                            finishedCasesReport.PoliceCheckCount += 1;
                                        }
                                        break;
                                        #endregion 
                                }
                            }
                        }
                    }
                    connection.Close();
                }
            }

            if (period != null)
            {
                activeCasesReport.SearchPeriod = period;
                finishedCasesReport.SearchPeriod = period;
            }

            return Tuple.Create(activeCasesReport, finishedCasesReport);
        }

        public bool RequestRegistrationNumberExist(string registrationNumber)
        {
            bool exist = false;
            using (SQLiteConnection conn = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM Requests WHERE registration_number = @Num;";
                    cmd.Parameters.AddWithValue("@Num", registrationNumber);
                    conn.Open();
                    int countHolder = Convert.ToInt32(cmd.ExecuteScalar());
                    if (countHolder > 0)
                    {
                        exist = true;
                    }
                }
            }
            return exist;
        }

        public int UpdateModifiedCase(Case modCase, string setPartQuery, string oldCaseRegNumber, DateTime oldCaseRegDate)
        {
            int rowsAffected = 0;
            string query = string.Format("UPDATE Cases SET {0}" +
            " WHERE (registration_number = @OldRegNum AND registration_date = @OldRegDate)",
            setPartQuery);

            using (SQLiteConnection conn = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand command = new SQLiteCommand(
                    query, conn))
                {
                    command.Parameters.AddWithValue("@OldRegNum", oldCaseRegNumber);
                    command.Parameters.AddWithValue("@OldRegDate", oldCaseRegDate.Date);
                    conn.Open();
                    rowsAffected = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            return rowsAffected;
        }

        public int UploadRequestToDatabase(Request newRequest, string caseID)
        {
            int rowsAffected = 0;
            using (SQLiteConnection conn = new SQLiteConnection(
                Constants.Constants.DATABASE_CONNECTION_STRING))
            {
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "INSERT INTO Requests (registration_number,request_info,answer_received,case_id) " +
                        "VALUES (@Number, @Information, @isResieved, @CaseID)";
                    cmd.Parameters.AddWithValue("@Number", newRequest.RequestNumber);
                    cmd.Parameters.AddWithValue("@Information", newRequest.RequestInformation);
                    cmd.Parameters.AddWithValue("@isResieved", ((newRequest.AnswerRecieved) ?
                        1 : 0).ToString());
                    cmd.Parameters.AddWithValue("@CaseID", caseID);
                    conn.Open();
                    rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            if (rowsAffected < 1)
            {
                throw new CaseClassException("Operation failed.\n{0} items inserted.");
            }

            return rowsAffected;
        }
    }
}
