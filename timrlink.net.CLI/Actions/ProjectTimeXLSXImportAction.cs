using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using timrlink.net.Core.Service;

namespace timrlink.net.CLI.Actions
{
    internal class ProjectTimeXLSXImportAction : ProjectTimeImportAction
    {
        private static readonly IDictionary<string, ColumnDefinition> ColumnDefinitions = new Dictionary<string, ColumnDefinition>
        {
            { "User", ColumnDefinition.English },
            { "Benutzer", ColumnDefinition.German },
            { "Usuario", ColumnDefinition.Spanish }
        };

        public ProjectTimeXLSXImportAction(ILoggerFactory loggerFactory, string filename, ITaskService taskService, IUserService userService, IProjectTimeService projectTimeService)
            : base(loggerFactory.CreateLogger<ProjectTimeXLSXImportAction>(), filename, taskService, userService, projectTimeService)
        {
        }

        protected override IEnumerable<ProjectTimeEntry> ParseFile()
        {
            using (var document = SpreadsheetDocument.Open(Filename, false))
            {
                var worksheet = document.WorkbookPart.WorksheetParts.First().Worksheet;
                var sheetData = worksheet.Elements<SheetData>().Single();
                var rows = sheetData.Cast<Row>().ToList();

                var headerRow = rows.First();

                var userColumnLabel = document.WorkbookPart.GetStringValue(headerRow.Elements<Cell>().ElementAt(0));
                if (!ColumnDefinitions.TryGetValue(userColumnLabel, out var columnDefinition))
                {
                    throw new InvalidOperationException("File is not in the supported format!");
                }

                var columnMapping = new ColumnMapping(document.WorkbookPart, headerRow, columnDefinition);

                return rows.Skip(1).Select(row =>
                {
                    var columns = row.Elements<Cell>().ToDictionary(ColumnNameFromCell);
                    var externalId = GetStringValue(document.WorkbookPart, columnMapping.UserExternalId, columns);
                    var externalTaskId = GetStringValue(document.WorkbookPart, columnMapping.TaskExternalId, columns);
                    if (String.IsNullOrEmpty(externalTaskId))
                    {
                        externalTaskId = String.Join("|", columnMapping.TaskLevels
                            .Select(level => columns.TryGetValue(level, out var cell) ? cell : null)
                            .Where(cell => cell != null)
                            .Select(cell => document.WorkbookPart.GetStringValue(cell))
                        );
                    }

                    var notes = GetStringValue(document.WorkbookPart, columnMapping.Notes, columns);

                    string startDateTimeIndex;
                    if (columnMapping.StartDateTime != null)
                    {
                        startDateTimeIndex = columnMapping.StartDateTime;
                    }
                    else if (columnMapping.StartTime != null)
                    {
                        startDateTimeIndex = columnMapping.StartTime;
                    }
                    else
                    {
                        throw new ArgumentException("StartTime column could not get calculated!");
                    }

                    var startTime = DateTime.FromOADate(double.Parse(columns[startDateTimeIndex].InnerText));
                    var startTimeZone = GetStringValue(document.WorkbookPart, columnMapping.StartTimeZone, columns);

                    string endDateTimeIndex;
                    if (columnMapping.EndDateTime != null)
                    {
                        endDateTimeIndex = columnMapping.EndDateTime;
                    }
                    else if (columnMapping.EndTime != null)
                    {
                        endDateTimeIndex = columnMapping.EndTime;
                    }
                    else
                    {
                        throw new ArgumentException("EndTime column could not get calculated!");
                    }

                    var endTime = DateTime.FromOADate(double.Parse(columns[endDateTimeIndex].InnerText));
                    var endTimeZone = GetStringValue(document.WorkbookPart, columnMapping.EndTimeZone, columns);

                    var duration = (long) (DateTime.FromOADate(double.Parse(columns[columnMapping.Duration].CellValue.Text)) - DateTime.FromOADate(0)).TotalMinutes;
                    var billable = columns[columnMapping.Billable].CellValue.Text == "1";
                    var changed = columns[columnMapping.ManuallyChanged].CellValue.Text == "1";
                    var @break = (int) decimal.Parse(columns[columnMapping.Break].CellValue.Text);

                    return new ProjectTimeEntry()
                    {
                        User = externalId,
                        Task = externalTaskId,
                        StartDateTime = startTime,
                        StartTimeZone = startTimeZone,
                        EndDateTime = endTime,
                        EndTimeZone = endTimeZone,
                        duration = duration,
                        Break = @break,
                        Notes = notes,
                        Billable = billable,
                        Changed = changed
                    };
                }).ToList();
            }
        }

        private static string ColumnNameFromCell(Cell cell) => cell != null ? Regex.Replace(cell.CellReference.Value, "\\d+", "") : null;

        private static string GetStringValue(WorkbookPart workbookPart, string column, IDictionary<string, Cell> cells)
        {
            if (column != null && cells.TryGetValue(column, out var cell))
                return workbookPart.GetStringValue(cell);
            else
                return null;
        }

        private class ColumnDefinition
        {
            public static readonly ColumnDefinition English = new ColumnDefinition
            {
                User = "User",
                UserExternalId = "User External ID",
                Organization = "Organization",
                Task = "Task",
                TaskExternalId = "Task External ID",
                Notes = "Notes",
                StartDateTime = "Start",
                StartDate = "Start Date",
                StartTime = "Start Time",
                EndDateTime = "End",
                EndDate = "End Date",
                EndTime = "End Time",
                Zone = "Zone",
                Billable = "Billable",
                ManuallyChanged = "Manually changed",
                StartPosition = "Start Position",
                EndPosition = "End Position",
                Duration = "Duration",
                Break = "Break",
                TaskLevelPrefix = "Task level ",
            };

