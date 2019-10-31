using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;
using SellbriteAccess.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SellbriteAccessTests
{
	public class BaseTest
	{
		protected SellbriteConfig Config { get; private set; }
		protected SellbriteMerchantCredentials Credentials { get; private set; }

		public BaseTest()
		{
			var config = this.LoadTestSettings< SellbriteTestConfig >( @"\..\..\test-config.csv" );

			this.Config = new SellbriteConfig();
			this.Credentials = new SellbriteMerchantCredentials( config.AccountToken, config.SecretKey );
		}

		protected T LoadTestSettings< T >( string filePath )
		{
			string basePath = new Uri( Path.GetDirectoryName( Assembly.GetExecutingAssembly().CodeBase ) ).LocalPath;

			using( var streamReader = new StreamReader( basePath + filePath ) )
			{
				var csvConfig = new Configuration()
				{
					Delimiter = ","
				};

				using( var csvReader = new CsvReader( streamReader, csvConfig ) )
				{
					var credentials = csvReader.GetRecords< T >();

					return credentials.FirstOrDefault();
				}
			}
		}
	}
}
