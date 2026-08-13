using Whalebone.Content.D15;

var data = D15BuildingPortBatchCatalog.Create();
data.Validate();
Console.WriteLine($"D15-A04 catalog PASS types={data.Definitions.Count} variants={data.Variants.Count} instances={data.Instances.Count}");
