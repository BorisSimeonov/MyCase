using MyCase.Classes;
using System;
using System.Windows.Forms;

namespace MyCase.Interfaces
{
    interface IReportEngine
    {
        bool ReportBuilder(Report activeReport, Report finishedReport, string reportSender);
        Report XMLtoReport(string filePath, DataGridView grid, Report sumReport);
    }
}
