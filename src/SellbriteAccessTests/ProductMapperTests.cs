using FluentAssertions;
using NUnit.Framework;
using SellbriteAccess.Models;
using SellbriteAccess.Shared;

namespace SellbriteAccessTests
{
	[ TestFixture ]
	public class ProductMapperTests
	{
		[ Test ]
		public void ToSvProduct()
		{
			var product = new Product
			{
				Brand = "some brand",
				CategoryName = "cats",
				Cost = 12,
				Description = "asdf",
				Id = 1,
				ImageList = "asdfsdf.com",
				Manufacturer = "manny",
				Name = "some name",
				Price = 10000000.00m,
				Provider = "asdflkj",
				Quantity = 12,
				Sku = "testsku1",
				UPC = "upc1234556",
				Weight = 12.3m,
				WeightUnits = "lbs",
				ModifiedAtRFC3339 = "2019-10-22T16:25:25Z",
			};

			var svProduct = product.ToSvProduct();

			svProduct.ModifiedAtUtc.Should().Be( product.ModifiedAtRFC3339.FromRFC3339ToUtc() );
			svProduct.Brand.Should().Be( product.Brand );
			svProduct.CategoryName.Should().Be( product.CategoryName );
			svProduct.Cost.Should().Be( product.Cost );
			svProduct.Description.Should().Be( product.Description );
			svProduct.Id.Should().Be( product.Id );
			svProduct.ImageList.Should().Be( product.ImageList );
			svProduct.Manufacturer.Should().Be( product.Manufacturer );
			svProduct.Name.Should().Be( product.Name );
			svProduct.Price.Should().Be( product.Price );
			svProduct.Provider.Should().Be( product.Provider );
			svProduct.Quantity.Should().Be( product.Quantity );
			svProduct.Sku.Should().Be( product.Sku );
			svProduct.UPC.Should().Be( product.UPC );
			svProduct.Weight.Should().Be( product.Weight );
			svProduct.WeightUnits.Should().Be( product.WeightUnits );
		}
	}
}
