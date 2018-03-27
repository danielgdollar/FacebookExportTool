using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FacebookExportEntities.Entities
{
    public partial class TimelinePost
    {
        public int Id { get; set; }

        public DateTime PostDate { get; set; }

        public int? PostFriendId { get; set; }

        [Required]
        public string PostText { get; set; }

        public string PostComment { get; set; }

        public virtual Friend PostFriend { get; set; }
    }
}
