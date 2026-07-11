using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HR_System.Application.DTOs.Reports;
using HR_System.Application.Interfaces;

namespace HR_System.Infrastructure.Services.PdfGenerators;

public class LeaveCertificatePdfGenerator : ILeaveCertificatePdfGenerator
{
    public byte[] Generate(LeaveCertificateDto certificate)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Portrait());
                page.Margin(35);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("TH Sarabun New"));

                page.Header().Element(c => ComposeHeader(c));
                page.Content().Element(c => ComposeContent(c, certificate));
                page.Footer().Element(c => ComposeFooter(c));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("PULSEPOINT HR").FontSize(10).FontColor(Colors.Grey.Medium).Bold();
                    c.Item().Text("HR Management System").FontSize(8).FontColor(Colors.Grey.Lighten1);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text("เอกสารทางการ").FontSize(9).FontColor(Colors.Grey.Medium);
                    c.Item().AlignRight().Text($"วันที่: {DateTime.Now:dd MMMM yyyy}").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            col.Item().PaddingTop(10).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
        });
    }

    private static void ComposeContent(IContainer container, LeaveCertificateDto certificate)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(20).AlignCenter().Text("หนังสือรับรองการลา").FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(3).AlignCenter().Text("ใบรับรองการลาออกจากงาน").FontSize(12).FontColor(Colors.Grey.Medium);

            column.Item().PaddingTop(25).Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(15).Column(section =>
            {
                section.Item().Text("ข้อมูลพนักงาน").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
                section.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Row(r =>
                        {
                            r.ConstantColumn(80).Text("ชื่อ-นามสกุล").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(certificate.EmployeeName);
                        });
                        info.Item().PaddingTop(5).Row(r =>
                        {
                            r.ConstantColumn(80).Text("ตำแหน่ง").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(string.IsNullOrEmpty(certificate.PositionName) ? "-" : certificate.PositionName);
                        });
                    });
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Row(r =>
                        {
                            r.ConstantColumn(60).Text("แผนก").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(string.IsNullOrEmpty(certificate.DepartmentName) ? "-" : certificate.DepartmentName);
                        });
                        info.Item().PaddingTop(5).Row(r =>
                        {
                            r.ConstantColumn(60).Text("ฝ่าย").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(string.IsNullOrEmpty(certificate.DivisionName) ? "-" : certificate.DivisionName);
                        });
                    });
                });
            });

            column.Item().PaddingTop(15).Background(Colors.Grey.Lighten5).Border(1).BorderColor(Colors.Grey.Lighten3).Padding(15).Column(section =>
            {
                section.Item().Text("รายละเอียดการลา").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);
                section.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Row(r =>
                        {
                            r.ConstantColumn(90).Text("ประเภทการลา").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(TranslateLeaveType(certificate.LeaveType)).Bold();
                        });
                        info.Item().PaddingTop(5).Row(r =>
                        {
                            r.ConstantColumn(90).Text("สาเหตุการลา").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(string.IsNullOrEmpty(certificate.Reason) ? "-" : certificate.Reason);
                        });
                    });
                    row.RelativeItem().Column(info =>
                    {
                        info.Item().Row(r =>
                        {
                            r.ConstantColumn(70).Text("วันที่เริ่ม").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(certificate.StartDate.ToString("dd/MM/yyyy"));
                        });
                        info.Item().PaddingTop(5).Row(r =>
                        {
                            r.ConstantColumn(70).Text("วันสิ้นสุด").Bold().FontColor(Colors.Grey.Medium);
                            r.RelativeItem().Text(certificate.EndDate.ToString("dd/MM/yyyy"));
                        });
                    });
                    row.ConstantColumn(80).Column(info =>
                    {
                        info.Item().AlignCenter().Background(TranslateStatusBgColor(certificate.Status)).Padding(8).Text(TranslateStatus(certificate.Status)).FontSize(11).Bold().FontColor(Colors.White);
                        info.Item().PaddingTop(5).AlignCenter().Text($"{certificate.Days} วัน").FontSize(12).Bold();
                    });
                });
            });

            column.Item().PaddingTop(15).Column(section =>
            {
                section.Item().Text("ผู้อนุมัติ").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);

                if (certificate.Approvers == null || certificate.Approvers.Count == 0)
                {
                    section.Item().PaddingTop(10).Background(Colors.Grey.Lighten5).Padding(15).AlignCenter().Text("ยังไม่มีผู้อนุมัติ").FontColor(Colors.Grey.Medium);
                }
                else
                {
                    section.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(35);
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("ลำดับ").FontColor(Colors.White).Bold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("ชื่อผู้อนุมัติ").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("บทบาท").FontColor(Colors.White).Bold();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("สถานะ").FontColor(Colors.White).Bold().AlignCenter();
                            header.Cell().Background(Colors.Blue.Darken2).Padding(8).Text("วันที่").FontColor(Colors.White).Bold().AlignCenter();
                        });

                        foreach (var approver in certificate.Approvers)
                        {
                            var bgColor = certificate.Approvers.IndexOf(approver) % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text(approver.StepNumber.ToString());
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(string.IsNullOrEmpty(approver.ApproverName) ? "-" : approver.ApproverName);
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).Text(approver.ApproverRole);
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text(TranslateStatus(approver.Status));
                            table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text(approver.ActionAt?.ToString("dd/MM/yyyy") ?? "-");
                        }
                    });
                }
            });

            column.Item().PaddingTop(30).Column(sigCol =>
            {
                sigCol.Item().PaddingTop(20).AlignCenter().Text("ลงนาม").FontSize(13).Bold().FontColor(Colors.Blue.Darken2);

                var approverCount = (certificate.Approvers?.Count ?? 0) + 1;
                sigCol.Item().PaddingTop(15).Row(sigRow =>
                {
                    sigRow.RelativeItem().Column(sig =>
                    {
                        sig.Item().PaddingBottom(-10).AlignCenter().Text(certificate.EmployeeName).FontSize(10);
                        sig.Item().AlignCenter().Text("_________________________").FontSize(10);
                        sig.Item().AlignCenter().Text($"(ผู้ร้องขอ - วันที่ขอ: {certificate.CreatedAt:dd/MM/yyyy})").FontSize(9).FontColor(Colors.Grey.Medium);
                        sig.Item().AlignCenter().Text(certificate.EmployeeName).FontSize(10).Bold();
                    });

                    if (certificate.Approvers != null)
                    {
                        foreach (var approver in certificate.Approvers)
                        {
                            sigRow.RelativeItem().Column(sig =>
                            {
                                if ((approver.Status == "Approved" || approver.Status == "Rejected") && !string.IsNullOrEmpty(approver.ApproverName))
                                {
                                    sig.Item().PaddingBottom(-10).AlignCenter().Text(approver.ApproverName).FontSize(10);
                                    sig.Item().AlignCenter().Text("_________________________").FontSize(10);
                                    sig.Item().AlignCenter().Text($"(ผู้อนุมัติ #{approver.StepNumber} - {TranslateStatus(approver.Status)} {approver.ActionAt:dd/MM/yyyy})").FontSize(9).FontColor(Colors.Grey.Medium);
                                    sig.Item().AlignCenter().Text(approver.ApproverName).FontSize(10).Bold();
                                }
                                else
                                {
                                    sig.Item().AlignCenter().Text("_________________________").FontSize(10);
                                    sig.Item().AlignCenter().Text($"(ผู้อนุมัติ #{approver.StepNumber} - {TranslateStatus(approver.Status)})").FontSize(9).FontColor(Colors.Grey.Medium);
                                    sig.Item().AlignCenter().Text(approver.ApproverName).FontSize(10).Bold();
                                }
                            });
                        }
                    }
                });
            });

            column.Item().PaddingTop(25).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().PaddingTop(8).AlignCenter().Text("เอกสารนี้ออกโดยระบบ HR System • หากมีข้อสงสัยกรุณาติดต่อฝ่ายบุคคล").FontSize(8).FontColor(Colors.Grey.Medium);
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

    private static string TranslateStatus(string status)
    {
        return status switch
        {
            "Pending" => "รอดำเนินการ",
            "Approved" => "อนุมัติ",
            "Rejected" => "ไม่อนุมัติ",
            "Cancelled" => "ยกเลิก",
            _ => status
        };
    }

    private static string TranslateStatusBgColor(string status)
    {
        return status switch
        {
            "Pending" => Colors.Orange.Darken1,
            "Approved" => Colors.Green.Darken1,
            "Rejected" => Colors.Red.Darken1,
            "Cancelled" => Colors.Grey.Medium,
            _ => Colors.Grey.Medium
        };
    }
}
