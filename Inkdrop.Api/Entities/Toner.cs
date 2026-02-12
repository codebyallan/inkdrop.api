using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public class Toner : Base, ISoftDeletable, IUpdatable
{
    public string Model { get; private set; } = string.Empty;
    public string Manufacturer { get; private set; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public int Quantity { get; private set; } = 0;
    public DateTime? UpdatedAt { get; protected set; } = null;
    public DateTime? DeletedAt { get; private set; } = null;
    private Toner() { }
    public Toner(string model, string manufacturer, string color)
    {
        Validate(model, manufacturer, color);
        Model = model;
        Manufacturer = manufacturer;
        Color = color;
    }
    public void UpdateModel(string model)
    {
        if (Model == model) return;
        ValidateModel(model);
        if (!IsValid) return;
        Model = model;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateManufacturer(string manufacturer)
    {
        if (Manufacturer == manufacturer) return;
        ValidateManufacturer(manufacturer);
        if (!IsValid) return;
        Manufacturer = manufacturer;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Out(int quantity)
    {
        if (quantity <= 0) AddNotification("TonerQuantityInvalid", "Quantity must be greater than zero.");
        if (quantity > Quantity) AddNotification("TonerInsufficientStock", "Not enough toner in stock.");
        if (!IsValid) return;
        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    public void In(int quantity)
    {
        if (quantity <= 0) AddNotification("TonerQuantityInvalid", "Quantity must be greater than zero.");
        if (!IsValid) return;
        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        DeletedAt = DateTime.UtcNow;
    }
    private void ValidateModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            AddNotification("TonerModelInvalid", "Toner model cannot be null or empty.");
            return;
        }
        if (model.Length < 3 || model.Length > 100) AddNotification("TonerModelLengthInvalid", "Toner model must be between 3 and 100 characters.");
    }
    private void ValidateManufacturer(string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer))
        {
            AddNotification("TonerManufacturerInvalid", "Toner manufacturer cannot be null or empty.");
            return;
        }
        if (manufacturer.Length < 2 || manufacturer.Length > 100) AddNotification("TonerManufacturerLengthInvalid", "Toner manufacturer must be between 2 and 100 characters.");
    }
    private void ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            AddNotification("TonerColorInvalid", "Toner color cannot be null or empty.");
            return;
        }
        if (color.Length < 3 || color.Length > 50) AddNotification("TonerColorLengthInvalid", "Toner color must be between 3 and 50 characters.");
    }
    private void Validate(string model, string manufacturer, string color)
    {
        ValidateModel(model);
        ValidateManufacturer(manufacturer);
        ValidateColor(color);
    }
}