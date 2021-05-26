using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProCode.FileHosterRepo.Api
{
    public static class Mapper
    {
        public static Model.Response.User MapReponseUser(this Dal.Model.User user)
        {
            return new Model.Response.User
            {
                UserId = user.UserId,
                Nickname = user.Nickname,
                Role = user.Role,
                Created = user.Created
            };
        }

        public static void MapResponseMedia(this Dal.Model.MediaHeader media, ref Model.Response.MediaHeader responseHeader)
        {
            responseHeader.MediaHeaderId = media.MediaHeaderId;
            responseHeader.Name = media.Name;
            responseHeader.Description = media.Description;
            responseHeader.ReferenceLink = media.ReferenceLink != null ? media.ReferenceLink.AbsoluteUri : string.Empty;
            responseHeader.User = media.User.MapReponseUser();
        }

        public static void MapResponseMediaPart(this Dal.Model.MediaPart mediaPart, ref Model.Response.MediaPart responsePart)
        {
            responsePart.MediaPartId = mediaPart.MediaPartId;
            responsePart.Season = mediaPart.Season;
            responsePart.Episode = mediaPart.Episode;
            responsePart.Name = mediaPart.Name;
            responsePart.Description = mediaPart.Description;
            responsePart.ReferenceLink = mediaPart.ReferenceLink != null ? mediaPart.ReferenceLink.AbsoluteUri : string.Empty;
            responsePart.User = mediaPart.User.MapReponseUser();
            responsePart.Created = mediaPart.Created;
        }

        public static void MapResponseMediaVersion(this Dal.Model.MediaVersion mediaVersion, ref Model.Response.MediaVersion responseVersion)
        {
            responseVersion.MediaVersionId = mediaVersion.MediaVersionId;
            responseVersion.VersionComment = mediaVersion.VersionComment;
            responseVersion.Created = mediaVersion.Created;
        }

        public static void MapResponseMediaLink(this Dal.Model.MediaLink link, ref Model.Response.MediaLink responseLink)
        {
            responseLink.MediaLinkId = link.MediaLinkId;
            responseLink.LinkOrderId = link.LinkOrderId;
            responseLink.Link = link.Link.AbsoluteUri;
        }
    }
}
