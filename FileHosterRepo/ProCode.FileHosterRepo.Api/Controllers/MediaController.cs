using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProCode.FileHosterRepo.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")] // "[controller]" adds "/Users" to link. Instead "/Login" we get "/Users/Login".
    public class MediaController : BaseController
    {
        #region Constructor
        public MediaController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
            : base(context, authenticationManager) { }
        #endregion

        #region Actions
        [HttpPost("Add")]
        public async Task<ActionResult<Model.Response.Media>> Add(Model.Request.Media newMedia)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                Model.Response.Media responseMedia;
                try
                {
                    // Media
                    var newDbMedia = await SaveMediaAsync(newMedia);
                    responseMedia = MapMedia(newDbMedia, loggedUser);

                    // Parts
                    var newDbMediaParts = await SaveMediaPartAsync(newMedia.Parts, newDbMedia.MediaId);
                    responseMedia.Parts = MapMediaParts(newDbMediaParts, loggedUser);

                    // Version
                    var newDbMediaVersion = await SaveMediaVersionAsync(newMedia.Parts, newDbMediaParts);

                    // Links
                    var newDbMediaLinks = await SaveMediaLinksAsync(newMedia.Parts, newDbMediaParts);
                    foreach (var part in responseMedia.Parts)
                    {
                        //part.Links = MapMediaLinks(newDbMediaLinks.Where(link => link.MediaPartId == part.MediaPartId));
                    }

                    return Ok(responseMedia);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return GetUnauthorizedLoginResponse();
            }
        }
        #endregion

        #region Methods
        private async Task<Dal.Model.Media> SaveMediaAsync(Model.Request.Media newMedia)
        {
            var newDbMedia = await context.Medias.AddAsync(new Dal.Model.Media
            {
                Name = newMedia.Name,
                Description = newMedia.Description,
                ReferenceLink = new Uri(newMedia.ReferenceLink),
                UserId = User.GetUserId()
            });
            await context.SaveChangesAsync();   // Save to get MediaId.
            return newDbMedia.Entity;
        }

        private static Model.Response.Media MapMedia(Dal.Model.Media newDbMedia, Dal.Model.User loggedUser)
        {
            return new Model.Response.Media
            {
                MediaId = newDbMedia.MediaId,
                Name = newDbMedia.Name,
                Description = newDbMedia.Description,
                ReferenceLink = newDbMedia.ReferenceLink.AbsoluteUri,
                User = newDbMedia.User.MapReponseUser()
            };
        }

        private async Task<IEnumerable<Dal.Model.MediaPart>> SaveMediaPartAsync(IEnumerable<Model.Request.MediaPart> newMediaParts, int mediaId)
        {
            await context.MediaParts.AddRangeAsync(newMediaParts.Select(part =>
              new Dal.Model.MediaPart
              {
                  MediaId = mediaId,
                  Season = part.Season,
                  Episode = part.Episode,
                  Name = part.Name,
                  Description = part.Description,
                  ReferenceLink = new Uri(part.ReferenceLink),
                  UserId = User.GetUserId(),
                  Created = DateTime.Now
              }));
            await context.SaveChangesAsync();
            return context.MediaParts.Where(part => part.MediaId == mediaId);
        }

        private static IEnumerable<Model.Response.MediaPart> MapMediaParts(IEnumerable<Dal.Model.MediaPart> newDbMediaParts, Dal.Model.User loggedUser)
        {
            return newDbMediaParts.Select(part => new Model.Response.MediaPart
            {
                MediaPartId = part.MediaPartId,
                Season = part.Season,
                Episode = part.Episode,
                Name = part.Name,
                Description = part.Description,
                ReferenceLink = part.ReferenceLink.AbsoluteUri,
                User = part.User.MapReponseUser()
            }).ToList();
        }

        private async Task<IEnumerable<Dal.Model.MediaVersion>> SaveMediaVersionAsync(IEnumerable<Model.Request.MediaPart> requestParts,
            IEnumerable<Dal.Model.MediaPart> dbParts)
        {
            var newDbVersion = dbParts.Select(dbPart =>
                new Dal.Model.MediaVersion
                {
                    MediaPartId = dbPart.MediaPartId,
                    VersionComment = requestParts.Where(p => p.Season == dbPart.Season && p.Episode == dbPart.Episode).FirstOrDefault().Version.VersionComment,
                    UserId = User.GetUserId(),
                    Created = DateTime.Now
                });
            await context.MediaVersions.AddRangeAsync(newDbVersion);
            await context.SaveChangesAsync();
            return context.MediaVersions.Where(v => newDbVersion.Any(newVer => newVer.MediaVersionId == v.MediaVersionId));
        }

        private void MapMediaVersion()
        {

        }

        private async Task<IEnumerable<Dal.Model.MediaLink>> SaveMediaLinksAsync(IEnumerable<Model.Request.MediaPart> newMediaParts, IEnumerable<Dal.Model.MediaPart> mediaParts)
        {
            /*
            var existingMediaLinks = context.MediaLinks.Where(l => mediaParts.Any(p => p.MediaPartId == 1 ));
            //int maxVersion = existingMediaLinks.Any() ? existingMediaLinks.Max(mp => mp.MediaLinkVersionId) : 1;

            foreach (var mediaPart in mediaParts)
                await context.MediaLinks.AddRangeAsync(newMediaParts.Where(p => p.Season == mediaPart.Season && p.Episode == mediaPart.Episode).FirstOrDefault().Links.Select(l =>
                    new Dal.Model.MediaLink
                    {
                        //MediaPartId = mediaPart.MediaPartId,
                        //MediaLinkVersionId = maxVersion,
                        LinkOrderId = l.LinkId,
                        Link = new Uri(l.Link),
                        Created = DateTime.Now,
                        UserId = User.GetUserId()
                    }));
            await context.SaveChangesAsync();

            return context.MediaLinks.Where(l => mediaParts.Any(p => p.MediaPartId == 1));
            */
            throw new NotImplementedException();
        }

        private static IEnumerable<Model.Response.MediaLink> MapMediaLinks(IEnumerable<Dal.Model.MediaLink> mediaLinks)
        {
            return mediaLinks.Select(link => new Model.Response.MediaLink
            {
                MediaLinkId = link.MediaLinkId,
                LinkId = link.LinkOrderId,
                Link = link.Link.AbsoluteUri
            }).ToList();
        }
        #endregion
    }
}