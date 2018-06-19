using System;
using System.Collections.Generic;

namespace RaceDB.Models
{
    public partial class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryKey { get; set; }
        public int Status { get; set; }
    }
}
