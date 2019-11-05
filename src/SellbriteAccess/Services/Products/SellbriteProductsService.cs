using Newtonsoft.Json;
using SellbriteAccess.Configuration;
using SellbriteAccess.Exceptions;
using SellbriteAccess.Models;
using SellbriteAccess.Models.Requests;
using SellbriteAccess.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SellbriteAccess.Services.Products
{
	public class SellbriteProductsService : BaseService, ISellbriteProductsService
	{
		private const string _productNotFoundErrorMessage = "not found";

		public SellbriteProductsService( SellbriteConfig config, SellbriteMerchantCredentials credentials ) : base( config, credentials )
		{ }

		public async Task< SellbriteProductInventory[] > GetAllSkusInventory( string warehouseId, CancellationToken token )
		{
			var productsInventories = new List< SellbriteProductInventory >();
			int page = 1;

			while( true )
			{
				var url = string.Format("{0}?page={1}&limit={2}", SellbriteEndPoint.ProductsInventoryUrl, page, base.Config.ProductsInventoriesPageLimit );
				var response = await base.GetAsync< SellbriteProductInventory[] >( url, token, Mark.CreateNew() ).ConfigureAwait( false );

				if ( response.Count() == 0 )
					break;

				productsInventories.AddRange( response );
				page++;
			}

			return productsInventories.ToArray();
		}

		public async Task< SellbriteProduct > GetProductBySkuAsync( string sku, CancellationToken token )
		{
			var url = string.Format( "{0}?skus={1}", SellbriteEndPoint.ProductsUrl, Uri.EscapeDataString( sku ) );
			var response = await base.GetAsync( url, token ).ConfigureAwait( false );

			if ( response.Contains( _productNotFoundErrorMessage ) )
			{
				return null;
			}

			return JsonConvert.DeserializeObject< SellbriteProduct[] >( response ).FirstOrDefault();
		}

		public async Task< SellbriteProductInventory > GetSkuInventory( string sku, string warehouseId, CancellationToken token )
		{
			var url = string.Format( "{0}?sku={1}", SellbriteEndPoint.ProductsInventoryUrl, Uri.EscapeDataString( sku ) );
			var response = await base.GetAsync( url, token ).ConfigureAwait( false );

			if ( response.Contains( _productNotFoundErrorMessage ) )
			{
				return null;
			}

			var skusInventories = JsonConvert.DeserializeObject< SellbriteProductInventory[] >( response );
			return skusInventories.FirstOrDefault( i => i.WarehouseUuid.Equals( warehouseId ) );
		}

		public Task< Warehouse[] > GetWarehousesAsync( CancellationToken token )
		{
			var mark = Mark.CreateNew();

			return base.GetAsync< Warehouse[] >( SellbriteEndPoint.WarehousesUrl, token, mark );
		}

		public Task UpdateSkuQuantityAsync( string sku, int quantity, string warehouseId, CancellationToken token )
		{
			var url = SellbriteEndPoint.ProductsInventoryUrl;
			var mark = Mark.CreateNew();

			var request = new UpdateSkusInventoryRequest()
			{
				Requests = new UpdateSkuInventoryRequest[] { 
					new UpdateSkuInventoryRequest()
					{
						WarehouseUuid = warehouseId,
						Sku = sku,
						Available = quantity
					}
				}
			};

			return base.PutAsync< UpdateSkusInventoryRequest >( url, request, token, mark );
		}

		public async Task UpdateSkusQuantitiesAsync( Dictionary< string, int > skusQuantities, string warehouseId, CancellationToken token )
		{
			var skuInventoryUpdateRequests = new List< UpdateSkuInventoryRequest >();
			var skusInventories = await this.GetAllSkusInventory( warehouseId, token ).ConfigureAwait( false );

			foreach( var skuInventory in skusInventories )
			{
				var skuQuantity = skusQuantities.FirstOrDefault( sq => sq.Key.ToLower().Equals( skuInventory.Sku.ToLower() ) );

				if ( skuQuantity.Key != null )
				{
					skuInventoryUpdateRequests.Add( new UpdateSkuInventoryRequest()
					{
						Sku = skuInventory.Sku,
						WarehouseUuid = warehouseId,
						Available = skuQuantity.Value
					} );
				}
			}

			var url = SellbriteEndPoint.ProductsInventoryUrl;
			var chunks = skuInventoryUpdateRequests.SplitToChunks( base.Config.ProductsInventoryUpdateMaxBatchSize );

			foreach( var chunk in chunks )
			{
				await base.PutAsync< UpdateSkusInventoryRequest >( url, new UpdateSkusInventoryRequest() { Requests = chunk.ToArray() }, token, Mark.CreateNew() );
			}
		}
	}
}
