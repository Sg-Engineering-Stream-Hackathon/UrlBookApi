using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModelService
{
    public class BookmarkCard
    {
        public string UserName { get; set; }
        public string LongUrl { get; set; }
        public string shortUrl { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Tribe { get; set; }
        public string FeatureTeam { get; set; }
        public string Application { get; set; }
        public string IconName { get; set; }
        public bool IsCardValidationRequired { get; set; }
        public bool IsCardExpired { get; set; }
        [Key]
        public int BookmarkId { get; set; }
    }
}
