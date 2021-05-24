using System.Collections.Generic;

namespace ProCode.FileHosterRepo.Api.Model.Response
{
    public class MediaHeader
    {
        public int MediaHeaderId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReferenceLink { get; set; }
        public User User { get; set; }
        public IEnumerable<MediaPart> Parts { get; set; }
        public IEnumerable<MediaTag> Tags { get; set; }
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
        public IEnumerable<MediaVersion> Versions { get; set; }
        public IEnumerable<MediaTag> Tags { get; set; }
    }

    public class MediaVersion
    {
        public int MediaVersionId { get; set; }
        public string VersionComment { get; set; }
        public IEnumerable<MediaLink> Links { get; set; }
        public IEnumerable<MediaTag> Tags { get; set; }
    }

    public class MediaLink
    {
        public int MediaLinkId { get; set; }
        public int LinkOrderId { get; set; }
        public string Link { get; set; }
        public IEnumerable<MediaTag> Tags { get; set; }
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
