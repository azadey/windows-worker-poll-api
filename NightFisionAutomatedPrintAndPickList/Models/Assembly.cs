public class Assembly
{
    public const string AutomatedPrintService = "Automated Print Service";

    public string? AssemblyNumber { get; set; }

    public string? AssemblyStatus { get; set; }

    public DateTime? AssembleBy { get; set; }

    public string? SalesOrderNumber { get; set; }

    public string? Quantity { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedOn { get; set; }

    public Product? Product { get; set; }

    public AssemblyLines[]? AssemblyLines { get; set; }

    public SourceWarehouse? SourceWarehouse { get; set; }

    public DestinationWarehouse? DestinationWarehouse { get; set; }
}