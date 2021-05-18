using System.Collections.Generic;

namespace ProCode.FileHosterRepo.Api.Model.Request
{
    public class Media
    {
        public string Name { get; set; }
        public MediaPart MediaPart { get; set; }
    }

    public class MediaPart
    {
        public Media Media { get; set; }
        public string Name { get; set; }
        public IList<MediaPartLink> MediaPartLinkList { get; set; }
    }

    public class MediaPartLink
    {
        public MediaPart MediaVersion { get; set; }
        public int LinkOrder { get; set; }
        public string Link { get; set; }
    }
}
