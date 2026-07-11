using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HR_System.Application.DTOs.Leave;
using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services.PdfGenerators;

public class LeaveReportPdfGenerator : ILeaveReportPdfGenerator
{
    public byte[] GenerateMyLeave(List<LeaveRequestDto> leaveRecords, MyLeaveReportOptions options)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("TH Sarabun New"));

                page.Header().Element(c => ComposeHeader(c, options, leaveRecords.Count));
                page.Content().Element(c => ComposeContent(c, options, leaveRecords));
                page.Footer().Element(c => ComposeFooter(c, leaveRecords.Count));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, MyLeaveReportOptions options, int totalRecords)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Blue.Darken2).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PulsePoint HR").FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("รายงานการลาของฉัน").FontSize(12).FontColor(Colors.White);
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

    private static void ComposeSummaryBox(IContainer container, MyLeaveReportOptions options)
    {
        container.Column(column =>
        {
            column.Item().Element(c => ComposeSummaryContent(c, options));
            column.Item().PaddingTop(10);
        });
    }

    private static void ComposeContent(IContainer container, MyLeaveReportOptions options, List<LeaveRequestDto> records)
    {
        container.Column(column =>
        {
            column.Item().Element(c => ComposeSummaryBox(c, options));
            column.Item().Element(c => ComposeTableContent(c, records));
        });
    }

    private static void ComposeSummaryContent(IContainer container, MyLeaveReportOptions options)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Column(column =>
        {
            column.Item().Background(Colors.Blue.Lighten5).Padding(8).Text("สรุปวันลา").FontSize(12).Bold();

            column.Item().Padding(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Text("ประเภท").Bold();
                table.Cell().Text("ลงวัน").Bold();
                table.Cell().Text("ใช้ไป").Bold();
                table.Cell().Text("เหลือ").Bold();

                table.Cell().Text("ลาพักร้อน:");
                table.Cell().Text($"{options.AnnualTotal} วัน");
                table.Cell().Text($"{options.AnnualUsed} วัน");
                table.Cell().Text($"{options.AnnualBalance} วัน");

                table.Cell().Text("ลาป่วย:");
                table.Cell().Text($"{options.SickTotal} วัน");
                table.Cell().Text($"{options.SickUsed} วัน");
                table.Cell().Text($"{options.SickBalance} วัน");

                table.Cell().ColumnSpan(4).PaddingTop(5).BorderTop(1).BorderColor(Colors.Grey.Lighten2);
                table.Cell().Text("รวมวันลาที่ใช้:").Bold();
                table.Cell().Text($"{options.TotalUsedDays} วัน").Bold();
                table.Cell().Text("เหลือวันลาทั้งหมด:").Bold();
                table.Cell().Text($"{options.TotalBalance} วัน").Bold();
            });
        });
    }

    private static void ComposeTableContent(IContainer container, List<LeaveRequestDto> records)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("ลำดับ").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("ประเภทลา").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("วันที่เริ่ม").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("วันสิ้นสุด").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("จำนวนวัน").FontColor(Colors.White).Bold();
            });

            foreach (var record in records)
            {
                var bgColor = records.IndexOf(record) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                var index = records.IndexOf(record) + 1;

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(index.ToString());
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(TranslateLeaveType(record.LeaveType));
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(record.StartDate.ToString("dd/MM/yyyy"));
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(record.EndDate.ToString("dd/MM/yyyy"));
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(record.Days.ToString());
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

    private static string TranslateLeaveType(string leaveType)
    {
        return leaveType switch
        {
            "Annual" => "ลาพักร้อน",
            "Sick" => "ลาป่วย",
            "Personal" => "ลากิจ",
            "Maternity" => "ลาคลอด",
            "Paternity" => "ลาบุตร",
            "Bereavement" => "ลาศึกษาพิเศษ",
            _ => leaveType
        };
    }
}
