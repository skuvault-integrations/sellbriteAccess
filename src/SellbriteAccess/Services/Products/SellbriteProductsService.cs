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

		public async Task< SellbriteProduct > GetProductBySkuAsync( string sku, CancellationToken token )
		{
			var url = string.Format( "{0}/{1}", SellbriteEndPoint.ProductsUrl, Uri.EscapeDataString( sku ) );
			var response = await base.GetAsync( url, token ).ConfigureAwait( false );

			if ( response.Contains( _productNotFoundErrorMessage ) )
			{
				return null;
			}

			return JsonConvert.DeserializeObject< SellbriteProduct >( response );
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
				Inventory = new UpdateSkuInventoryRequest[] { 
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
			var url = SellbriteEndPoint.ProductsInventoryUrl;
			var skuInventoryUpdateRequests = skusQuantities.Select( sq => new UpdateSkuInventoryRequest() { Sku = sq.Key, Available = sq.Value, WarehouseUuid = warehouseId } );
			var chunks = skuInventoryUpdateRequests.SplitToChunks( base.Config.ProductsInventoryUpdateMaxBatchSize );

			foreach( var chunk in chunks )
			{
				await base.PutAsync< UpdateSkusInventoryRequest >( url, new UpdateSkusInventoryRequest() { Inventory = chunk.ToArray() }, token, Mark.CreateNew() );
			}
		}
	}
}
