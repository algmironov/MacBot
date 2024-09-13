using MacBot.ConsoleApp.Models;

using OfficeOpenXml;

namespace MacBot.ConsoleApp.Services
{
    public class SessionExporter
    {
        public static MemoryStream ExportSessionsToExcel(List<Session> sessions)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();

            var worksheet = package.Workbook.Worksheets.Add("Sessions");

            // Заголовки
            worksheet.Cells[1, 1].Value = "ID сессии";
            worksheet.Cells[1, 2].Value = "Дата";
            worksheet.Cells[1, 3].Value = "Продолжительность";
            worksheet.Cells[1, 4].Value = "Имя клиента";
            worksheet.Cells[1, 5].Value = "Показанные карты";

            int row = 2;

            foreach (var session in sessions)
            {
                worksheet.Cells[row, 1].Value = session.SessionId.ToString();
                worksheet.Cells[row, 2].Value = session.Date.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 3].Value = session.Duration.ToString(@"hh\:mm\:ss");
                worksheet.Cells[row, 4].Value = session.Client?.Name ?? "N/A";

                var shownCards = session.ChoosenCards?
                    .Where(sc => sc.IsShown)
                    .Select(sc => Path.GetFileName(sc.Card?.Link))
                    .Where(link => !string.IsNullOrEmpty(link));

                worksheet.Cells[row, 5].Value = shownCards != null ? string.Join(", ", shownCards) : "";

                row++;
            }

            worksheet.Cells.AutoFitColumns();

            var stream = new MemoryStream(package.GetAsByteArray());
            return stream;
        }
    }
}
