using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HR_System.Application.DTOs.Employee;
using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services.PdfGenerators;

public class EmployeeReportPdfGenerator : IEmployeeReportPdfGenerator
{
    public byte[] Generate(List<EmployeeListDto> employees, EmployeeReportOptions options)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("TH Sarabun New"));

                page.Header().Element(c => ComposeHeader(c, options, employees.Count));
                page.Content().Element(c => ComposeTable(c, employees));
                page.Footer().Element(c => ComposeFooter(c, employees.Count));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, EmployeeReportOptions options, int totalEmployees)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Blue.Darken2).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("PulsePoint HR").FontSize(18).Bold().FontColor(Colors.White);
                    col.Item().Text("รายงานข้อมูลพนักงาน").FontSize(12).FontColor(Colors.White);
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
                row.RelativeItem().AlignRight().Text($"จำนวนพนักงานทั้งหมด: {totalEmployees} คน").FontSize(10).Bold();
            });

            column.Item().PaddingTop(5);
        });
    }

    private static void ComposeTable(IContainer container, List<EmployeeListDto> employees)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("ลำดับ").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("ชื่อพนักงาน").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("ฝ่าย").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("แผนก").FontColor(Colors.White).Bold();
                header.Cell().Background(Colors.Blue.Darken1).Padding(6).Text("ตำแหน่ง").FontColor(Colors.White).Bold();
            });

            foreach (var emp in employees)
            {
                var bgColor = employees.IndexOf(emp) % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                var index = employees.IndexOf(emp) + 1;

                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).AlignCenter().Text(index.ToString());
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(emp.Name ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(emp.DivisionName ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(emp.DepartmentName ?? "-");
                table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(emp.PositionName ?? "-");
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