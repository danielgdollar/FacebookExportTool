using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FacebookExport.Settings;
using FacebookExportEntities;
using FacebookExportEntities.Entities;
using HtmlAgilityPack;

namespace FacebookExport.Parsers
{
    public class FriendsParser : EntityParser<Friend>
    {
        protected Regex FriendRegex { get; }

        protected string DateFormat { get; set; }

        public FriendsParser(FacebookExportDbContext dbContext, string filePath, EntityParserSettings settings) : base(dbContext, filePath, settings)
        {
           FriendRegex = new Regex(".* ((.*))");
           DateFormat = "MMM d, yyyy";
        }

        /// <summary>
        /// Given the html for a friend, retrieve a Friend entity.
        /// Not all nodes will point to a valid friend.
        /// </summary>
        /// <param name="html"></param>
        /// <returns>A Friend entity. If the node does not point to a valid friend then null will be returned.</returns>
        public override Friend ParseEntityFromHtml(HtmlNode node)
        {
            Friend friend = null;
            var html = node.InnerHtml;
            var match = FriendRegex.Match(html);
            if (match != null)
            {
                var firstParenIndex = html.IndexOf('(');
                var friendName = WebUtility.HtmlDecode(html.Substring(0, firstParenIndex - 1));
                var dateString = html.Substring(firstParenIndex + 1, html.Length - firstParenIndex - 2);

                DateTime friendAddedDate;
                bool convertSucceeded = DateTime.TryParseExact(dateString, DateFormat, CultureInfoProvider, DateTimeStyles.None, out friendAddedDate);
                if (!convertSucceeded)
                {
                    //Dates do not include a year if from the current year. Append current year to date and then parse.
                    dateString += ", " + DateTime.UtcNow.Year;
                    friendAddedDate = DateTime.ParseExact(dateString, DateFormat, CultureInfoProvider);
                }
                friend = new Friend { DateAdded = friendAddedDate, Name = friendName };
            }
            return friend;
        }
    }
}
