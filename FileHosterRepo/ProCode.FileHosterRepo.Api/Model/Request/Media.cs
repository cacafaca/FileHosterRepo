using System.Collections.Generic;

namespace ProCode.FileHosterRepo.Api.Model.Request
{
    public class Media
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReferenceLink { get; set; }
        public IList<MediaPart> Parts { get; set; }
    }

    public class MediaPart
    {
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ReferenceLink { get; set; }
        public IList<MediaLink> Links { get; set; }
    }

    public class MediaLink
    {
        public int LinkId { get; set; }
        public string Link { get; set; }
    }
}
