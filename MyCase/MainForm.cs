using MyCase.Classes;
using MyCase.Enumerations;
using MyCase.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MyCase.Interfaces;
using System.Windows.Forms;
using System.Drawing;
using System.Data.SQLite;

namespace MyCase
{
    public partial class MainForm : Form
    {
        private Control selectedPanel;

        private IDataManipulator dataDriver;

        private IReportEngine reportDriver;

        private readonly Color errorColor = Color.Red;

        private readonly Color successColor = Color.Green;

        private Tuple<string, string> reportStatemet = 
            new Tuple<string, string>(Constants.Constants
                .DEFAULT_REPORT_STATEMENT, null);

        public MainForm()
        {
            InitializeComponent();
            selectedPanel = pnlInsert;
            dataDriver = new DataFlow.SQLiteDriver();
            reportDriver = new DataFlow.XMLReportDriver();

            GetDataToGrid(Constants.Constants.STARTUP_CASES_TABLE_GRID_DISPLAY,
                dgvCaseDisplay);
            GetDataToGrid(Constants.Constants.DEFAULT_REQUEST_TABLE_GRID_DISPLAY,
                dgvRequestsDisplay);
        }

        #region Insert Panel Methods
        private void btnInsert_Click(object sender, EventArgs e)
        {
            var registrationErrors = new List<string>[]
            {   new List<string>(),
                new List<string>()
            };
            //Validate the registration number of the document 
            //before database insertion
            Label[] labelArray = { lblInsertValidationErrorsReg,
                lblInsertValidationErrorsProsec };
            foreach (Label l in labelArray) { l.Text = ""; }
            pnlInsertThumbUp.Visible = false;
            //Check if there is a valid selection choice;
            bool hasSelectedDocument = (cbTypePicker.SelectedIndex > cbTypePicker.Items.Count - 1 ||
                cbTypePicker.SelectedIndex < 0) ? false : true;

            if (!hasSelectedDocument)
            {
                registrationErrors[0].Add(
                    Constants.Constants.NO_DOCTYPE_SELECTED_WARNING);
            }
            else
            {
                //Validating the registration number textbox
                if (!DataValidator.ValidateRegNumberString(
                tbInsertRegNumber.Text, cbTypePicker.SelectedIndex))
                {
                    registrationErrors[0].Add(
                    Constants.Constants.INVALID_REG_NUMBER_FORMAT_WARNING);
                }
                //Validating the registration number date picker
                if (!DataValidator.ValidateRegNumberDate(
                    dtpInsertRegDate.Value))
                {
                    registrationErrors[0].Add(
                        Constants.Constants.FUTURE_DATE_PICKED_WARNING);
                }
                //Validating the source name
                if (!DataValidator.ValidateSourceName(
                    tbInsertSourceName.Text))
                {
                    registrationErrors[1].Add(
                        Constants.Constants.INVALID_SOURCE_NAME_WARNING);
                }
                //Validting Basic Info text field. Must be longer than 10
                //characters and shorter than 251.
                if (!DataValidator.ValidateBasicInformationString(
                    tbInsertBasicInfo.Text))
                {
                    registrationErrors[0].Add(
                        Constants.Constants.INVALID_BASIC_INFO_FORMAT_WARNING);
                }
                //Check if the documet type is prosecutor's decree and 
                //if it is validate the prosecutor's data fields
                if (cbTypePicker.SelectedIndex ==
                    (int)DocumentTypeEnum.ProsecutionDecree)
                {
                    //Validating prosecution's registration number
                    if (!DataValidator.ValidateProsecutionRegNumString(
                        tbInsertProsecutionRegNum.Text))
                    {
                        registrationErrors[1].Add(
                            Constants.Constants.PROSECUTION_NUMBER_FORMAT_WARNING);
                    }
                    //Validating prosecution's registration data
                    if (!DataValidator.ValidateRegNumberDate(
                    dtpInsertProsecRegDate.Value))
                    {

                        registrationErrors[1].Add(
                            Constants.Constants.FUTURE_DATE_PICKED_WARNING);
                    }
                    //Validating the prosecutions office name
                    if (!DataValidator.ValidationProsecutionOfficeName(
                        tbInsertProsecutionOffice.Text))
                    {

                        registrationErrors[1].Add(
                            Constants.Constants.INVALID_PROSECUTION_OFFICE_WARNING);
                    }
                }
            }

            //Display the errors in two labels under each textbox group
            bool allFieldsAreValid = true;
            for (int idx = 0; idx < registrationErrors.Length; idx++)
            {
                if (registrationErrors[idx].Count > 0)
                {
                    allFieldsAreValid = false;
                    labelArray[idx].ForeColor = errorColor;
                    registrationErrors[idx].Select(s => { s = "- " + s; return s; }).ToList();
                    foreach (string s in registrationErrors[idx])
                    {
                        labelArray[idx].Text += "- " + s + Environment.NewLine;
                    }
                }
            }
            //Importing data to the database if all data is valid
            if (allFieldsAreValid)
            {
                string prosecOffice = tbInsertProsecutionOffice.Text;
                DateTime prosecRegDate = dtpInsertProsecRegDate.Value;
                string prosRegNum = tbInsertProsecutionRegNum.Text;
                string result;
                try
                {
                    Case newCase = CaseBuilder();
                    result = dataDriver.InsertToDatabase(newCase);
                    pnlInsertThumbUp.Visible = true;
                    //TODO: label color and text method
                    LabelTextColorChanger(lblInsertValidationErrorsProsec, successColor, result);
                    btnInsertNew.Enabled = true;
                    btnInsert.Enabled = false;
                    GetDataToGrid(Constants.Constants.STARTUP_CASES_TABLE_GRID_DISPLAY,
                    dgvCaseDisplay);
                }
                catch (Exception ex) when (ex is SQLiteException || ex is DatabaseNameAndDateExistException)
                {
                    LabelTextColorChanger(lblInsertValidationErrorsReg, errorColor, ex.Message);
                }
            }
        }

