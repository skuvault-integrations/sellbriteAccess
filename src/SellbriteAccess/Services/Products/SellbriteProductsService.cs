using Newtonsoft.Json;
using SellbriteAccess.Configuration;
using SellbriteAccess.Exceptions;
using SellbriteAccess.Models;
using SellbriteAccess.Models.Requests;
using SellbriteAccess.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingEdge.Conditions;

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

		public async Task< IEnumerable< SellbriteProduct > > GetProductsByDateModifiedAsync( DateTime startDateUtc, DateTime endDateUtc, CancellationToken token )
		{
			Condition.Requires( startDateUtc ).IsLessThan( endDateUtc );
			
			var products = new List< SellbriteProduct >();
			int page = 1;

			while( true )
			{
				var productsByPage = ( await this.GetProductsByPageAsync( startDateUtc, endDateUtc, page, base.Config.ProductsPageLimit, token ).ConfigureAwait( false ) ).ToList();

				if ( !productsByPage.Any() )
				{
					break;
				}

				products.AddRange( productsByPage );

				++page;
			}

			return products;
		}

		private async Task< IEnumerable< SellbriteProduct > > GetProductsByPageAsync( DateTime startDateUtc, DateTime endDateUtc, int page, int limit, CancellationToken token )
		{
			var mark = Mark.CreateNew();
			var products = new List< SellbriteProduct >();
			var url = string.Format("{0}?min_modified_at={1}&max_modified_at={2}&page={3}&limit={4}", SellbriteEndPoint.ProductsUrl, startDateUtc.FromUtcToRFC3339(), endDateUtc.FromUtcToRFC3339(), page, limit );

			try
			{
				SellbriteLogger.LogStarted( this.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ) );

				var response = await base.GetAsync( url, token ).ConfigureAwait( false );

				products = JsonConvert.DeserializeObject< IEnumerable< SellbriteProduct > >( response ).ToList();

				SellbriteLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: products.ToJson(), additionalInfo: this.AdditionalLogInfo() ) );
			}
			catch( Exception ex )
			{
				var sellbriteException = new SellbriteException( this.CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ), ex );
				SellbriteLogger.LogTraceException( sellbriteException );
				throw sellbriteException;
			}

			return products;
		}
	}
}