            public static readonly ColumnDefinition German = new ColumnDefinition
            {
                User = "Benutzer",
                UserExternalId = "Benutzer Externe ID",
                Organization = "Organisation",
                Task = "Aufgabe",
                TaskExternalId = "Aufgabe Externe ID",
                Notes = "Notiz",
                StartDateTime = "Start",
                StartDate = "Startdatum",
                StartTime = "Startzeit",
                EndDateTime = "Ende",
                EndDate = "Enddatum",
                EndTime = "Endzeit",
                Zone = "Zone",
                Billable = "Verrechenbar",
                ManuallyChanged = "Manuell verändert",
                StartPosition = "Startposition",
                EndPosition = "Endposition",
                Duration = "Dauer",
                Break = "Pause",
                TaskLevelPrefix = "Ebene ",
            };

            public static readonly ColumnDefinition Spanish = new ColumnDefinition
            {
                User = "Usuario",
                UserExternalId = "ID externa del usuario",
                Organization = "Organización",
                Task = "Tarea",
                TaskExternalId = "ID externa de la tarea",
                Notes = "Notas",
                StartDateTime = "Iniciar",
                StartDate = "Fecha de inicio",
                StartTime = "Hora de inicio",
                EndDateTime = "Fin",
                EndDate = "fecha final",
                EndTime = "Hora final",
                Zone = "Zona",
                Billable = "Facturable",
                ManuallyChanged = "Modificado manualmente",
                StartPosition = "Posición inicial",
                EndPosition = "Posición final",
                Duration = "Duración",
                Break = "Pausa",
                TaskLevelPrefix = "Nivel de la tarea ",
            };

            private ColumnDefinition()
            {
            }

            public string User { get; private set; }
            public string UserExternalId { get; private set; }
            public string Organization { get; private set; }
            public string Task { get; private set; }
            public string TaskExternalId { get; private set; }
            public string Notes { get; private set; }
            public string StartDateTime { get; private set; }
            public string StartDate { get; private set; }
            public string StartTime { get; private set; }
            public string EndDateTime { get; private set; }
            public string EndDate { get; private set; }
            public string EndTime { get; private set; }
            public string Zone { get; private set; }
            public string Billable { get; private set; }
            public string ManuallyChanged { get; private set; }
            public string StartPosition { get; private set; }
            public string EndPosition { get; private set; }
            public string Duration { get; private set; }
            public string Break { get; private set; }
            public string TaskLevelPrefix { get; private set; }
        }

        private class ColumnMapping
        {
            private readonly WorkbookPart workbookPart;
            private readonly ColumnDefinition columnDefinition;
            private readonly List<Cell> cells;

            public ColumnMapping(WorkbookPart workbookPart, Row headerRow, ColumnDefinition columnDefinition)
            {
                this.workbookPart = workbookPart;
                this.columnDefinition = columnDefinition;
                this.cells = headerRow.Elements<Cell>().ToList();
            }

            public string User => GetColumnName(columnDefinition.User);
            public string UserExternalId => GetColumnName(columnDefinition.UserExternalId);
            public string Organization => GetColumnName(columnDefinition.Organization);
            public string Task => GetColumnName(columnDefinition.Task);
            public string TaskExternalId => GetColumnName(columnDefinition.TaskExternalId);
            public string Notes => GetColumnName(columnDefinition.Notes);
            public string StartDateTime => GetColumnName(columnDefinition.StartDateTime);
            public string StartDate => GetColumnName(columnDefinition.StartDate);
            public string StartTime => GetColumnName(columnDefinition.StartTime);

            public string StartTimeZone
            {
                get
                {
                    var cell = cells.Find(c => workbookPart.GetStringValue(c) == columnDefinition.Zone);
                    return ColumnNameFromCell(cell);
                }
            }

            public string EndDateTime => GetColumnName(columnDefinition.EndDateTime);
            public string EndDate => GetColumnName(columnDefinition.EndDate);
            public string EndTime => GetColumnName(columnDefinition.EndTime);

            public string EndTimeZone
            {
                get
                {
                    var cell = cells.Find(c => workbookPart.GetStringValue(c) == columnDefinition.Zone && ColumnNameFromCell(c) != StartTimeZone);
                    return ColumnNameFromCell(cell);
                }
            }

            public string Billable => GetColumnName(columnDefinition.Billable);
            public string ManuallyChanged => GetColumnName(columnDefinition.ManuallyChanged);
            public string StartPosition => GetColumnName(columnDefinition.StartPosition);
            public string EndPosition => GetColumnName(columnDefinition.EndPosition);
            public string Duration => GetColumnName(columnDefinition.Duration);
            public string Break => GetColumnName(columnDefinition.Break);

            public List<string> TaskLevels
            {
                get
                {
                    return cells
                        .Where(c => workbookPart.GetStringValue(c).StartsWith(columnDefinition.TaskLevelPrefix))
                        .Select(ColumnNameFromCell)
                        .ToList();
                }
            }

            private string GetColumnName(string headerValue)
            {
                return ColumnNameFromCell(cells.Find(c => workbookPart.GetStringValue(c) == headerValue));
            }
        }
    }
}
