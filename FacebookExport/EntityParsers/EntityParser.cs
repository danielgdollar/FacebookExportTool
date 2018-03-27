using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FacebookExport.Settings;
using FacebookExportEntities;
using HtmlAgilityPack;

namespace FacebookExport.Parsers
{
    public abstract class EntityParser<T> where T : class, new()
    {
        protected FacebookExportDbContext DbContext { get; }

        protected string FilePath { get; }

        protected CultureInfo CultureInfoProvider { get; }

        protected EntityParserSettings Settings { get; }

        public EntityParser(FacebookExportDbContext dbContext, string filePath, EntityParserSettings settings)
        {
            DbContext = dbContext;
            FilePath = filePath;
            CultureInfoProvider = CultureInfo.InvariantCulture;
            Settings = settings;
        }

        /// <summary>
        /// Using the ParseEntityFromHtml method, retrieve an individual entity per node returned by the provided XPath. Add each of
        /// these entities to the context and commit the changes.
        /// </summary>
        /// <returns></returns>
        public async Task ProcessImport()
        {
            var nodes = GetHtmlNodesFromFile();
            foreach (var node in nodes)
            {
                var entity = ParseEntityFromHtml(node);
                if (entity != null)
                {
                    DbContext.Add(entity);
                }
            }
            await DbContext.SaveChangesAsync();
        }

        public abstract T ParseEntityFromHtml(HtmlNode node);

        /// <summary>
        /// Given a f
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="exportPath"></param>
        /// <param name="nodesXpath"></param>
        /// <returns></returns>
        protected HtmlNodeCollection GetHtmlNodesFromFile()
        {
            var doc = new HtmlDocument();
            doc.Load(FilePath);

            var nodes = doc.DocumentNode.SelectNodes(Settings.NodesXPath);

            return nodes;
        }
    }
}
