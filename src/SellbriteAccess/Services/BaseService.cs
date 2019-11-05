using CuttingEdge.Conditions;
using Newtonsoft.Json;
using SellbriteAccess.Configuration;
using SellbriteAccess.Exceptions;
using SellbriteAccess.Shared;
using SellbriteAccess.Throttling;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SellbriteAccess.Services
{
	public class BaseService : IDisposable
	{
		protected SellbriteConfig Config { get; private set; }
		protected SellbriteMerchantCredentials Credentials { get; private set; }
		protected readonly Throttler Throttler;
		protected readonly HttpClient HttpClient;

		private Func< string > _additionalLogInfo;
		private const int _tooManyRequestsHttpCode = 429;

		/// <summary>
		///	Extra logging information
		/// </summary>
		public Func< string > AdditionalLogInfo
		{
			get { return this._additionalLogInfo ?? ( () => string.Empty ); }
			set => _additionalLogInfo = value;
		}

		public BaseService( SellbriteConfig config, SellbriteMerchantCredentials credentials )
		{
			Condition.Requires( config, "config" ).IsNotNull();
			Condition.Requires( credentials, "credentials" ).IsNotNull();

			this.Config = config;
			this.Credentials = credentials;
			this.Throttler = new Throttler( config.ThrottlingOptions.MaxRequestsPerTimeInterval, config.ThrottlingOptions.TimeIntervalInSec, config.ThrottlingOptions.MaxRetryAttempts );

			HttpClient = new HttpClient()
			{
				BaseAddress = new Uri( Config.ApiBaseUrl ) 
			};

			SetBasicAuthorizationHeader();
		}

		private void SetBasicAuthorizationHeader()
		{
			var headerValue = String.Format( "Basic {0}",  Convert.ToBase64String( Encoding.UTF8.GetBytes( this.Credentials.AccountToken + ":" + this.Credentials.SecretKey ) ) );
			this.HttpClient.DefaultRequestHeaders.Add( "Authorization", headerValue );
		}

		protected Task< string > PutAsync< T >( string url, T data, CancellationToken cancellationToken, Mark mark = null )
		{
			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
				throw new SellbriteException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
			}

			return this.ThrottleRequest( url, data.ToJson(), mark, async ( token ) =>
			{
				var payload = new StringContent( JsonConvert.SerializeObject( data ), Encoding.UTF8, "application/json" );
				var httpResponse = await HttpClient.PutAsync( url, payload, token ).ConfigureAwait( false );
				var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

				ThrowIfError( httpResponse, content );

				return content;
			}, cancellationToken );
		}

		protected Task< string > GetAsync( string url, CancellationToken cancellationToken, Mark mark = null )
		{
			if ( cancellationToken.IsCancellationRequested )
			{
				var exceptionDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
				throw new SellbriteException( string.Format( "{0}. Task was cancelled", exceptionDetails ) );
			}

			return this.ThrottleRequest( url, string.Empty, mark, async ( token ) =>
			{
				var httpResponse = await HttpClient.GetAsync( url ).ConfigureAwait( false );
				var content = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait( false );

				ThrowIfError( httpResponse, content );

				return content;
			}, cancellationToken );
		}

		protected async Task< T > GetAsync< T >( string url, CancellationToken cancellationToken, Mark mark = null )
		{
			var response = await this.GetAsync( url, cancellationToken, mark ).ConfigureAwait( false );

			return JsonConvert.DeserializeObject< T >( response );
		}

		protected void ThrowIfError( HttpResponseMessage response, string message )
		{
			HttpStatusCode responseStatusCode = response.StatusCode;

			if ( response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound )
				return;

			if ( responseStatusCode == HttpStatusCode.Unauthorized )
			{
				throw new SellbriteUnauthorizedException( message );
			}
			else if ( (int)responseStatusCode == _tooManyRequestsHttpCode )
			{
				throw new SellbriteRateLimitsExceeded( message );
			}

			throw new SellbriteNetworkException( message );
		}

		protected Task< T > ThrottleRequest< T >( string url, string payload, Mark mark, Func< CancellationToken, Task< T > > processor, CancellationToken token )
		{
			return Throttler.ExecuteAsync( () =>
			{
				return new ActionPolicy( Config.NetworkOptions.RetryAttempts, Config.NetworkOptions.DelayBetweenFailedRequestsInSec, Config.NetworkOptions.DelayFailRequestRate )
					.ExecuteAsync( async () =>
					{
						using( var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource( token ) )
						{
							SellbriteLogger.LogStarted( this.CreateMethodCallInfo( url, mark, payload: payload, additionalInfo: this.AdditionalLogInfo() ) );
							linkedTokenSource.CancelAfter( Config.NetworkOptions.RequestTimeoutMs );

							var result = await processor( linkedTokenSource.Token ).ConfigureAwait( false );

							SellbriteLogger.LogEnd( this.CreateMethodCallInfo( url, mark, methodResult: result.ToJson(), additionalInfo: this.AdditionalLogInfo() ) );

							return result;
						}
					}, 
					( exception, timeSpan, retryCount ) =>
					{
						string retryDetails = CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() );
						SellbriteLogger.LogTraceRetryStarted( timeSpan.Seconds, retryCount, retryDetails );
					},
					() => CreateMethodCallInfo( url, mark, additionalInfo: this.AdditionalLogInfo() ),
					SellbriteLogger.LogTraceException );
			} );
		}

		/// <summary>
		///	Creates method calling detailed information
		/// </summary>
		/// <param name="url">Absolute path to service endpoint</param>
		/// <param name="mark">Unique stamp to track concrete method</param>
		/// <param name="errors">Errors</param>
		/// <param name="methodResult">Service endpoint raw result</param>
		/// <param name="payload">Method payload (POST)</param>
		/// <param name="additionalInfo">Extra logging information</param>
		/// <param name="memberName">Method name</param>
		/// <returns></returns>
		protected string CreateMethodCallInfo( string url = "", Mark mark = null, string errors = "", string methodResult = "", string additionalInfo = "", string payload = "", [ CallerMemberName ] string memberName = "" )
		{
			string serviceEndPoint = null;
			string requestParameters = null;

			if ( !string.IsNullOrEmpty( url ) )
			{
				Uri uri = new Uri( url.Contains( Config.ApiBaseUrl ) ? url : Config.ApiBaseUrl + url );

				serviceEndPoint = uri.LocalPath;
				requestParameters = uri.Query;
			}

			var str = string.Format(
				"{{MethodName: {0}, Mark: '{1}', ServiceEndPoint: '{2}', {3} {4}{5}{6}{7}}}",
				memberName,
				mark ?? Mark.Blank(),
				string.IsNullOrWhiteSpace( serviceEndPoint ) ? string.Empty : serviceEndPoint,
				string.IsNullOrWhiteSpace( requestParameters ) ? string.Empty : ", RequestParameters: " + requestParameters,
				string.IsNullOrWhiteSpace( errors ) ? string.Empty : ", Errors:" + errors,
				string.IsNullOrWhiteSpace( methodResult ) ? string.Empty : ", Result:" + methodResult,
				string.IsNullOrWhiteSpace( payload ) ? string.Empty : ", Payload:" + payload,
				string.IsNullOrWhiteSpace( additionalInfo ) ? string.Empty : ", " + additionalInfo
			);
			return str;
		}

		#region IDisposable Support
		private bool disposedValue = false;

		void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					this.Throttler.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
