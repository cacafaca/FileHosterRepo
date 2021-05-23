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
        public async Task<ActionResult<Model.Response.MediaHeader>> Add(Model.Request.MediaHeader requestedMedia)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                try
                {
                    // Header
                    var newMedia = await context.Medias.AddAsync(new Dal.Model.MediaHeader
                    {
                        Name = requestedMedia.Name,
                        Description = requestedMedia.Description,
                        ReferenceLink = new Uri(requestedMedia.ReferenceLink),
                        UserId = loggedUser.UserId
                    });
                    await context.SaveChangesAsync();                           // Save to get MediaId.
                    requestedMedia.MediaHeaderId = newMedia.Entity.MediaHeaderId;         // Populate id in request structure, so it can be used later if needed.

                    // Header tags
                    if (requestedMedia.Tags != null)
                        foreach (var reqHeaderTag in requestedMedia.Tags)
                        {
                            var oldTag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqHeaderTag.Name);
                            if (oldTag == null)
                            {
                                var newTag = new Dal.Model.MediaTag { Name = reqHeaderTag.Name };
                                var savedTag = await context.MediaTags.AddAsync(newTag);
                                await context.SaveChangesAsync();
                                reqHeaderTag.MediaTagId = savedTag.Entity.MediaTagId;
                            }
                            else
                            {
                                reqHeaderTag.MediaTagId = oldTag.MediaTagId;
                            }

                            // Header-Tag link
                            await context.MediaHeaderTags.AddAsync(new Dal.Model.MediaHeaderTag
                            {
                                MediaHeaderId = requestedMedia.MediaHeaderId,
                                MediaTagId = reqHeaderTag.MediaTagId
                            });
                            await context.SaveChangesAsync();
                        }

                    // Parts
                    if (requestedMedia.Parts != null)
                        foreach (var reqPart in requestedMedia.Parts)
                        {
                            var newPart = await context.MediaParts.AddAsync(new Dal.Model.MediaPart
                            {
                                MediaHeaderId = newMedia.Entity.MediaHeaderId,
                                Season = reqPart.Season,
                                Episode = reqPart.Episode,
                                Name = reqPart.Name,
                                Description = reqPart.Description,
                                ReferenceLink = new Uri(reqPart.ReferenceLink),
                                UserId = loggedUser.UserId,
                                Created = DateTime.Now
                            });
                            await context.SaveChangesAsync();
                            reqPart.MediaPartId = newPart.Entity.MediaPartId;

                            // Part tags
                            if (reqPart.Tags != null)
                            {
                                foreach (var reqPartTag in reqPart.Tags)
                                {
                                    var oldTag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqPartTag.Name);
                                    if (oldTag == null)
                                    {
                                        var newTag = new Dal.Model.MediaTag { Name = reqPartTag.Name };
                                        var savedTag = await context.MediaTags.AddAsync(newTag);
                                        await context.SaveChangesAsync();
                                        reqPartTag.MediaTagId = savedTag.Entity.MediaTagId;
                                    }
                                    else
                                    {
                                        reqPartTag.MediaTagId = oldTag.MediaTagId;
                                    }

                                    // Part-Tag link
                                    await context.MediaPartTags.AddAsync(new Dal.Model.MediaPartTag
                                    {
                                        MediaPartId = reqPart.MediaPartId,
                                        MediaTagId = reqPartTag.MediaTagId
                                    });
                                    await context.SaveChangesAsync();
                                }
                            }

                            // Media versions
                            var newVersion = await context.MediaVersions.AddAsync(new Dal.Model.MediaVersion
                            {
                                MediaPartId = newPart.Entity.MediaPartId,
                                VersionComment = reqPart.Version.VersionComment,
                                UserId = loggedUser.UserId,
                                Created = DateTime.Now
                            });
                            await context.SaveChangesAsync();
                            reqPart.Version.MediaVersionId = newVersion.Entity.MediaVersionId;

                            // Media version tags
                            if (reqPart.Version.Tags != null)
                            {
                                foreach (var reqVersionTag in reqPart.Version.Tags)
                                {
                                    var oldTag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqVersionTag.Name);
                                    if (oldTag == null)
                                    {
                                        var newTag = new Dal.Model.MediaTag { Name = reqVersionTag.Name };
                                        var savedTag = await context.MediaTags.AddAsync(newTag);
                                        await context.SaveChangesAsync();
                                        reqVersionTag.MediaTagId = savedTag.Entity.MediaTagId;
                                    }
                                    else
                                    {
                                        reqVersionTag.MediaTagId = oldTag.MediaTagId;
                                    }

                                    // Part-Tag link
                                    await context.MediaVersionTags.AddAsync(new Dal.Model.MediaVersionTag
                                    {
                                        MediaVersionId = reqPart.Version.MediaVersionId,
                                        MediaTagId = reqVersionTag.MediaTagId
                                    });
                                    await context.SaveChangesAsync();
                                }
                            }

                            // Links
                            if (reqPart.Version.Links != null)
                            {
                                int linkIndex = 1;
                                foreach (var reqLink in reqPart.Version.Links)
                                {
                                    var newLink = context.MediaLinks.Add(new Dal.Model.MediaLink
                                    {
                                        MediaVersionId = reqPart.Version.MediaVersionId,
                                        LinkOrderId = linkIndex++,
                                        Link = new Uri(reqLink.Link),
                                        Created = DateTime.Now,
                                        UserId = loggedUser.UserId
                                    });
                                }
                            }
                        }


                    return Ok(null);
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

        private async Task GroupApproach(Model.Request.MediaHeader requestedMedia, Dal.Model.User loggedUser)
        {
            // Add Media
            var addedMedia = await AddMediaAsync(requestedMedia, loggedUser);
            // Add Parts
            var addedParts = await AddMediaPartAsync(requestedMedia.Parts, addedMedia.MediaHeaderId, loggedUser);
            // Add Tags
            var addedTags = await AddTagsAsync(requestedMedia.Parts);
            // Add Version
            var addedVersions = await AddMediaVersionAsync(requestedMedia.Parts, addedParts, loggedUser);
            // Version-Tag link
            //var addedVersionTags = await SaveVersionTagsAsync(requestedMedia.Parts, addedParts);
            // Links
            //var addedLinks = await SaveMediaLinksAsync(requestedMedia.Parts, addedParts);

            // Map everything.
            Model.Response.MediaHeader responseMedia = addedMedia.MapResponseMedia();
            responseMedia.Parts = addedParts.Select(dbPart => dbPart.MapResponseMediaPart()).ToList();
        }
        private async Task<Dal.Model.MediaHeader> AddMediaAsync(Model.Request.MediaHeader requestedMedia, Dal.Model.User loggedUser)
        {
            var savedMedia = await context.Medias.AddAsync(new Dal.Model.MediaHeader
            {
                Name = requestedMedia.Name,
                Description = requestedMedia.Description,
                ReferenceLink = new Uri(requestedMedia.ReferenceLink),
                UserId = loggedUser.UserId
            });
            await context.SaveChangesAsync();                           // Save to get MediaId.
            requestedMedia.MediaHeaderId = savedMedia.Entity.MediaHeaderId;         // Populate id in request structure, so it can be used later if needed.
            return savedMedia.Entity;
        }

        private async Task<IEnumerable<Dal.Model.MediaPart>> AddMediaPartAsync(IEnumerable<Model.Request.MediaPart> requestedParts, int mediaId,
            Dal.Model.User loggedUser)
        {
            Dictionary<int, int> requestedAndSavedLink = new();
            List<Dal.Model.MediaPart> savedParts = new();

            foreach (var reqPart in requestedParts)
            {
                var newPart = await context.MediaParts.AddAsync(new Dal.Model.MediaPart
                {
                    MediaHeaderId = mediaId,
                    Season = reqPart.Season,
                    Episode = reqPart.Episode,
                    Name = reqPart.Name,
                    Description = reqPart.Description,
                    ReferenceLink = new Uri(reqPart.ReferenceLink),
                    UserId = loggedUser.UserId,
                    Created = DateTime.Now
                });
                savedParts.Add(newPart.Entity);
                requestedAndSavedLink.Add(reqPart.GetHashCode(), newPart.Entity.GetHashCode());
            }
            await context.SaveChangesAsync();

            // Update MediaPartId in request, because link between request and DB object will be needed later.
            foreach (var reqPart in requestedParts)
                reqPart.MediaPartId = savedParts.Single(p => p.GetHashCode() == requestedAndSavedLink[reqPart.GetHashCode()]).MediaPartId;

            return savedParts;
        }

        private async Task<IEnumerable<Dal.Model.MediaTag>> AddTagsAsync(IEnumerable<Model.Request.MediaPart> reqParts)
        {
            // Extricate new tags (that do not exists in DB), and save.
            var requestedTags = reqParts.SelectMany(part => part.Version.Tags)
                .GroupBy(tag => tag.Name)
                .Select(sameTags => sameTags.FirstOrDefault()).ToList();
            List<Dal.Model.MediaTag> existingTags = new();
            foreach (var requestedTag in requestedTags)
            {
                var foundTag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == requestedTag.Name);
                if (foundTag != null)
                    existingTags.Add(foundTag);
            }
            var tagsToSave = requestedTags.Where(rt => !existingTags.Any(et => et.Name == rt.Name));
            var savedTags = tagsToSave.Select(t => new Dal.Model.MediaTag { Name = t.Name });
            await context.MediaTags.AddRangeAsync(savedTags);
            await context.SaveChangesAsync();

            return existingTags.Union(savedTags).ToList();
        }

        private async Task<IEnumerable<Dal.Model.MediaVersion>> AddMediaVersionAsync(IEnumerable<Model.Request.MediaPart> requestParts,
            IEnumerable<Dal.Model.MediaPart> savedParts, Dal.Model.User loggedUser)
        {
            var savedVersions = savedParts.Select(savedPart =>
                new Dal.Model.MediaVersion
                {
                    MediaPartId = savedPart.MediaPartId,
                    VersionComment = requestParts.SingleOrDefault(p => p.Season == savedPart.Season && p.Episode == savedPart.Episode).Version.VersionComment,
                    UserId = loggedUser.UserId,
                    Created = DateTime.Now
                });
            await context.MediaVersions.AddRangeAsync(savedVersions);
            await context.SaveChangesAsync();
            return savedVersions;
        }

        private async Task<IEnumerable<Dal.Model.MediaVersionTag>> SaveVersionTagsAsync(IEnumerable<Model.Request.MediaPart> reqParts,
            IEnumerable<Dal.Model.MediaPart> savedParts, IEnumerable<Dal.Model.MediaVersion> savedVersions,
            IEnumerable<Dal.Model.MediaTag> savedTags)
        {
            var savedVersionTags = savedVersions.Join(savedParts, v => v.MediaPartId, p => p.MediaPartId, (v, p) => new Dal.Model.MediaVersionTag
            {
                MediaVersionId = v.MediaVersionId,
                MediaTagId = 1
            });
            await context.MediaVersionTags.AddRangeAsync(savedVersionTags);
            await context.SaveChangesAsync();
            return savedVersionTags;
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