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

        /// <summary>
        /// Parts property will be null.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public static Model.Response.MediaHeader MapResponseMedia(this Dal.Model.MediaHeader media)
        {
            return new Model.Response.MediaHeader
            {
                MediaHeaderId = media.MediaHeaderId,
                Name = media.Name,
                Description = media.Description,
                ReferenceLink = media.ReferenceLink.AbsoluteUri,
                User = media.User.MapReponseUser()
            };
        }

        /// <summary>
        /// Links will be null.
        /// </summary>
        /// <param name="mediaPart"></param>
        /// <returns></returns>
        public static Model.Response.MediaPart MapResponseMediaPart(this Dal.Model.MediaPart mediaPart)
        {
            return new Model.Response.MediaPart
            {
                MediaPartId = mediaPart.MediaPartId,
                Season = mediaPart.Season,
                Episode = mediaPart.Episode,
                Name = mediaPart.Name,
                Description = mediaPart.Description,
                ReferenceLink = mediaPart.ReferenceLink.AbsoluteUri,
                User = mediaPart.User.MapReponseUser()
            };
        }

        public static Model.Response.MediaVersion MapResponseMediaVersion(this Dal.Model.MediaVersion mediaVersion)
        {
            return new Model.Response.MediaVersion
            {
                MediaLinkVersionId = mediaVersion.MediaVersionId,
                VersionComment = mediaVersion.VersionComment,
            };
        }

        public static Model.Response.MediaTag MapResponseMediaTag(this Dal.Model.MediaTag mediaTag)
        {
            return new Model.Response.MediaTag
            {
                Name= mediaTag.Name
            };
        }
    }
}
