namespace elusoft.eluCloud.Client.Model;

public class PartEnd
{
    public string? Timestamp { get; set; }
    public List<PartDto>? Parts { get; set; }
}

public class PartDto
{
    public int PartId { get; set; }
    public int MachineId { get; set; }
    public int ProgramId { get; set; }
    public int BarId { get; set; }
}
