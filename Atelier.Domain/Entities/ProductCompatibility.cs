public class ProductCompatibility : Entity
{
    public int ProductId { get; private set; }
    public int VehicleId { get; private set; }
    public bool RequiresVinCheck { get; private set; }
    public string? Note { get; private set; }
    public Vehicle Vehicle { get; private set; } = null!;

    protected ProductCompatibility() { }

    public ProductCompatibility(int vehicleId, bool requiresVinCheck = false, string? note = null)
    {
        VehicleId = vehicleId;
        RequiresVinCheck = requiresVinCheck;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}
