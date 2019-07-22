using System;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace timrlink.net.CLI
{
    internal static class OpenXmlExtensions
    {
        public static string GetStringValue(this WorkbookPart workbook, Cell cell)
        {
            switch (cell.DataType.Value)
            {
                case CellValues.SharedString:
                    if (int.TryParse(cell.CellValue.Text, out var id))
                        return workbook.GetSharedString(id);
                    else
                        return null;
                case CellValues.String:
                case CellValues.InlineString:
                case CellValues.Number:
                    return cell.CellValue.Text;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cell.DataType), cell.DataType, "");
            }
        }

        private static string GetSharedString(this WorkbookPart workbookPart, int index)
        {
            var sharedStringTable = workbookPart.SharedStringTablePart.SharedStringTable;
            var sharedStringItems = sharedStringTable.Elements<SharedStringItem>();
            return sharedStringItems.ElementAt(index).Text.Text;
        }
    }
}
