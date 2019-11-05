using Newtonsoft.Json;

namespace SellbriteAccess.Models.Requests
{
	public class UpdateSkusInventoryRequest
	{
		[ JsonProperty( "inventory" ) ]
		public UpdateSkuInventoryRequest[] Requests { get; set; }
	}

	public class UpdateSkuInventoryRequest
	{
		[ JsonProperty( "warehouse_uuid" ) ]
		public string WarehouseUuid { get; set; }
		[ JsonProperty( "sku" ) ]
		public string Sku { get; set; }
		[ JsonProperty( "available" ) ]
		public int Available { get; set; }
	}
}
