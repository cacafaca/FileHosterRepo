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
        public async Task<ActionResult<Model.Response.MediaHeader>> Add(Model.Request.MediaHeader requestedHeader)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                try
                {
                    var addedMediaHeader = await AddHeaderAsync(requestedHeader, loggedUser);

                    return Ok(await BuildResponseHeaderAsync(addedMediaHeader.MediaHeaderId));
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
        private async Task<Dal.Model.MediaHeader> AddHeaderAsync(Model.Request.MediaHeader requestedHeader, Dal.Model.User loggedUser)
        {
            var addedHeader = await context.MediaHeaders.AddAsync(new Dal.Model.MediaHeader
            {
                Name = requestedHeader.Name,
                Description = requestedHeader.Description,
                ReferenceLink = !string.IsNullOrWhiteSpace(requestedHeader.ReferenceLink) ? new Uri(requestedHeader.ReferenceLink) : null,
                UserId = loggedUser.UserId
            });
            await context.SaveChangesAsync();                                   // Save to get MediaId.
            requestedHeader.MediaHeaderId = addedHeader.Entity.MediaHeaderId;   // Populate id in request structure, so it can be used later if needed.

            // Header tags
            await AddHeaderTagsAsync(requestedHeader);

            // Parts
            await AddPartsAsync(requestedHeader, addedHeader.Entity, loggedUser);

            return addedHeader.Entity;
        }

        private async Task AddHeaderTagsAsync(Model.Request.MediaHeader requestedHeader)
        {
            if (requestedHeader.Tags != null)
                foreach (var reqHeaderTag in requestedHeader.Tags)
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
                        MediaHeaderId = requestedHeader.MediaHeaderId,
                        MediaTagId = reqHeaderTag.MediaTagId
                    });
                    await context.SaveChangesAsync();
                }
        }

        private async Task AddPartsAsync(Model.Request.MediaHeader requestedHeader,
            Dal.Model.MediaHeader addedHeader, Dal.Model.User loggedUser)
        {
            if (requestedHeader.Parts != null)
                foreach (var requestedPart in requestedHeader.Parts)
                {
                    var addedPart = await context.MediaParts.AddAsync(new Dal.Model.MediaPart
                    {
                        MediaHeaderId = addedHeader.MediaHeaderId,
                        Season = requestedPart.Season,
                        Episode = requestedPart.Episode,
                        Name = requestedPart.Name,
                        Description = requestedPart.Description,
                        ReferenceLink = !string.IsNullOrWhiteSpace(requestedPart.ReferenceLink) ? new Uri(requestedPart.ReferenceLink) : null,
                        UserId = loggedUser.UserId,
                        Created = DateTime.Now
                    });
                    await context.SaveChangesAsync();
                    requestedPart.MediaPartId = addedPart.Entity.MediaPartId;

                    // Part tags
                    await AddPartTagsAsync(requestedPart);

                    // Media versions and tags
                    await AddVersionAsync(requestedPart, addedPart.Entity, loggedUser);
                }
        }

        private async Task AddPartTagsAsync(Model.Request.MediaPart reqPart)
        {
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
        }

        private async Task AddVersionAsync(Model.Request.MediaPart requestedPart,
            Dal.Model.MediaPart addedPart, Dal.Model.User loggedUser)
        {
            var newVersion = await context.MediaVersions.AddAsync(new Dal.Model.MediaVersion
            {
                MediaPartId = addedPart.MediaPartId,
                VersionComment = requestedPart.Version.VersionComment,
                UserId = loggedUser.UserId,
                Created = DateTime.Now
            });
            await context.SaveChangesAsync();
            requestedPart.Version.MediaVersionId = newVersion.Entity.MediaVersionId;

            // Media version tags
            await AddVersionTagsAsync(requestedPart);

            // Links
            await AddLinksAsync(requestedPart.Version, loggedUser);
        }

        private async Task AddVersionTagsAsync(Model.Request.MediaPart requestedPart)
        {
            if (requestedPart.Version.Tags != null)
            {
                foreach (var reqVersionTag in requestedPart.Version.Tags)
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
                        MediaVersionId = requestedPart.Version.MediaVersionId,
                        MediaTagId = reqVersionTag.MediaTagId
                    });
                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task AddLinksAsync(Model.Request.MediaVersion requestedVersion, Dal.Model.User loggedUser)
        {
            if (requestedVersion.Links != null)
            {
                int linkIndex = 1;
                foreach (var requestedLink in requestedVersion.Links)
                {
                    var newLink = context.MediaLinks.Add(new Dal.Model.MediaLink
                    {
                        MediaVersionId = requestedVersion.MediaVersionId,
                        LinkOrderId = linkIndex++,
                        Link = new Uri(requestedLink.Link),
                        Created = DateTime.Now,
                        UserId = loggedUser.UserId
                    });
                    await context.SaveChangesAsync();
                    requestedLink.MediaLinkId = newLink.Entity.MediaLinkId;
                }
            }
        }

        private async Task<Model.Response.MediaHeader> BuildResponseHeaderAsync(int mediaHeaderId)
        {
            Model.Response.MediaHeader responseHeader = new();

            // Header
            var header = await context.MediaHeaders.SingleOrDefaultAsync(h => h.MediaHeaderId == mediaHeaderId);
            header.MapResponseMedia(ref responseHeader);

            //Parts
            responseHeader.Parts = new List<Model.Response.MediaPart>();
            var parts = await context.MediaParts.Where(p => p.MediaHeaderId == header.MediaHeaderId).ToListAsync();
            foreach (var part in parts)
            {
                Model.Response.MediaPart newResponsePart = new();
                part.MapResponseMediaPart(ref newResponsePart);
                ((List<Model.Response.MediaPart>)responseHeader.Parts).Add(newResponsePart);

                // Versions
                newResponsePart.Versions = new List<Model.Response.MediaVersion>();
                var versions = await context.MediaVersions.Where(v => v.MediaPartId == part.MediaPartId).ToListAsync();
                foreach (var version in versions)
                {
                    Model.Response.MediaVersion newResponseVersion = new();
                    version.MapResponseMediaVersion(ref newResponseVersion);
                    ((List<Model.Response.MediaVersion>)newResponsePart.Versions).Add(newResponseVersion);

                    // Links
                    newResponseVersion.Links = new List<Model.Response.MediaLink>();
                    var links = await context.MediaLinks.Where(l => l.MediaVersionId == version.MediaVersionId).ToListAsync();
                    foreach (var link in links)
                    {
                        Model.Response.MediaLink newResponseLink = new();
                        link.MapResponseMediaLink(ref newResponseLink);
                        ((List<Model.Response.MediaLink>)newResponseVersion.Links).Add(newResponseLink);
                    }
                }
            }
            return responseHeader;
        }
        #endregion
    }
}