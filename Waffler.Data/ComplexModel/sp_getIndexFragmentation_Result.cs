#pragma warning disable IDE1006 // Naming Styles
namespace Waffler.Data.ComplexModel
{
    public class sp_getIndexFragmentation_Result
	{
		public string SchemaName { get; set; }
		public string TableName { get; set; }
		public string IndexName { get; set; }
		public decimal Fragmentation { get; set; }
		public long PageCount { get; set; }
	}
}