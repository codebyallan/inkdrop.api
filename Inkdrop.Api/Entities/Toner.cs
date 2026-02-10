using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public class Toner : Base, ISoftDeletable
{
    public string Model { get; private set; } = string.Empty;
    public string Manufacturer { get; private set; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public int Quantity { get; private set; } = 0;
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
        Model = model;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateManufacturer(string manufacturer)
    {
        if (Manufacturer == manufacturer) return;
        ValidateManufacturer(manufacturer);
        Manufacturer = manufacturer;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Out(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (quantity > Quantity) throw new InvalidOperationException("Not enough toner in stock.");
        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    public void In(int quantity)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }
    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        DeletedAt = DateTime.UtcNow;
    }
    private static void ValidateModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("Toner model cannot be null or empty.", nameof(model));
        if (model.Length < 3 || model.Length > 100) throw new ArgumentException("Toner model must be between 3 and 100 characters.", nameof(model));
    }
    private static void ValidateManufacturer(string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer)) throw new ArgumentException("Toner manufacturer cannot be null or empty.", nameof(manufacturer));
        if (manufacturer.Length < 2 || manufacturer.Length > 100) throw new ArgumentException("Toner manufacturer must be between 2 and 100 characters.", nameof(manufacturer));
    }
    private static void ValidateColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) throw new ArgumentException("Toner color cannot be null or empty.", nameof(color));
        if (color.Length < 3 || color.Length > 50) throw new ArgumentException("Toner color must be between 3 and 50 characters.", nameof(color));
    }
    private static void Validate(string model, string manufacturer, string color)
    {
        ValidateModel(model);
        ValidateManufacturer(manufacturer);
        ValidateColor(color);
    }
}