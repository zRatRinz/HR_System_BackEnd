using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HR_System.Application.DTOs.Attendance;
using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services.PdfGenerators;

public class AttendanceOverviewReportPdfGenerator : IAttendanceOverviewReportPdfGenerator
{
    public byte[] Generate(List<AttendanceDto> attendanceRecords, AttendanceOverviewReportOptions options)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("TH Sarabun New"));

                page.Header().Element(c => ComposeHeader(c, options, attendanceRecords.Count));
                page.Content().Element(c => ComposeTable(c, attendanceRecords));
                page.Footer().Element(c => ComposeFooter(c, attendanceRecords.Count));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, AttendanceOverviewReportOptions options, int totalRecords)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Orange.Darken2).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PulsePoint HR").FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("รายงานภาพรวมการลงเวลา").FontSize(12).FontColor(Colors.White);
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text($"ออกรายงานวันที่: {options.GeneratedAt:dd/MM/yyyy}").FontSize(9).FontColor(Colors.White);
                });
            });

            column.Item().Background(Colors.Grey.Lighten2).Padding(8).Row(row =>
            {
                row.RelativeItem().Text($"ฝ่าย: {options.DivisionName ?? "-"}").FontSize(10);
                row.RelativeItem().Text($"แผนก: {options.DepartmentName ?? "-"}").FontSize(10);
                row.RelativeItem().Text($"สถานะ: {options.Status ?? "-"}").FontSize(10);
                row.RelativeItem().Text($"ช่วงวันที่: {(options.StartDate?.ToString("dd/MM/yyyy") ?? "-")} - {(options.EndDate?.ToString("dd/MM/yyyy") ?? "-")}").FontSize(10);
                row.RelativeItem().AlignRight().Text($"จำนวน: {totalRecords} รายการ").FontSize(10).Bold();
            });

            column.Item().PaddingTop(5);
        });
    }

    private static void ComposeTable(IContainer container, List<AttendanceDto> records)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1.5f);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("ลำดับ").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("ชื่อพนักงาน").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("ฝ่าย").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("แผนก").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("วันที่").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("เวลาเข้า").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("เวลาออก").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("สถานะ").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Orange.Darken1).Padding(5).Text("หมายเหตุ").FontColor(Colors.White).Bold();
            });

            foreach (var record in records)
            {
                var bgColor = records.IndexOf(record) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                var index = records.IndexOf(record) + 1;

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(index.ToString());
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(record.EmployeeName ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(record.DivisionName ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(record.DepartmentName ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(record.Date.ToString("dd/MM/yyyy"));
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(record.CheckIn?.ToString("HH:mm") ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(record.CheckOut?.ToString("HH:mm") ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(record.Status);
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(GetStatusNote(record));
            }
        });
    }

    private static string GetStatusNote(AttendanceDto record)
    {
        if (!record.CheckIn.HasValue && !record.CheckOut.HasValue)
            return "ไม่ลงเวลา";
        if (record.CheckIn.HasValue && record.CheckIn.Value.TimeOfDay > new TimeSpan(9, 0, 0))
            return "มาสาย";
        if (record.CheckIn.HasValue && !record.CheckOut.HasValue)
            return "ยังไม่ลงเวลาออก";
        return "-";
    }

    private static void ComposeFooter(IContainer container, int totalCount)
    {
        container.Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
        {
            row.RelativeItem().AlignLeft().Text($"ทั้งหมด {totalCount} รายการ").FontSize(9);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("หน้า ").FontSize(9);
                text.CurrentPageNumber().FontSize(9);
                text.Span(" / ").FontSize(9);
                text.TotalPages().FontSize(9);
            });
        });
    }
}