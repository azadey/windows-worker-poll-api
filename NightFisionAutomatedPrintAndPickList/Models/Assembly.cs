public class Assembly
{
    public const string AutomatedPrintService = "Automated Print Service";

    public string? AssemblyNumber { get; set; }

    public string? SalesOrderNumber { get; set; }

    public string? Quantity { get; set; }

    public Product? Product { get; set; }
}