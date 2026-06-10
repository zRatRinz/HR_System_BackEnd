using HR_System.Application.DTOs.Approval;
using HR_System.Application.Interfaces;
using HR_System.Domain.Entities;
using HR_System.Domain.Enums;

namespace HR_System.Application.UseCases.Approval;

public class ApprovalUseCase
{
    private readonly IApprovalRepository _approvalRepository;
    private readonly IScopeService _scopeService;

    public ApprovalUseCase(IApprovalRepository approvalRepository, IScopeService scopeService)
    {
        _approvalRepository = approvalRepository;
        _scopeService = scopeService;
    }

    public async Task<ApprovalListResponse> GetAllAsync(string? status)
    {
        ApprovalStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status))
        {
            statusEnum = Enum.Parse<ApprovalStatus>(status, true);
        }

        var role = _scopeService.GetRole();
        var divisionId = _scopeService.GetDivisionId();
        var departmentId = _scopeService.GetDepartmentId();
        var userId = _scopeService.GetUserId();

        var bypassScope = role == "Admin" || role == "HR";

        List<ApprovalItemDto> items;

        if (bypassScope)
        {
            items = await _approvalRepository.GetAllAsDtoAsync(statusEnum);
        }
        else
        {
            var (dbItems, _) = await _approvalRepository.GetAllAsync(statusEnum, divisionId, departmentId, role, userId);
            items = dbItems;
        }

        return new ApprovalListResponse
        {
            Data = items
        };
    }

    public async Task<ApprovalItemDto> UpdateStatusAsync(Guid id, UpdateApprovalRequest request)
    {
        var itemDto = (await _approvalRepository.GetAllAsDtoAsync(null)).FirstOrDefault(x => x.Id == id);
        if (itemDto == null)
        {
            throw new KeyNotFoundException("Approval item not found");
        }

        var item = new ApprovalItem
        {
            Id = id,
            EmployeeId = itemDto.EmployeeId,
            Status = Enum.Parse<ApprovalStatus>(request.Status, true)
        };

        await _approvalRepository.UpdateAsync(item);

        itemDto.Status = request.Status.ToLower();
        return itemDto;
    }
}
