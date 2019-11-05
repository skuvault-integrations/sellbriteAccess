using System;
using System.Linq;
using Newtonsoft.Json;
using SellbriteAccess.Shared;

namespace SellbriteAccess.Models
{
	public class Product
	{
		public int Id { get; set; }
		public string Sku { get; set; }
		public string Brand { get; set; }
		public string Manufacturer { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public decimal Cost { get; set; }
		[ JsonProperty( "category_name" ) ]
		public string CategoryName { get; set; }
		[ JsonProperty( "upc" ) ]
		public string UPC { get; set; }
		public int? Quantity { get; set; }
		public string Provider { get; set; }
		[ JsonProperty( "image_list") ]
		public string ImageList { get; set; }
		[ JsonProperty( "package_weight") ]
		public decimal Weight { get; set; }
		[ JsonProperty( "package_unit_of_weight") ]
		public string WeightUnits { get; set; }
		[ JsonProperty( "modified_at") ]
		public string ModifiedAtRFC3339 { get; set; }
	}

	public class SellbriteProduct
	{
		public int Id { get; set; }
		public string Sku { get; set; }
		public string Brand { get; set; }
		public string Manufacturer { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public decimal Price { get; set; }
		public decimal Cost { get; set; }
		public string CategoryName { get; set; }
		public string UPC { get; set; }
		public int? Quantity { get; set; }
		public string Provider { get; set; }
		public string Image { get; set; }
		public decimal Weight { get; set; }
		public string WeightUnits { get; set; }
		public DateTime ModifiedAtUtc { get; set; }
	}

	public class SellbriteProductInventory
	{
		public string Sku { get; set; }
		[ JsonProperty( "warehouse_uuid" ) ]
		public string WarehouseUuid { get; set; }
		public int OnHand { get; set; }
		public int Available { get; set; }
		public int Reserved { get; set; }
	}

	public static class ProductExtensions
	{
		public static SellbriteProduct ToSvProduct( this Product product ) 
		{
			return new SellbriteProduct
			{
				Brand = product.Brand,
				CategoryName = product.CategoryName,
				Cost = product.Cost,
				Description = product.Description,
				Id = product.Id,
				Image = product.ImageList.Split( '|' ).FirstOrDefault(),
				Manufacturer = product.Manufacturer,
				Name = product.Name,
				Price = product.Price,
				Provider = product.Provider,
				Quantity = product.Quantity,
				Sku = product.Sku,
				UPC = product.UPC,
				Weight = product.Weight,
				WeightUnits = product.WeightUnits,
				ModifiedAtUtc = string.IsNullOrWhiteSpace( product.ModifiedAtRFC3339 ) ? default( DateTime ) : product.ModifiedAtRFC3339.FromRFC3339ToUtc(),
			};
		}		
	}
}
