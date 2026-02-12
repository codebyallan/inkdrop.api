namespace Inkdrop.Api.Entities
{
    public class Movements : Base
    {
        public Guid TonerId { get; init; }
        public Guid? PrinterId { get; init; }
        public int Quantity { get; init; }
        public string? Description { get; init; }
        public string Type { get; init; } = string.Empty;
        private Movements() { }
        public Movements(Guid tonerId, Guid? printerId, int quantity, string? description, string type)
        {
            Validate(tonerId, printerId, quantity, description, type);
            TonerId = tonerId;
            Type = type.ToUpper();
            PrinterId = Type == "OUT" ? printerId : null;
            Quantity = quantity;
            Description = description;
        }
        private void Validate(Guid tonerId, Guid? printerId, int quantity, string? description, string type)
        {
            if (tonerId == Guid.Empty) AddNotification("MovementsTonerIdInvalid", "TonerId cannot be empty.");
            if (printerId.HasValue && printerId.Value == Guid.Empty) AddNotification("MovementsPrinterIdInvalid", "PrinterId cannot be empty if provided.");
            if (description != null && (description.Length < 5 || description.Length > 100)) AddNotification("MovementsDescriptionLengthInvalid", "Description must be between 5 and 100 characters.");
            if (quantity <= 0) AddNotification("MovementsQuantityInvalid", "Quantity must be greater than zero.");
            if (string.IsNullOrWhiteSpace(type))
            {
                AddNotification("MovementsTypeInvalid", "Type cannot be empty.");
                return;
            }
            string movementType = type.ToUpper();
            if (movementType != "IN" && movementType != "OUT") AddNotification("MovementsTypeInvalid", "Type must be either 'IN' or 'OUT'.");
            if (movementType == "OUT" && !printerId.HasValue) AddNotification("MovementsPrinterIdRequiredForOutMovement", "A PrinterId must be provided for stock OUT movements.");
        }
    }
}