namespace Gulora.Contracts.Orders;

public record RequestReturnRequest(string Reason, List<Guid>? ItemIds = null);
