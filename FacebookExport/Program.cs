using System;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.Extensions.Configuration;
using FacebookExportEntities;
using FacebookExportEntities.Entities;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Net;
using FacebookExport.Parsers;
using FacebookExport.Settings;

namespace FacebookExport
{
    class Program
    {
        static void Main(string[] args)
        {
            var task = MainAsync(args);
            task.Wait();
        }

        static async Task MainAsync(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.local.json", true);
            var configuration = builder.Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if(string.IsNullOrEmpty(connectionString))
            {
                //Indicates that a connection string was not provided in the appsettings file
                throw new InvalidOperationException("A connection string must be configured");
            }

            //Configure database
            var optionsBuilder = new DbContextOptionsBuilder();
            var dbOptions = optionsBuilder
                .UseSqlServer(connectionString, providerOptions => providerOptions.CommandTimeout(60))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .Options;

            //Bind to typed config objects
            var appsettings = new Appsettings();
            var friendsParserSettings = new EntityParserSettings();
            var timelinePostParserSettings = new EntityParserSettings();      
            configuration.GetSection("Appsettings").Bind(appsettings);
            configuration.GetSection("FriendsParserSettings").Bind(friendsParserSettings);
            configuration.GetSection("TimelinePostParserSettings").Bind(timelinePostParserSettings);

            //Process parsing of export
            using (var dbContext = new FacebookExportEntities.FacebookExportDbContext(dbOptions))
            {
                //Parse friends
                var friendsFilePath = Path.Combine(appsettings.FacebookExportPath, friendsParserSettings.FilePath);
                var friendsParser = new FriendsParser(dbContext, friendsFilePath, friendsParserSettings);
                await friendsParser.ProcessImport();

                //Parse timeline posts
                var timelinePostsPath = Path.Combine(appsettings.FacebookExportPath, timelinePostParserSettings.FilePath);
                var timelinePostParser = new TimelinePostParser(dbContext, timelinePostsPath, timelinePostParserSettings);
                await timelinePostParser.ProcessImport();
            }
        }
    }
}
