namespace elusoft.eluCloud.Client.Model;

public class Part
{
    /// <summary>
    /// Flag informing whether the part is still being processed.
    /// </summary>
    public bool Active { get; set; }


    /// <summary>
    /// Barcode sent from the machine.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// ID of the bar that the part belongs to.
    /// </summary>
    public int BarId { get; set; }

    /// <summary>
    /// Auxiliary processing time of the part.
    /// </summary>
    public long? AuxprocessingTime { get; set; }

    /// <summary>
    /// Identifier of the part sent from the machine.
    /// </summary>
    public string? IdentNo { get; set; }

    /// <summary>
    /// Nonproductive time that occured during processing.
    /// </summary>
    public long? NonproductiveTime { get; set; }

    /// <summary>
    /// Job identifier sent from the machine.
    /// </summary>
    public string? JobName { get; set; }

    /// <summary>
    /// ID of the machine that processed the part.
    /// </summary>
    public int MachineId { get; set; }

    /// <summary>
    /// Name of the machine that processed the part.
    /// </summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Timestamp when the part was processed.
    /// </summary>
    public DateTime? Processed { get; set; }

    /// <summary>
    /// Processing time of the part.
    /// </summary>
    public long? ProcessingTime { get; set; }

    /// <summary>
    /// Idle time that occured during processing.
    /// </summary>
    public long? IdleTime { get; set; }

    /// <summary>
    /// ID of the program that was executed.
    /// </summary>
    public int ProgramId { get; set; }

    /// <summary>
    /// Name of the program that was executed.
    /// </summary>
    public string? ProgramName { get; set; }

    /// <summary>
    /// This field is currently not in use.
    /// </summary>
    public DateTime? Scanned { get; set; }

    /// <summary>
    /// Timestamp when the part processing started.
    /// </summary>
    public DateTime? Started { get; set; }

    /// <summary>
    /// Station number where the part was processed on.
    /// </summary>
    public short? Station { get; set; }

    /// <summary>
    /// Total processing time of the part. This is a sum of every individual time.
    /// </summary>
    public long? TotalTime { get; set; }

    /// <summary>
    /// Average feed value of the machine during processing.
    /// </summary>
    public double? AvgFeed { get; set; }
    
    /// <summary>
    /// Processing end code
    /// </summary>
    public CncEndCode? EndCode { get; set; }
}
