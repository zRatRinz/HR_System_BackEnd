using HR_System.Application.DTOs.Reports;

namespace HR_System.Application.Interfaces;

public interface ILeaveCertificatePdfGenerator
{
    byte[] Generate(LeaveCertificateDto certificate);
}
