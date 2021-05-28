using System.Collections.Generic;

namespace ProCode.FileHosterRepo.Dto.Api.Request
{
    public interface IMediaTags
    {
        public IEnumerable<MediaTag> Tags { get; set; }
    }

    public class MediaHeader : IMediaTags
    {
        public int? MediaHeaderId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReferenceLink { get; set; }
        public IEnumerable<MediaPart> Parts { get; set; }
        public IEnumerable<MediaTag> Tags { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MediaPart : IMediaTags
    {
        public int? MediaPartId { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReferenceLink { get; set; }
        public MediaVersion Version { get; set; }
        public IEnumerable<MediaTag> Tags { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class MediaVersion : IMediaTags
    {
        public int? MediaVersionId { get; set; }
        public string VersionComment { get; set; }
        public IEnumerable<MediaLink> Links { get; set; }
        public IEnumerable<MediaTag> Tags { get; set; }

        public override string ToString()
        {
            return VersionComment;
        }
    }

    public class MediaLink
    {
        public int? MediaLinkId { get; set; }
        public int LinkOrderId { get; set; }
        public string Link { get; set; }

        public override string ToString()
        {
            return $"{LinkOrderId:00}:{Link}";
        }
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
