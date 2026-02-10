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
            PrinterId = Type == "OUT" ? printerId : null;
            Quantity = quantity;
            Description = description;
            Type = type.ToUpper();
        }
        private static void Validate(Guid tonerId, Guid? printerId, int quantity, string? description, string type)
        {
            if (tonerId == Guid.Empty) throw new ArgumentException("TonerId cannot be empty.");
            if (printerId.HasValue && printerId.Value == Guid.Empty) throw new ArgumentException("PrinterId cannot be empty if provided.");
            if (description != null && (description.Length < 5 || description.Length > 100)) throw new ArgumentException("Description must be between 5 and 100 characters.");
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.");
            if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type cannot be empty.");
            string movementType = type.ToUpper();
            if (movementType != "IN" && movementType != "OUT") throw new ArgumentException("Type must be either 'IN' or 'OUT'.");
            if (movementType == "OUT" && !printerId.HasValue) throw new ArgumentException("A PrinterId must be provided for stock OUT movements.");
        }
    }
}