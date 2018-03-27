using FacebookExport.Settings;
using FacebookExportEntities;
using FacebookExportEntities.Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FacebookExport.Parsers
{
    public class TimelinePostParser : EntityParser<TimelinePost>
    {
        protected string DateFormat { get; }

        protected IEnumerable<Friend> Friends { get; }

        protected Regex FriendNameRegex { get; }

        public TimelinePostParser(FacebookExportDbContext dbContext, string filePath, EntityParserSettings settings) : base(dbContext, filePath, settings)
        {
            DateFormat = "dddd, MMMM d, yyyy a\\t h:mmtt";
            Friends = dbContext.Friend.ToList();
            FriendNameRegex = new Regex("(.*)<br>");
        }

        /// <summary>
        /// Given the html node of a timeline post, create a TimelinePost entity from the available data.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override TimelinePost ParseEntityFromHtml(HtmlNode node)
        {
            DateTime postDate;
            string postText = "";
            int? postFriendId = null;
            string postComment = null;

            //The first node is always a date/time. Get the date string with timezone portion trimmed
            var nextNode = node.FirstChild;
            var dateString = nextNode.InnerHtml.Substring(0, nextNode.InnerHtml.Length - 4);
            postDate = DateTime.ParseExact(dateString, DateFormat, CultureInfoProvider);

            //Check if the post is empty
            if (nextNode.NextSibling != null)
            {
                nextNode = nextNode.NextSibling;
                //Check if the post's text is one html block
                if (nextNode.NextSibling == null)
                {
                    postText = nextNode.InnerHtml;
                }
                else
                {
                    //Check if the post was made by a friend
                    if (nextNode.Name == "div" && !nextNode.Attributes.Any(attr => attr.Name == "class" && attr.Value == "comment"))
                    {
                        //Retrieve the friends name by getting all characters up to the beginning of a <br /> tag, which always follows the name
                        var friendName = WebUtility.HtmlDecode(nextNode.InnerHtml.Substring(0, nextNode.InnerHtml.IndexOf('<')));

                        var friend = Friends.FirstOrDefault(x => x.Name == friendName);
                        if (friend != null)
                        {
                            postFriendId = friend.Id;
                        }
                        nextNode = nextNode.NextSibling;
                    }

                    //Check if the post has text
                    if (nextNode.Name == "#text")
                    {
                        postText = nextNode.InnerHtml;
                        nextNode = nextNode.NextSibling;
                    }

                    //Check if the post has a comment
                    if (nextNode != null)
                    {
                        postComment = nextNode.InnerHtml;
                    }
                }
            }
            var timelinePost = new TimelinePost { PostComment = WebUtility.HtmlDecode(postComment), PostDate = postDate, PostFriendId = postFriendId, PostText = WebUtility.HtmlDecode(postText) };
            return timelinePost;
        }
    }
}
