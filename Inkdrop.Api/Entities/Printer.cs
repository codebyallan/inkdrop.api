using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public class Printer : Base, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string Manufacturer { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public Guid LocationId { get; private set; }
    public DateTime? DeletedAt { get; private set; } = null;
    private Printer() { }
    public Printer(string name, string model, string manufacturer, string ipAddress, Guid locationId)
    {
        Validate(name, model, manufacturer, ipAddress, locationId);
        Name = name;
        Model = model;
        Manufacturer = manufacturer;
        IpAddress = ipAddress;
        LocationId = locationId;
    }
    public void UpdateName(string name)
    {
        if (Name == name) return;
        ValidateName(name);
        Name = name;
        UpdatedAt = DateTime.UtcNow;
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
    public void UpdateIpAddress(string ipAddress)
    {
        if (IpAddress == ipAddress) return;
        ValidateIpAddress(ipAddress);
        IpAddress = ipAddress;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateLocationId(Guid locationId)
    {
        if (LocationId == locationId) return;
        ValidateLocationId(locationId);
        LocationId = locationId;
        UpdatedAt = DateTime.UtcNow;
    }
    public void SetActiveStatus(bool isActive)
    {
        if (IsActive == isActive) return;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }
    public void MarkAsDeleted()
    {
        if (DeletedAt != null) return;
        IsActive = false;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Printer name cannot be empty.", nameof(name));
        if (name.Length < 3 || name.Length > 100) throw new ArgumentException("Printer name must be between 3 and 100 characters.", nameof(name));
    }
    private static void ValidateModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("Printer model cannot be empty.", nameof(model));
        if (model.Length < 3 || model.Length > 100) throw new ArgumentException("Printer model must be between 3 and 100 characters.", nameof(model));
    }
    private static void ValidateManufacturer(string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer)) throw new ArgumentException("Printer manufacturer cannot be empty.", nameof(manufacturer));
        if (manufacturer.Length < 2 || manufacturer.Length > 100) throw new ArgumentException("Printer manufacturer must be between 2 and 100 characters.", nameof(manufacturer));
    }
    private static void ValidateIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress)) throw new ArgumentException("Printer IP address cannot be empty.", nameof(ipAddress));
        if (ipAddress.Length < 7 || ipAddress.Length > 45) throw new ArgumentException("Printer IP address must be between 7 and 45 characters.", nameof(ipAddress));
    }
    private static void ValidateLocationId(Guid locationId)
    {
        if (locationId == Guid.Empty) throw new ArgumentException("Location ID cannot be empty.", nameof(locationId));
    }
    private static void Validate(string name, string model, string manufacturer, string ipAddress, Guid locationId)
    {
        ValidateName(name);
        ValidateModel(model);
        ValidateManufacturer(manufacturer);
        ValidateIpAddress(ipAddress);
        ValidateLocationId(locationId);
    }
}