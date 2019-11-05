using FluentAssertions;
using NUnit.Framework;
using SellbriteAccess.Services.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SellbriteAccessTests
{
	[ TestFixture ]
	public class ProductsServiceTests : BaseTest
	{
		private ISellbriteProductsService _productsService;
		private string _sku = "testsku1";
		private string _sku2 = "testsku2";
		private string _sku3 = "testsku4";
		private string _activeWarehouseId;

		[ SetUp ]
		public void Init()
		{
			_productsService = new SellbriteProductsService( base.Config, base.Credentials );
			_activeWarehouseId = _productsService.GetWarehousesAsync( CancellationToken.None ).Result.FirstOrDefault( w => !w.Archived ).Uuid;
		}

		[ Test ]
		public async Task GetProductBySku()
		{
			var product = await _productsService.GetProductBySkuAsync( _sku, CancellationToken.None );

			product.Should().NotBeNull();
			product.Sku.Should().Be( _sku );
		}

		[ Test ]
		public async Task GetProductBySkuSearchShouldBeCaseSensitive()
		{
			var product = await _productsService.GetProductBySkuAsync( _sku3, CancellationToken.None );

			product.Should().BeNull();
		}

		[ Test ]
		public async Task GetWarehouses()
		{
			var warehouses = await _productsService.GetWarehousesAsync( CancellationToken.None );

			warehouses.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task GetAllSkusInventories()
		{
			base.Config.ProductsInventoriesPageLimit = 5;
			var inventories = await _productsService.GetAllSkusInventory( _activeWarehouseId, CancellationToken.None );

			inventories.Count().Should().BeGreaterThan( 0 );
		}

		[ Test ]
		public async Task UpdateSkuQuantity()
		{
			int quantity = new Random().Next(1, 100 );
			await _productsService.UpdateSkuQuantityAsync( _sku, quantity, _activeWarehouseId, CancellationToken.None );
	
			var skuInventory = await _productsService.GetSkuInventory( _sku, _activeWarehouseId, CancellationToken.None );

			skuInventory.Should().NotBeNull();
			skuInventory.Sku.Should().Be( _sku );
			skuInventory.Available.Should().Be( quantity );
		}

		[ Test ]
		public async Task UpdateSkuQuantityToZero()
		{
			int quantity = 0;
			await _productsService.UpdateSkuQuantityAsync( _sku, quantity, _activeWarehouseId, CancellationToken.None );
			var skuInventory = await _productsService.GetSkuInventory( _sku, _activeWarehouseId, CancellationToken.None );

			skuInventory.Should().NotBeNull();
			skuInventory.Sku.Should().Be( _sku );
			skuInventory.Available.Should().Be( quantity );
		}

		[ Test ]
		public async Task UpdateSkusQuantities()
		{
			var rand = new Random();
			int skuQuantity = rand.Next(1, 100 );
			int sku2Quantity = rand.Next(1, 100 );

			var skusQuantities = new Dictionary< string, int >
			{
				{ _sku, skuQuantity },
				{ _sku2, sku2Quantity }
			};

			await _productsService.UpdateSkusQuantitiesAsync( skusQuantities, _activeWarehouseId, CancellationToken.None );

			var skuInventory = await _productsService.GetSkuInventory( _sku, _activeWarehouseId, CancellationToken.None );
			skuInventory.Should().NotBeNull();
			skuInventory.Sku.Should().Be( _sku );
			skuInventory.Available.Should().Be( skuQuantity );

			var sku2Inventory = await _productsService.GetSkuInventory( _sku2, _activeWarehouseId, CancellationToken.None );
			sku2Inventory.Should().NotBeNull();
			sku2Inventory.Sku.Should().Be( _sku2 );
			sku2Inventory.Available.Should().Be( sku2Quantity );
		}

		[ Test ]
		public async Task BatchUpdateSkusQuantities()
		{
			base.Config.ProductsInventoryUpdateMaxBatchSize = 2;
			var rand = new Random();
			var skusQuantities = new Dictionary< string, int >();
			var skusNumber = 20;
			
			for( int i = 0; i < skusNumber; i++ )
			{
				var sku = "testsku" + i.ToString();
				skusQuantities.Add( sku, rand.Next( 1, 100 ) );
			}

			await _productsService.UpdateSkusQuantitiesAsync( skusQuantities, _activeWarehouseId, CancellationToken.None );

			foreach( var skuQuantity in skusQuantities )
			{
				var skuInventory = await _productsService.GetSkuInventory( skuQuantity.Key, _activeWarehouseId, CancellationToken.None );
				
				if ( skuInventory == null )
					continue;

				skuInventory.Sku.Should().Be( skuQuantity.Key );
				skuInventory.Available.Should().Be( skuQuantity.Value );
			}
		}

		[ Test ]
		public async Task GetProductsByDateModifiedAsync()
		{
			var startDateUtc = new DateTime( 2019, 1, 1 );

			var products = await _productsService.GetProductsByDateModifiedAsync( startDateUtc, DateTime.MaxValue, CancellationToken.None );

			products.Should().NotBeEmpty();
		}

		[ Test ]
		public async Task GetProductsPageAsync()
		{
			var startDateUtc = new DateTime( 2019, 1, 1 );
			base.Config.ProductsPageLimit = 2;

			var products = await _productsService.GetProductsByDateModifiedAsync( startDateUtc, DateTime.MaxValue, CancellationToken.None );

			products.Count().Should().BeGreaterThan( base.Config.ProductsPageLimit );
		}
	}
}
