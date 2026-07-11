using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HR_System.Application.DTOs.Attendance;
using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services.PdfGenerators;

public class MyAttendanceReportPdfGenerator : IMyAttendanceReportPdfGenerator
{
    public byte[] Generate(List<AttendanceDto> attendanceRecords, MyAttendanceReportOptions options)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("TH Sarabun New"));

                page.Header().Element(c => ComposeHeader(c, options, attendanceRecords.Count));
                page.Content().Element(c => ComposeContent(c, options, attendanceRecords));
                page.Footer().Element(c => ComposeFooter(c, attendanceRecords.Count));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, MyAttendanceReportOptions options, int totalRecords)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Green.Darken2).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PulsePoint HR").FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("รายงานการลงเวลาของฉัน").FontSize(12).FontColor(Colors.White);
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text($"ออกรายงานวันที่: {options.GeneratedAt:dd/MM/yyyy}").FontSize(9).FontColor(Colors.White);
                });
            });

            column.Item().Background(Colors.Grey.Lighten2).Padding(8).Row(row =>
            {
                row.RelativeItem().Text($"ชื่อ: {options.EmployeeName ?? "-"}").FontSize(10);
                row.RelativeItem().Text($"ช่วงวันที่: {(options.StartDate?.ToString("dd/MM/yyyy") ?? "-")} - {(options.EndDate?.ToString("dd/MM/yyyy") ?? "-")}").FontSize(10);
                row.RelativeItem().AlignRight().Text($"จำนวนรายการ: {totalRecords} รายการ").FontSize(10).Bold();
            });

            column.Item().PaddingTop(5);
        });
    }

    private static void ComposeContent(IContainer container, MyAttendanceReportOptions options, List<AttendanceDto> records)
    {
        container.Column(column =>
        {
            column.Item().Element(c => ComposeSummaryBox(c, options));
            column.Item().PaddingTop(10).Element(c => ComposeTable(c, records));
        });
    }

    private static void ComposeSummaryBox(IContainer container, MyAttendanceReportOptions options)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Column(column =>
        {
            column.Item().Background(Colors.Blue.Lighten5).Padding(8).Text("สรุปรายงาน").FontSize(12).Bold();

            column.Item().Padding(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Text("เข้างานตรงเวลา: ").Bold();
                table.Cell().Text($"{options.OnTimeCount} วัน");

                table.Cell().Text("มาสาย: ").Bold();
                table.Cell().Text($"{options.LateCount} วัน");

                table.Cell().Text("ขาดงาน: ").Bold();
                table.Cell().Text($"{options.AbsentCount} วัน");

                table.Cell().Text("ลาป่วย: ").Bold();
                table.Cell().Text($"{options.SickLeaveDays} วัน");

                table.Cell().Text("ลากิจ: ").Bold();
                table.Cell().Text($"{options.PersonalLeaveDays} วัน");
            });
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
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Green.Darken1).Padding(6).Text("ลำดับ").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Green.Darken1).Padding(6).Text("วันที่").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Green.Darken1).Padding(6).Text("เวลาเข้า").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Green.Darken1).Padding(6).Text("เวลาออก").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Green.Darken1).Padding(6).Text("สถานะ").FontColor(Colors.White).Bold();
            });

            foreach (var record in records)
            {
                var bgColor = records.IndexOf(record) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                var index = records.IndexOf(record) + 1;

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(index.ToString());
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(record.Date.ToString("dd/MM/yyyy"));
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(record.CheckIn?.ToString("HH:mm") ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(record.CheckOut?.ToString("HH:mm") ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(record.Status);
            }
        });
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