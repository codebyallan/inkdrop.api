using Inkdrop.Api.Interfaces;

namespace Inkdrop.Api.Entities;

public class Printer : Base, ISoftDeletable, IUpdatable
{
    public string Name { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string Manufacturer { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public Guid LocationId { get; private set; }
    public DateTime? UpdatedAt { get; protected set; } = null;
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
        if (!IsValid) return;
        Name = name;
        UpdatedAt = DateTime.UtcNow;
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
    public void UpdateIpAddress(string ipAddress)
    {
        if (IpAddress == ipAddress) return;
        ValidateIpAddress(ipAddress);
        if (!IsValid) return;
        IpAddress = ipAddress;
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateLocationId(Guid locationId)
    {
        if (LocationId == locationId) return;
        ValidateLocationId(locationId);
        if (!IsValid) return;
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
    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddNotification("PrinterNameInvalid", "Printer name cannot be empty.");
            return;
        }
        if (name.Length < 3 || name.Length > 100) AddNotification("PrinterNameLengthInvalid", "Printer name must be between 3 and 100 characters.");
    }
    private void ValidateModel(string model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            AddNotification("PrinterModelInvalid", "Printer model cannot be empty.");
            return;
        }
        if (model.Length < 3 || model.Length > 100) AddNotification("PrinterModelLengthInvalid", "Printer model must be between 3 and 100 characters.");
    }
    private void ValidateManufacturer(string manufacturer)
    {
        if (string.IsNullOrWhiteSpace(manufacturer))
        {
            AddNotification("PrinterManufacturerInvalid", "Printer manufacturer cannot be empty.");
            return;
        }
        if (manufacturer.Length < 2 || manufacturer.Length > 100) AddNotification("PrinterManufacturerLengthInvalid", "Printer manufacturer must be between 2 and 100 characters.");
    }
    private void ValidateIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            AddNotification("PrinterIpAddressInvalid", "Printer IP address cannot be empty.");
            return;
        }
        if (ipAddress.Length < 7 || ipAddress.Length > 45) AddNotification("PrinterIpAddressLengthInvalid", "Printer IP address must be between 7 and 45 characters.");
    }
    private void ValidateLocationId(Guid locationId)
    {
        if (locationId == Guid.Empty) AddNotification("PrinterLocationIdInvalid", "Location ID cannot be empty.");
    }
    private void Validate(string name, string model, string manufacturer, string ipAddress, Guid locationId)
    {
        ValidateName(name);
        ValidateModel(model);
        ValidateManufacturer(manufacturer);
        ValidateIpAddress(ipAddress);
        ValidateLocationId(locationId);
    }
}