﻿namespace ProCode.FileHosterRepo.Api
{
    public static class Mapper
    {
        public static Common.Api.Response.User MapReponseUser(this Dal.Model.User user)
        {
            return new Common.Api.Response.User
            {
                UserId = user.UserId,
                Nickname = user?.Nickname,
                Role = user.Role,
                Created = user.Created
            };
        }

        public static void MapResponseMedia(this Dal.Model.MediaHeader media, ref Common.Api.Response.MediaHeader responseHeader)
        {
            responseHeader.MediaHeaderId = media.MediaHeaderId;
            responseHeader.Name = media.Name;
            responseHeader.Year = media.Year;
            responseHeader.Description = media.Description;
            responseHeader.ReferenceLink = media.ReferenceLink != null ? media.ReferenceLink.AbsoluteUri : string.Empty;
            responseHeader.User = media.User.MapReponseUser();
        }

        public static void MapResponseMediaPart(this Dal.Model.MediaPart mediaPart, ref Common.Api.Response.MediaPart responsePart)
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

        public static void MapResponseMediaVersion(this Dal.Model.MediaVersion mediaVersion, ref Common.Api.Response.MediaVersion responseVersion)
        {
            responseVersion.MediaVersionId = mediaVersion.MediaVersionId;
            responseVersion.VersionComment = mediaVersion.VersionComment;
            responseVersion.Created = mediaVersion.Created;
        }

        public static void MapResponseMediaLink(this Dal.Model.MediaLink link, ref Common.Api.Response.MediaLink responseLink)
        {
            responseLink.MediaLinkId = link.MediaLinkId;
            responseLink.LinkOrderId = link.LinkOrderId;
            responseLink.Link = link.Link.AbsoluteUri;
        }
    }
}