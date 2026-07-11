using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HR_System.Application.DTOs.Payroll;
using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services.PdfGenerators;

public class PayrollPdfGenerator : IPayrollPdfGenerator
{
    public byte[] Generate(PayrollDto payroll, string? companyName = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var monthNamesThai = new[]
        {
            "", "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน",
            "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม",
            "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม"
        };

        var periodText = $"{monthNamesThai[payroll.Month]} {payroll.Year}";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("TH Sarabun New"));

                page.Header().Element(c => ComposeHeader(c, periodText, companyName));
                page.Content().Element(c => ComposeContent(c, payroll));
                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, string period, string? companyName)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(companyName ?? "PULSEPOINT HR").FontSize(10).FontColor(Colors.Grey.Medium).Bold();
                    c.Item().Text("HR Management System").FontSize(8).FontColor(Colors.Grey.Lighten1);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text("รายงานเงินเดือน").FontSize(9).FontColor(Colors.Grey.Medium);
                    c.Item().AlignRight().Text($"ประจำเดือน: {period}").FontSize(9).FontColor(Colors.Grey.Medium);
                    c.Item().AlignRight().Text($"วันที่: {DateTime.Now:dd MMMM yyyy}").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            col.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
        });
    }

    private static void ComposeContent(IContainer container, PayrollDto payroll)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(20).AlignCenter().Text("PAYSLIP").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(3).AlignCenter().Text("รายงานสรุปเงินเดือน").FontSize(12).FontColor(Colors.Grey.Medium);

            column.Item().PaddingTop(20).Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(15).Column(section =>
            {
                section.Item().Text("ข้อมูลพนักงาน").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
                section.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Row(r =>
                        {
                            r.ConstantColumn(100).Text("ชื่อ-นามสกุล").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(payroll.EmployeeName);
                        });
                    });
                });
            });

            column.Item().PaddingTop(15).Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(section =>
            {
                section.Item().Text("รายละเอียดเงินเดือน").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
                section.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("รายการ").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(8).AlignRight().Text("จำนวน (บาท)").FontColor(Colors.White).Bold();
                    });

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text("เงินเดือนพื้นฐาน (Basic Salary)");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).AlignRight().Text(payroll.BasicSalary.ToString("N2"));

                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text("ค่าตอบแทน (Allowance)");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).AlignRight().Text(payroll.Allowance.ToString("N2"));

                    if (payroll.UnpaidLeaveDays > 0)
                    {
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Background(Colors.Red.Lighten5).Text($"หักลากิจ/ลาป่วย ({payroll.UnpaidLeaveDays} วัน)");
                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Background(Colors.Red.Lighten5).AlignRight().Text($"-{payroll.Deduction.ToString("N2")}");
                    }

                    table.Cell().Background(Colors.Grey.Lighten4).Padding(8).Text("รวมหัก (Total Deduction)").Bold();
                    table.Cell().Background(Colors.Grey.Lighten4).Padding(8).AlignRight().Text(payroll.Deduction.ToString("N2")).Bold();

                    table.Cell().Background(Colors.Green.Lighten3).Padding(8).Text("เงินเดือนสุทธิ (Net Salary)").FontSize(13).Bold();
                    table.Cell().Background(Colors.Green.Lighten3).Padding(8).AlignRight().Text(payroll.NetSalary.ToString("N2")).FontSize(13).Bold();
                });
            });

            column.Item().PaddingTop(15).Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(section =>
            {
                section.Item().Text("สถานะ").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
                section.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Row(r =>
                        {
                            r.ConstantColumn(80).Text("สถานะ").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Background(TranslateStatusBgColor(payroll.Status)).Padding(5).Text(TranslateStatus(payroll.Status)).FontSize(11).Bold().FontColor(Colors.White);
                        });
                    });
                });
            });

            column.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().PaddingTop(8).AlignCenter().Text("เอกสารนี้ออกโดยระบบ HR System").FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text($"พิมพ์เมื่อ: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("หน้า ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static string TranslateStatus(string status)
    {
        return status switch
        {
            "Processed" => "ประมวลผลแล้ว",
            "Pending" => "รอดำเนินการ",
            "Draft" => "แบบร่าง",
            _ => status
        };
    }

    private static string TranslateStatusBgColor(string status)
    {
        return status switch
        {
            "Processed" => Colors.Green.Darken1,
            "Pending" => Colors.Orange.Darken1,
            "Draft" => Colors.Grey.Medium,
            _ => Colors.Grey.Medium
        };
    }
}