        private void btnInsertNew_RestartPanelControls(object sender, EventArgs e)
        {
            RestartAllControlsInPanel(pnlInsert);
        }

        private Case CaseBuilder()
        {
            string registrationNumber = tbInsertRegNumber.Text;
            DateTime registrationDate = dtpInsertRegDate.Value.Date;
            string basicInfo = tbInsertBasicInfo.Text;
            string source = tbInsertSourceName.Text;
            int docTypeValue = cbTypePicker.SelectedIndex;
            string prosecutionregistration_number = "";
            string prosecutionOffice = "";
            DateTime prosecutionRegistrationDate = DateTime.Parse(
                Constants.Constants.DEFFAULT_DATE);
            DateTime allowedWorkPeriod = registrationDate.AddDays(
                Constants.Constants.ALLOWED_WORKING_PERIOD);

            if (docTypeValue == (int)DocumentTypeEnum.ProsecutionDecree)
            {
                prosecutionregistration_number = tbInsertProsecutionRegNum.Text;
                prosecutionOffice = tbInsertProsecutionOffice.Text;
                prosecutionRegistrationDate = dtpInsertProsecRegDate.Value.Date;
            }

            Case bufferCase = new Case(
                registrationNumber, registrationDate, basicInfo, source,
                docTypeValue, allowedWorkPeriod, prosecutionregistration_number,
                prosecutionRegistrationDate, prosecutionOffice
                );

            return bufferCase;
        }

        private void cbTypePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool isProsecutorsDecree = (cbTypePicker.SelectedIndex ==
                (int)DocumentTypeEnum.ProsecutionDecree);

            tbInsertProsecutionRegNum.Enabled = isProsecutorsDecree;
            tbInsertProsecutionOffice.Enabled = isProsecutorsDecree;
            dtpInsertProsecRegDate.Enabled = isProsecutorsDecree;
        }
        #endregion

