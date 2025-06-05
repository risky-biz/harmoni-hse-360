using HarmoniHSE360.Domain.Common;
using HarmoniHSE360.Domain.Events;

namespace HarmoniHSE360.Domain.Entities;

public class PPERequest : BaseEntity, IAuditableEntity
{
    public string RequestNumber { get; private set; } = string.Empty;
    public int RequesterId { get; private set; }
    public User Requester { get; private set; } = null!;
    public int CategoryId { get; private set; }
    public PPECategory Category { get; private set; } = null!;
    public string Justification { get; private set; } = string.Empty;
    public RequestPriority Priority { get; private set; }
    public RequestStatus Status { get; private set; }
    public DateTime RequestDate { get; private set; }
    public DateTime? RequiredDate { get; private set; }
    public int? ReviewerId { get; private set; }
    public User? Reviewer { get; private set; }
    public DateTime? ReviewedDate { get; private set; }
    public DateTime? ApprovedDate { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? FulfilledDate { get; private set; }
    public string? FulfilledBy { get; private set; }
    public int? FulfilledPPEItemId { get; private set; }
    public PPEItem? FulfilledPPEItem { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<PPERequestItem> _requestItems = new();
    public IReadOnlyCollection<PPERequestItem> RequestItems => _requestItems.AsReadOnly();

    // Audit fields
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? LastModifiedAt { get; private set; }
    public string? LastModifiedBy { get; private set; }

    protected PPERequest() { } // For EF Core

    public static PPERequest Create(
        int requesterId,
        int categoryId,
        string justification,
        RequestPriority priority,
        string createdBy,
        DateTime? requiredDate = null,
        string? notes = null)
    {
        var request = new PPERequest
        {
            RequestNumber = GenerateRequestNumber(),
            RequesterId = requesterId,
            CategoryId = categoryId,
            Justification = justification,
            Priority = priority,
            Status = RequestStatus.Draft,
            RequestDate = DateTime.UtcNow,
            RequiredDate = requiredDate,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        request.AddDomainEvent(new PPERequestCreatedEvent(request));

        return request;
    }

    public void AddRequestItem(string itemDescription, string? size = null, int quantity = 1)
    {
        if (Status != RequestStatus.Draft)
            throw new InvalidOperationException("Cannot modify submitted request");

        var item = PPERequestItem.Create(Id, itemDescription, size, quantity);
        _requestItems.Add(item);
    }

    public void RemoveRequestItem(int itemId)
    {
        if (Status != RequestStatus.Draft)
            throw new InvalidOperationException("Cannot modify submitted request");

        var item = _requestItems.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _requestItems.Remove(item);
        }
    }

    public void Submit(string submittedBy)
    {
        if (Status != RequestStatus.Draft)
            throw new InvalidOperationException("Request has already been submitted");

        if (!_requestItems.Any())
            throw new InvalidOperationException("Cannot submit request without items");

        Status = RequestStatus.Submitted;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = submittedBy;

        AddDomainEvent(new PPERequestSubmittedEvent(this));
    }

    public void AssignReviewer(int reviewerId, string assignedBy)
    {
        if (Status != RequestStatus.Submitted)
            throw new InvalidOperationException("Can only assign reviewer to submitted requests");

        ReviewerId = reviewerId;
        Status = RequestStatus.UnderReview;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = assignedBy;

        AddDomainEvent(new PPERequestReviewerAssignedEvent(this, reviewerId));
    }

    public void Approve(string approvedBy, string? approvalNotes = null)
    {
        if (Status != RequestStatus.UnderReview)
            throw new InvalidOperationException("Can only approve requests under review");

        Status = RequestStatus.Approved;
        ApprovedDate = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        ReviewedDate = DateTime.UtcNow;
        
        if (!string.IsNullOrEmpty(approvalNotes))
        {
            Notes = $"{Notes}\nApproval Notes: {approvalNotes}";
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = approvedBy;

        AddDomainEvent(new PPERequestApprovedEvent(this));
    }

    public void Reject(string rejectedBy, string rejectionReason)
    {
        if (Status != RequestStatus.UnderReview)
            throw new InvalidOperationException("Can only reject requests under review");

        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("Rejection reason is required", nameof(rejectionReason));

        Status = RequestStatus.Rejected;
        RejectionReason = rejectionReason;
        ReviewedDate = DateTime.UtcNow;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = rejectedBy;

        AddDomainEvent(new PPERequestRejectedEvent(this, rejectionReason));
    }

    public void Fulfill(int ppeItemId, string fulfilledBy, string? fulfillmentNotes = null)
    {
        if (Status != RequestStatus.Approved)
            throw new InvalidOperationException("Can only fulfill approved requests");

        Status = RequestStatus.Fulfilled;
        FulfilledDate = DateTime.UtcNow;
        FulfilledBy = fulfilledBy;
        FulfilledPPEItemId = ppeItemId;

        if (!string.IsNullOrEmpty(fulfillmentNotes))
        {
            Notes = $"{Notes}\nFulfillment Notes: {fulfillmentNotes}";
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = fulfilledBy;

        AddDomainEvent(new PPERequestFulfilledEvent(this, ppeItemId));
    }

    public void Cancel(string cancelledBy, string? cancelReason = null)
    {
        if (Status == RequestStatus.Fulfilled || Status == RequestStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel request with status {Status}");

        Status = RequestStatus.Cancelled;
        
        if (!string.IsNullOrEmpty(cancelReason))
        {
            Notes = $"{Notes}\nCancellation Reason: {cancelReason}";
        }

        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = cancelledBy;

        AddDomainEvent(new PPERequestCancelledEvent(this, cancelReason));
    }

    public void UpdatePriority(RequestPriority priority, string modifiedBy, string? reason = null)
    {
        if (Status == RequestStatus.Fulfilled || Status == RequestStatus.Cancelled)
            throw new InvalidOperationException($"Cannot update priority of {Status.ToString().ToLower()} request");

        var oldPriority = Priority;
        Priority = priority;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = modifiedBy;

        if (!string.IsNullOrEmpty(reason))
        {
            Notes = $"{Notes}\nPriority changed from {oldPriority} to {priority}: {reason}";
        }

        AddDomainEvent(new PPERequestPriorityChangedEvent(this, oldPriority, priority));
    }

    // Computed Properties
    public bool IsOverdue => RequiredDate.HasValue && DateTime.UtcNow > RequiredDate.Value && 
                           Status != RequestStatus.Fulfilled && Status != RequestStatus.Cancelled;

    public bool IsUrgent => Priority == RequestPriority.Urgent || 
                          (RequiredDate.HasValue && (RequiredDate.Value - DateTime.UtcNow).TotalDays <= 1);

    public int? DaysUntilRequired => RequiredDate.HasValue 
        ? (int)(RequiredDate.Value - DateTime.UtcNow).TotalDays 
        : null;

    public TimeSpan ProcessingTime => Status switch
    {
        RequestStatus.Fulfilled => FulfilledDate!.Value - RequestDate,
        RequestStatus.Rejected => ReviewedDate!.Value - RequestDate,
        _ => DateTime.UtcNow - RequestDate
    };

    private static string GenerateRequestNumber()
    {
        return $"PPE-{DateTime.UtcNow:yyyyMMdd}-{DateTime.UtcNow.Ticks.ToString().Substring(12)}";
    }
}

public class PPERequestItem : BaseEntity
{
    public int RequestId { get; private set; }
    public PPERequest Request { get; private set; } = null!;
    public string ItemDescription { get; private set; } = string.Empty;
    public string? Size { get; private set; }
    public int Quantity { get; private set; }
    public string? SpecialRequirements { get; private set; }

    protected PPERequestItem() { } // For EF Core

    public static PPERequestItem Create(
        int requestId,
        string itemDescription,
        string? size = null,
        int quantity = 1,
        string? specialRequirements = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        return new PPERequestItem
        {
            RequestId = requestId,
            ItemDescription = itemDescription,
            Size = size,
            Quantity = quantity,
            SpecialRequirements = specialRequirements
        };
    }

    public void UpdateDetails(string itemDescription, string? size, int quantity, string? specialRequirements)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        ItemDescription = itemDescription;
        Size = size;
        Quantity = quantity;
        SpecialRequirements = specialRequirements;
    }
}

public enum RequestStatus
{
    Draft = 1,
    Submitted = 2,
    UnderReview = 3,
    Approved = 4,
    Rejected = 5,
    Fulfilled = 6,
    Cancelled = 7
}

public enum RequestPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}