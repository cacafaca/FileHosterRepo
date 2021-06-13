using System;
using System.Collections.Generic;

namespace ProCode.FileHosterRepo.Common.Api.Response
{
    public class MediaHeader
    {
        public int MediaHeaderId { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public string Description { get; set; }
        public string ReferenceLink { get; set; }
        public User User { get; set; }
        public IList<MediaPart> Parts { get; set; }
        public IList<MediaTag> Tags { get; set; }

        public override string ToString()
        {
            int partCount = 0;
            foreach (var p in Parts)
                partCount++;
            return $"{Name} (Parts:{partCount})";
        }
    }

    public class MediaPart
    {
        public int MediaPartId { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReferenceLink { get; set; }
        public User User { get; set; }
        public IList<MediaVersion> Versions { get; set; }
        public IList<MediaTag> Tags { get; set; }
        public DateTime Created { get; set; }

        public override string ToString()
        {
            int versionCount = 0;
            foreach (var p in Versions)
                versionCount++;
            return $"{Name} (Versions:{versionCount})";
        }
    }

    public class MediaVersion
    {
        public int MediaVersionId { get; set; }
        public string VersionComment { get; set; }
        public IList<MediaLink> Links { get; set; }
        public IList<MediaTag> Tags { get; set; }
        public User User { get; set; }
        public DateTime Created { get; set; }

        public override string ToString()
        {
            int linkCount = 0;
            foreach (var p in Links)
                linkCount++;
            return $"{VersionComment} (Links:{linkCount})";
        }
    }

    public class MediaLink
    {
        public int MediaLinkId { get; set; }
        public int LinkOrderId { get; set; }
        public string Link { get; set; }
        public IList<MediaTag> Tags { get; set; }
    }

    public class MediaTag
    {
        public string Name { get; set; }
        
        public override string ToString()
        {
            return Name;
        }
    }
}
