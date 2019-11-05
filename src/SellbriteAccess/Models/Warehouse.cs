using Newtonsoft.Json;

namespace SellbriteAccess.Models
{
	public class Warehouse
	{
		public string Uuid { get; set; }
		public string Name { get; set; }
		[ JsonProperty( "inventory_master" ) ]
		public string InventoryMaster { get; set; }
		public bool Archived { get; set; }
	}
}
