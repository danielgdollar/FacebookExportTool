using System;
using System.Collections.Generic;
using System.Text;

namespace FacebookExportEntities.Entities
{
    public partial class Friend
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime DateAdded { get; set; }
    }
}