        #region UpdatePanel Methods
        private KeyValuePair<Case, string> BuildQueryAndChangeExtractedCase(
            Case extractedCase, int checkedIndex, string senderName)
        {
            string queryBuffer = null;
            switch (checkedIndex)
            {
                #region Class cases without requests
                case 0:
                    extractedCase.CaseBasicInformation = (senderName == btnUpdateAddData.Name) ?
                string.Format("{0}; {1}", extractedCase.CaseBasicInformation, tbUpdatePrimary.Text)
                : tbUpdatePrimary.Text;
                    queryBuffer = string.Format("basic_info = '{0}'",
                        extractedCase.CaseBasicInformation);
                    break;
                case 1:
                    extractedCase.CaseRegistrationNumber = tbUpdatePrimary.Text;
                    extractedCase.CaseRegistrationDate = dtpUpdateChangeDate.Value;
                    queryBuffer = string.Format(
                        "registration_number = '{0}',registration_date = '{1}'",
                        extractedCase.CaseRegistrationNumber, extractedCase.CaseRegistrationDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case 2:
                    if (extractedCase.FinishedState == true)
                    {
                        extractedCase.FinishedState = false;
                    }
                    else
                    {
                        extractedCase.FinishedState = true;
                    }

                    queryBuffer = string.Format("is_finished = '{0}'",
                        (extractedCase.FinishedState) ? 1 : 0);
                    break;
                case 3:
                    extractedCase.WorkPeriodAlowed =
                        dtpUpdateChangeDate.Value.Date;
                    queryBuffer = string.Format("allowed_work_period = '{0}'",
                        extractedCase.WorkPeriodAlowed.Date.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case 4:
                    extractedCase.CaseSource = tbUpdatePrimary.Text;
                    queryBuffer = string.Format("source_name = '{0}'",
                        extractedCase.CaseSource);
                    break;
                case 8:
                    extractedCase.ProsecutionRegistrationNumber = tbUpdatePrimary.Text;
                    extractedCase.ProsecutionRegistrationDate = dtpUpdateChangeDate.Value.Date;
                    queryBuffer = string.Format("prosecution_number = '{0}', prosecution_date = '{1}'",
                        extractedCase.ProsecutionRegistrationNumber,
                        extractedCase.ProsecutionRegistrationDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    break;
                case 9:
                    extractedCase.ProsecutionOfficeName = tbUpdatePrimary.Text;
                    queryBuffer = string.Format("prosecution_office_name = '{0}'",
                    extractedCase.ProsecutionOfficeName);
                    break;
                    #endregion
            }

            if (queryBuffer == null)
            {
                throw new FormatException(
                    string.Format("Query building failed. Variable queryBuffer == {1}",
                    queryBuffer.ToString()));
            }

            KeyValuePair<Case, string> bufferPair =
                new KeyValuePair<Case, string>(
                    extractedCase, queryBuffer);

            return bufferPair;
        }

        private void UpdateDatabaseData(object sender, EventArgs e)
        {
            //Gets the value of the sender control in order to determine
            //if the user wants to add or replace the stored data
            string senderName = ((Button)sender).Name;
            string caseRegistrationNumber = tbUpdateRegNumInput.Text;
            DateTime caseRegistrationDate = dtpUpdateRegNumDate.Value;
            //Check if the record exists and extract it
            //in a Case class instance. Throw custom exception if
            //no results are found
            Case extractedCase = null;
            int checkedIndex = cbUpdateFieldSelect.SelectedIndex;

            try
            {
                if (checkedIndex >= 5 &&
                checkedIndex <= 7)
                {
                    #region Request add and change
                    string requestNumber = tbUpdatePrimary.Text;
                    switch (checkedIndex)
                    {
                        case 5:
                            string requestInfo = tbUpdateSecondary.Text;
                            string relatedCaseID = dataDriver.ExtractCaseID(
                                caseRegistrationNumber, caseRegistrationDate);

                            if (!dataDriver.RequestRegistrationNumberExist(requestNumber))
                            {
                                Request newRequest = new Request(requestNumber, requestInfo);
                                int rowsInserted = dataDriver.UploadRequestToDatabase(
                                     newRequest, relatedCaseID);

                                ClearAndDiseableUpdatePanelControls();
                                LabelTextColorChanger(lblUpdateErrors, successColor,
                                    string.Format("Success.\n{0} roll inserted.", rowsInserted));
                            }
                            else
                            {
                                throw new CaseClassException("Request number exist.");
                            }
                            break;
                        case 6:
                            if (dataDriver.RequestRegistrationNumberExist(requestNumber))
                            {
                                if (dataDriver.DeleteRequestByRegistrationNumber(requestNumber))
                                {
                                    LabelTextColorChanger(lblUpdateErrors, successColor,
                                    string.Format("Success.\n{0} roll inserted.", "Success.\nRequest deleted."));
                                }
                                else
                                {
                                    throw new CaseClassException("Delete failed.");
                                }
                            }
                            else
                            {
                                throw new CaseClassException("Request does not exist.");
                            }
                            break;
                        case 7:
                            string requestRegistrationNumber = tbUpdatePrimary.Text;
                            int rowsAffected = dataDriver.ChangeRequestAnswerRecievedStatus(
                                requestRegistrationNumber);
                            if (rowsAffected > 0)
                            {
                                LabelTextColorChanger(lblUpdateErrors, successColor,
                                    string.Format("Success.\n{0} Request/s updated.",
                                    rowsAffected));
                            }
                            else
                            {
                                LabelTextColorChanger(lblUpdateErrors, errorColor,
                                    string.Format("Fail.\nNo requests found for\n{0}.",
                                    requestRegistrationNumber));
                            }
                            break;
                    }
                    GetDataToGrid(Constants.Constants.DEFAULT_REQUEST_TABLE_GRID_DISPLAY,
                            dgvRequestsDisplay);
                    #endregion
                }
                else
                {
                    #region Update Case Data - without Requests data
                    extractedCase = dataDriver.ExtractCase(
                        caseRegistrationNumber, caseRegistrationDate);
                    if (extractedCase != null)
                    {
                        //Getting the integer value of the selected property index
                        //that need to be changed and change the data in the extracted case

                        if (extractedCase.CaseDocumentType !=
                            cbUpdateTypePicker.SelectedIndex)
                        {
                            throw new CaseClassException(
                                "Matched case type is different\n" +
                                "than the document selection.");
                        }

                        string oldRegistrationNumber = extractedCase.CaseRegistrationNumber;
                        DateTime oldRegistrationDate = extractedCase.CaseRegistrationDate;
                        KeyValuePair<Case, string> buffer = BuildQueryAndChangeExtractedCase(extractedCase,
                            checkedIndex, senderName);

                        extractedCase = buffer.Key;
                        string setPartOfQuery = buffer.Value;

                        //Check if the registration number and date (which are 
                        //unique key constraint)are changed
                        if (checkedIndex == 1)
                        {
                            if (dataDriver.CheckIfRegistrationNumberAndYearExist(
                                extractedCase.CaseRegistrationNumber,
                                extractedCase.CaseRegistrationDate,
                                true)
                                )
                            {
                                throw new DatabaseNameAndDateExistException("UPDATE cannot be executed:\nThere is another record with\n" +
                                    "this registration number and date.");
                            }
                        }

                        int rowsAffected = dataDriver
                            .UpdateModifiedCase(extractedCase, setPartOfQuery,
                            oldRegistrationNumber, oldRegistrationDate);
                        ClearAndDiseableUpdatePanelControls();
                        LabelTextColorChanger(lblUpdateErrors, successColor,
                            string.Format("Updated Successfully.\n{0} row\\s affected",
                            rowsAffected));
                        GetDataToGrid(Constants.Constants.STARTUP_CASES_TABLE_GRID_DISPLAY,
                            dgvCaseDisplay);
                        if (checkedIndex == 1)
                        {
                            GetDataToGrid(Constants.Constants.DEFAULT_REQUEST_TABLE_GRID_DISPLAY,
                                dgvRequestsDisplay);
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex) when
            (ex is CaseClassException ||
            ex is DatabaseNameAndDateExistException ||
            ex is ZeroOrTooManyResultsFoundException ||
            ex is SQLiteException)
            {
                //Display in the panel all the errors, that are thrown
                //inside the case class validation
                LabelTextColorChanger(lblUpdateErrors, errorColor, ex.Message);
            }
        }

        private void btnUpdateDelete_Click(object sender, EventArgs e)
        {
            lblUpdateErrors.Text = "";
            try
            {
                var rowsAffected = dataDriver
                .DeleteCaseAndRelatedRequestsFromDatabase(
                tbUpdateRegNumInput.Text, dtpUpdateRegNumDate.Value);

                if (rowsAffected > 0)
                {
                    ClearAndDiseableUpdatePanelControls();
                    LabelTextColorChanger(lblUpdateErrors, successColor,
                        string.Format("Success:\n{0} case/s deleted.",
                        rowsAffected));
                    GetDataToGrid(Constants.Constants.STARTUP_CASES_TABLE_GRID_DISPLAY,
                            dgvCaseDisplay);
                    GetDataToGrid(Constants.Constants.DEFAULT_REQUEST_TABLE_GRID_DISPLAY,
                            dgvRequestsDisplay);
                }
                else
                {
                    LabelTextColorChanger(lblUpdateErrors, errorColor,
                        "No matches found in the database.");
                }
            }
            catch (Exception ex) when (ex is SQLiteException || ex is ZeroOrTooManyResultsFoundException)
            {
                LabelTextColorChanger(lblUpdateErrors, errorColor,
                        ex.Message);
            }
        }
        #endregion

        #region SearchPanel Methods
        private void ShowAllData(object sender, EventArgs e)
        {
            string caseQuery =
            "SELECT " +
            "registration_number AS 'Registration Number'," +
            "registration_date AS 'Registration Date'," +
            "source_name AS 'Source'," +
            "(CASE WHEN is_finished = 0 THEN 'No' ELSE 'Yes' END) AS 'Finished'," +
            "basic_info AS 'Basic Information'" +
            " FROM Cases";

            string requestQuery =
            "SELECT " +
            "Cases.registration_number AS 'Case', " +
            "(CASE WHEN Requests.answer_received = 1 THEN 'Yes' ELSE 'No' END) AS 'Answered', " +
            "Requests.request_info AS 'Sent to' " +
            //"Requests.registration_number AS 'Reg.Number' " +
            "FROM Requests " +
            "JOIN Cases ON Cases.id = Requests.case_id ";

            GetDataToGrid(caseQuery, dgvCaseDisplay);
            GetDataToGrid(requestQuery, dgvRequestsDisplay);

        }

        private void cbSearchEnableDoctypeSelect_CheckedChanged(object sender, EventArgs e)
        {
            cboxSearchTypePicker.Enabled = cbSearchEnableDoctypeSelect.Checked;
        }

        private void cbSearchTimePeriod_CheckedChanged(object sender, EventArgs e)
        {
            bool state = cbSearchTimePeriod.Checked;
            dtpSearchFromDate.Enabled = state;
            dtpSearchToDate.Enabled = state;
        }

        private void cbSearchRegistraionNumberCheck_CheckedChanged(object sender, EventArgs e)
        {
            tbSearchRegNumber.Enabled = cbSearchRegistraionNumberCheck.Checked;
            tbSearchRegNumber.Text = "";
        }

        private void ChangeDisplayedRequestsOnClick(object sender, EventArgs e)
        {
            Button senderButton = (Button)sender;
            string statement = "";
            switch (senderButton.Name)
            {
                case "btnSearchAnsweredRequests":
                    statement = "SELECT " +
                    "Cases.registration_number AS 'Case', " +
                    "Requests.request_info AS 'Sent to', " +
                    "Requests.registration_number AS 'Reg.Number' " +
                    "FROM Requests " +
                    "JOIN Cases ON Cases.id = Requests.case_id " +
                    "WHERE Requests.answer_received = '1'";
                    GetDataToGrid(statement, dgvRequestsDisplay);
                    break;
                case "btnSearchRequestsWithNoAnswer":
                    statement = "SELECT " +
                    "Cases.registration_number AS 'Case', " +
                    "Requests.request_info AS 'Sent to', " +
                    "Requests.registration_number AS 'Reg.Number' " +
                    "FROM Requests " +
                    "JOIN Cases ON Cases.id = Requests.case_id " +
                    "WHERE Requests.answer_received = '0'";
                    GetDataToGrid(statement, dgvRequestsDisplay);
                    break;
                case "btnSearchAllRequests":
                    statement = "SELECT " +
                    "Cases.registration_number AS 'Case', " +
                    "Requests.request_info AS 'Sent to', " +
                    "Requests.registration_number AS 'Reg.Number' " +
                    "FROM Requests " +
                    "JOIN Cases ON Cases.id = Requests.case_id";
                    GetDataToGrid(statement, dgvRequestsDisplay);
                    break;
            }
        }

        private void ShowDisplayedCasesAndRequests(object sender, EventArgs e)
        {
            lblVisualCount.ForeColor = Color.DimGray;
            lblVisualCount.Text = string.Format("{0} Cases | {1} Requests",
                dgvCaseDisplay.RowCount,
                dgvRequestsDisplay.RowCount);
        }

        private void lblVisualCount_MouseLeave(object sender, EventArgs e)
        {
            lblVisualCount.ForeColor = Color.Azure;
            lblVisualCount.Text = "Show me result count.";
        }

        private void ExecuteSearchInTheDB(object sender, EventArgs e)
        {
            //Build statement with the data from GUI
            string statement = "SELECT {0} FROM Cases {1}";
            string conditions = "";
            List<string> fieldsRequired = new List<string>();
            List<string> conditionsList = new List<string>();
            fieldsRequired.Add("registration_number AS 'Reg. Number'"); //mandatory field for the result
            foreach (CheckBox cb in pnlSearchFieldsNeeded
                .Controls.OfType<CheckBox>())
            {
                if (cb.Checked)
                {
                    fieldsRequired.Add(cb.Tag.ToString());
                }
            }
            //Check if doctype filter is selected
            if (cbSearchEnableDoctypeSelect.Checked)
            {
                conditionsList.Add(string.Format("{0} = '{1}'",
                    cbSearchEnableDoctypeSelect.Tag.ToString(),
                    cboxSearchTypePicker.SelectedIndex.ToString()));
            }
            //Check if time period filter is selected 
            //and sort picked date values
            if (cbSearchTimePeriod.Checked)
            {
                List<DateTime> period = new List<DateTime>();
                period.Add(dtpSearchFromDate.Value.Date);
                period.Add(dtpSearchToDate.Value.Date);
                period.Sort((d1, d2) => d1.CompareTo(d2));

                conditionsList.Add(string.Format("(date({0}) BETWEEN date('{1}') AND date('{2}'))",
                    cbSearchTimePeriod.Tag.ToString(),
                    period[0].ToString("yyyy-MM-dd HH:mm:ss"),
                    period[1].ToString("yyyy-MM-dd HH:mm:ss")));
            }

            //Check if the user searche by registration number from the tbSearchRegNumber
            if (cbSearchRegistraionNumberCheck.Checked)
            {
                string inputBuffer = tbSearchRegNumber.Text.Trim();
                if (DataValidator.ValidateSearchRegistrationNumber(
                    inputBuffer))
                {
                    conditionsList.Add(string.Format("{0} = '{1}'",
                        cbSearchRegistraionNumberCheck.Tag.ToString(),
                        inputBuffer));
                }
                else
                {
                    tbSearchRegNumber.Text = "***  Invalid fromat  ***";
                }
            }


            //Build the statement with all conditions
            if (conditionsList.Count > 0)
            {
                conditions = string.Format(" WHERE({0})",
                    string.Join(" AND ", conditionsList));
            }
            statement = string.Format(statement,
            string.Join(", ", fieldsRequired), conditions);
            //Display and format result
            //tbDisplayTableError.Text = statement;
            GetDataToGrid(statement, dgvCaseDisplay);
            reportStatemet = new Tuple<string, string>(
                Constants.Constants.DEFAULT_REPORT_STATEMENT + conditions, 
                (cbSearchTimePeriod.Checked) ? 
                string.Format("{0}-{1}", 
                dtpSearchFromDate.Value.ToString("dd.MM.yyyy"),
                dtpSearchToDate.Value.ToString("dd.MM.yyyy")) 
                : null);
            //tbDisplayTableError.Text = reportStatemet;
        }
        #endregion

        #region ReportBuilding and ReportReading
        private void GenerateReport(object sender, EventArgs e)
        {
            try
            {
                string nameBuffer = tbSearchReportOfficerName.Text.Trim();
                if (DataValidator.ValidateOfficerNameForReport(nameBuffer))
                {
                    var newReport = 
                        dataDriver.PopulateReport(reportStatemet.Item1, reportStatemet.Item2);
                    //TODO: Export to xml
                    bool dataExported = reportDriver.ReportBuilder(newReport.Item1, newReport.Item2, nameBuffer);
                    if (dataExported)
                    {
                        LabelTextColorChanger(lblSearchReportException, Color.Green,
                            "Success. File exported to desktop.");
                    }
                }
            }
            catch (Exception ex)
            when (ex is ReportArgumentFormatException ||
            ex is ReportValueOutOfRangeException ||
            ex is System.Xml.XmlException)
            {
                LabelTextColorChanger(lblSearchReportException,
                    Color.Red, ex.Message);
            }
        }

        #endregion

        #region Display Table Fill Method
        public void GetDataToGrid(string SQLiteCommand,
            DataGridView dataGrid)
        {
            try
            {
                dataDriver.GetDataToGrid(SQLiteCommand, dataGrid);
                //custom format of the table result
                foreach (DataGridViewColumn col in dataGrid.Columns)
                {
                    if (col.ValueType == typeof(DateTime))
                    {
                        col.DefaultCellStyle.Format = "dd.MM.yyyy";
                    }
                }
            }
            catch (Exception ex) when (
                ex is ArgumentNullException ||
                ex is DBConcurrencyException ||
                ex is InvalidOperationException ||
                ex is SQLiteException)
            {
                tbDisplayTableError.Text = ex.Message;
            }
        }
        #endregion

        #region ButtonEvents Enter and Leave - color change
        private void Button_MouseEnter(object sender, EventArgs e)
        {
            ChangeButonColorsOnMouseover((Button)sender, true);
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            ChangeButonColorsOnMouseover((Button)sender, false);
        }

        private void ChangeButonColorsOnMouseover(Control button,
            bool mouseEnter)
        {
            if (mouseEnter)
            {
                button.ForeColor = Color.Black;
                button.BackColor = Color.Cyan;
            }
            else
            {
                button.ForeColor = Color.White;
                button.BackColor = Color.Teal;
            }
        }
        #endregion

        #region Support Methods
        private void OnVisiblePanelChange(object sender, EventArgs e)
        {
            RestartAllControlsInPanel(selectedPanel);

            var checkedRadioButton = pnlNavigation.Controls.
                OfType<RadioButton>().Where(r => r.Checked == true).
                First();

            if (selectedPanel.Name == "pnlSearch" &&
                checkedRadioButton.Name != "rbSearch")
            {
                GetDataToGrid(Constants.Constants.STARTUP_CASES_TABLE_GRID_DISPLAY,
                    dgvCaseDisplay);
                GetDataToGrid(Constants.Constants.DEFAULT_REQUEST_TABLE_GRID_DISPLAY,
                    dgvRequestsDisplay);
                reportStatemet = new Tuple<string, string>(Constants.Constants.DEFAULT_REPORT_STATEMENT, null);
                lblSearchReportException.Text = "";
            }

            pnlMain.Controls.OfType<Panel>().Any(p => p.Visible = false);

            switch (checkedRadioButton.Name)
            {
                case "rbInsert":
                    pnlInsert.Visible = true;
                    selectedPanel = pnlInsert;
                    break;
                case "rbUpdate":
                    pnlUpdate.Visible = true;
                    selectedPanel = pnlUpdate;
                    break;
                case "rbSearch":
                    pnlSearch.Visible = true;
                    cboxSearchTypePicker.SelectedIndex = 0;
                    selectedPanel = pnlSearch;
                    break;
                case "rbReportAnalyzer":
                    pnlAnalyze.Visible = true;
                    selectedPanel = pnlAnalyze;
                    break;
                case "rbHelp":
                    pnlHelp.Visible = true;
                    selectedPanel = pnlHelp;
                    break;
            }
        }

        private void RestartAllControlsInPanel(Control parent)
        {
            foreach (TextBox tb in
                parent.Controls.OfType<TextBox>())
            {
                tb.Text = "";
            }

            foreach (DateTimePicker dtp in
                parent.Controls.OfType<DateTimePicker>())
            {
                dtp.Value = DateTime.Today;
            }

            foreach (ComboBox cb in
                parent.Controls.OfType<ComboBox>())
            {
                cb.SelectedIndex = -1;
            }

            //Panel speciffic cleans

            switch (parent.Name)
            {
                case "pnlInsert":
                    pnlInsertThumbUp.Visible = false;
                    btnInsertNew.Enabled = false;
                    btnInsert.Enabled = true;
                    lblInsertValidationErrorsProsec.Text = "";
                    lblInsertValidationErrorsReg.Text = "";
                    break;
                case "pnlUpdate":
                    ClearAndDiseableUpdatePanelControls();
                    lblUpdateErrors.Text = "";
                    break;
                case "pnlSearch":
                    cboxSearchTypePicker.Enabled = false;
                    cboxSearchTypePicker.SelectedIndex = 0;
                    foreach (Panel pnl in parent.Controls.OfType<Panel>())
                    {
                        foreach (CheckBox cb in
                        pnl.Controls.OfType<CheckBox>())
                        {
                            cb.Checked = false;
                        }
                    }

                    break;
                case "pnlAnalyze":
                    break;
                case "pnlHelp":
                    break;
            }
        }

        private void ClearAndDiseableUpdatePanelControls()
        {
            foreach (TextBox tb in
                pnlUpdate.Controls.OfType<TextBox>())
            {
                tb.Text = "";
                if (tb.Name != "tbUpdateRegNumInput")
                {
                    tb.Enabled = false;
                }
            }

            foreach (Button btn in
                pnlUpdate.Controls.OfType<Button>())
            {
                if (btn.Name != "btnUpdateDelete")
                {
                    btn.Enabled = false;
                }
            }

            foreach (DateTimePicker dtp in
                pnlUpdate.Controls.OfType<DateTimePicker>())
            {
                dtp.Value = DateTime.Now.Date;
                if (dtp.Name != "dtpUpdateRegNumDate")
                {
                    dtp.Enabled = false;
                }
            }
            cbUpdateFieldSelect.SelectedIndex = -1;
        }

        private void ChangeUpdateControlsEnableState(Control[] controls,
            bool[] values)
        {
            for (int index = 0; index < controls.Length; index++)
            {
                controls[index].Enabled = values[index];
            }
        }

        private void cbUpdateFieldSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool[] controlsState = { false, false, false, false, false };
            Control[] updateControls = { tbUpdatePrimary, tbUpdateSecondary,
            dtpUpdateChangeDate, btnUpdateAddData, btnUpdateChangeData};
            switch (cbUpdateFieldSelect.SelectedIndex)
            {
                //Basic Info Change
                case 0:
                    controlsState[0] = true;
                    controlsState[3] = true;
                    controlsState[4] = true;
                    break;
                //Registration Number and date Change
                case 1:
                    controlsState[0] = true;
                    controlsState[2] = true;
                    controlsState[4] = true;
                    break;
                //Done material boolean change
                case 2:
                    controlsState[4] = true;
                    break;
                //Change of the alowed working period
                case 3:
                    controlsState[2] = true;
                    controlsState[4] = true;
                    break;
                //Change the name of the source
                case 4:
                    controlsState[0] = true;
                    controlsState[4] = true;
                    break;
                //Add sent message
                case 5:
                    controlsState[0] = true;
                    controlsState[1] = true;
                    controlsState[3] = true;
                    break;
                //Delete sent message
                case 6:
                    controlsState[0] = true;
                    controlsState[4] = true;
                    break;
                //Change the "recieved" state of a sent message
                case 7:
                    controlsState[0] = true;
                    controlsState[4] = true;
                    break;
                //Change the registration number and date of a
                //prosecution decree
                case 8:
                    controlsState[0] = true;
                    controlsState[2] = true;
                    controlsState[4] = true;
                    break;
                //Change the name of a prosecution office
                case 9:
                    controlsState[0] = true;
                    controlsState[4] = true;
                    break;
            }

            ChangeUpdateControlsEnableState(updateControls, controlsState);
        }

        private void cbUpdateTypePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            cbUpdateFieldSelect.Items.Clear();
            ClearAndDiseableUpdatePanelControls();
            switch (cbUpdateTypePicker.SelectedIndex)
            {
                case 0:
                case 1:
                    cbUpdateFieldSelect.Items.AddRange(
                        Constants.Constants.UPDATE_BASIC_DATAFIELDS_LIST);
                    break;
                case 2:
                    cbUpdateFieldSelect.Items.AddRange(
                        Constants.Constants.UPDATE_PROSECUTION_DATAFIELDS_LIST);
                    break;
                case 3:
                case 4:
                    cbUpdateFieldSelect.Items.AddRange(
                        Constants.Constants.UPDATE_BASIC_DATAFIELDS_LIST);
                    break;

                default:
                    break;
            }
        }

        private void LabelTextColorChanger(Label label, Color color, string text)
        {
            label.ForeColor = color;
            label.Text = text;
        }

        private void DeveloperLabelMouseEnter(object sender, EventArgs e)
        {
            Color color = Color.Gold;
            lblNavigationDeveloper.ForeColor = color;
            lblDisplayRequestsPanelTitle.ForeColor = color;
            lblDisplayCaseData.ForeColor = color;
            lblTitleName.ForeColor = color;
            lblTitleText.ForeColor = color;
        }

        private void DeveloperLabelMouseLeave(object sender, EventArgs e)
        {
            lblDisplayRequestsPanelTitle.ForeColor = Color.White;
            lblDisplayCaseData.ForeColor = Color.White;
            lblTitleName.ForeColor = Color.White;
            lblNavigationDeveloper.ForeColor = Color.Lime;
            lblTitleText.ForeColor = Color.Cyan;
        }

        #endregion

        private void AnalyzeOneOrMoreReports(object sender, EventArgs e)
        {
            List<string> xmlFilePaths = GetPathOfXMLFiles();
            List<Report> extractedReports = new List<Report>();
            if (xmlFilePaths.Count > 0)
            {
                dgvAnalyzeDisplay.Rows.Clear();
                Report buffer = new Report();
                for (int index = 0; index < xmlFilePaths.Count; index++)
                {
                    buffer = reportDriver.XMLtoReport(
                        xmlFilePaths[index], dgvAnalyzeDisplay, buffer);
                }
                AddSummaryRowToGrid(buffer, dgvAnalyzeDisplay);
            }
        }

        private void AddSummaryRowToGrid(Report report, DataGridView grid)
        {
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(grid);
            row.Cells[0].Value = "Summary";
            row.Cells[1].Value = "";
            row.Cells[2].Value = report.PoliceCheckCount.ToString();
            row.Cells[3].Value = report.RequestFromDMOSCount.ToString();
            row.Cells[4].Value = report.ProsecutionDecreeCount.ToString();
            row.Cells[5].Value = report.InvestigationOfficerDecreeCount.ToString();
            row.Cells[6].Value = report.SignalFromDMOSCount.ToString();
            row.DefaultCellStyle.BackColor = Color.Black;
            row.DefaultCellStyle.ForeColor = Color.Lime;
            grid.Rows.Add(row);
        }

        private List<string> GetPathOfXMLFiles()
        {
            List<string> buffer = new List<string>();
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "XML Files|*.xml";
            fileDialog.Multiselect = true;

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string filePath in fileDialog.FileNames)
                {
                    buffer.Add(filePath);
                }
            }

            return buffer;
        }
    }
}