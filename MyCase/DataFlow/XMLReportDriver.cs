using MyCase.Interfaces;
using System;
using MyCase.Classes;
using System.Xml.Linq;
using System.Windows.Forms;

namespace MyCase.DataFlow
{
    public class XMLReportDriver : IReportEngine
    {
        public bool ReportBuilder(Report activeReport, Report finishedReport,
            string senderName)
        {
            XDocument doc = new XDocument(
                new XElement("Report",
                    new XElement("From", senderName),
                    new XElement("Period", activeReport.SearchPeriod),
                    new XElement("Cases",
                        new XElement("Signal_DMOS",
                            new XElement("Active",
                            activeReport.SignalFromDMOSCount),
                            new XElement("Finished", 
                            finishedReport.SignalFromDMOSCount)
                            ),
                        new XElement("Investigation_Officer_Decree",
                            new XElement("Active",
                            activeReport.InvestigationOfficerDecreeCount),
                            new XElement("Finished",
                            finishedReport.InvestigationOfficerDecreeCount)
                            ),
                        new XElement("Prosecutor_decree",
                            new XElement("Active",
                            activeReport.ProsecutionDecreeCount),
                            new XElement("Finished",
                            finishedReport.ProsecutionDecreeCount)
                            ),
                        new XElement("Requests_DMOS",
                            new XElement("Active",
                            activeReport.RequestFromDMOSCount),
                            new XElement("Finished",
                            finishedReport.RequestFromDMOSCount)
                            ),
                        new XElement("Police_checks",
                           new XElement("Active",
                            activeReport.PoliceCheckCount),
                            new XElement("Finished",
                            finishedReport.PoliceCheckCount)
                            )
                        )
                    )
                );
            string location = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            doc.Save(string.Format("{0}\\MyCaseReport-{1}.xml",
                location,
                DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss.fff")));

            return true;
        }

        public Report XMLtoReport(string filePath, DataGridView grid, Report sumReport)
        {
            var document = XDocument.Load(filePath);
            //generate row
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(grid);
            row.Cells[0].Value = document.Root.Element("From").Value;
            row.Cells[1].Value = document.Root.Element("Period").Value;
            row.Cells[2].Value = string.Format("{0} нал./ {1} прикл.",
                document.Root.Element("Cases").Element("Police_checks").Element("Active").Value,
                document.Root.Element("Cases").Element("Police_checks").Element("Finished").Value);
            row.Cells[3].Value = string.Format("{0} нал./ {1} прикл.",
                document.Root.Element("Cases").Element("Requests_DMOS").Element("Active").Value,
                document.Root.Element("Cases").Element("Requests_DMOS").Element("Finished").Value);
            row.Cells[4].Value = string.Format("{0} нал./ {1} прикл.",
                document.Root.Element("Cases").Element("Prosecutor_decree").Element("Active").Value,
                document.Root.Element("Cases").Element("Prosecutor_decree").Element("Finished").Value);
            row.Cells[5].Value = string.Format("{0} нал./ {1} прикл.",
                document.Root.Element("Cases").Element("Investigation_Officer_Decree").Element("Active").Value,
                document.Root.Element("Cases").Element("Investigation_Officer_Decree").Element("Finished").Value);
            row.Cells[6].Value = string.Format("{0} нал./ {1} прикл.",
                document.Root.Element("Cases").Element("Signal_DMOS").Element("Active").Value,
                document.Root.Element("Cases").Element("Signal_DMOS").Element("Finished").Value);
            grid.Rows.Add(row);

            sumReport.PoliceCheckCount += (
                int.Parse(document.Root.Element("Cases").Element("Police_checks").Element("Active").Value) +
                int.Parse(document.Root.Element("Cases").Element("Police_checks").Element("Finished").Value));
            sumReport.RequestFromDMOSCount += (
                int.Parse(document.Root.Element("Cases").Element("Requests_DMOS").Element("Active").Value) +
                int.Parse(document.Root.Element("Cases").Element("Requests_DMOS").Element("Finished").Value));
            sumReport.SignalFromDMOSCount += (
                int.Parse(document.Root.Element("Cases").Element("Signal_DMOS").Element("Active").Value) +
                int.Parse(document.Root.Element("Cases").Element("Signal_DMOS").Element("Finished").Value));
            sumReport.InvestigationOfficerDecreeCount += (
                int.Parse(document.Root.Element("Cases").Element("Investigation_Officer_Decree").Element("Active").Value) +
                int.Parse(document.Root.Element("Cases").Element("Investigation_Officer_Decree").Element("Finished").Value));
            sumReport.ProsecutionDecreeCount += (
                int.Parse(document.Root.Element("Cases").Element("Prosecutor_decree").Element("Active").Value) +
                int.Parse(document.Root.Element("Cases").Element("Prosecutor_decree").Element("Finished").Value));
            return sumReport;
        }
    }
}
