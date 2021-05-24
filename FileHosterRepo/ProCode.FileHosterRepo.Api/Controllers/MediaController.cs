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
                    var addedMediaHeader = await AddOrUpdateHeaderAsync(requestedHeader, loggedUser);
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

        [HttpGet("Get/{mediaHeaderId}")]
        public async Task<ActionResult<Model.Response.MediaHeader>> Get(int mediaHeaderId)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                try
                {
                    return Ok(await BuildResponseHeaderAsync(mediaHeaderId));
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

        [HttpPost("Update")]
        public async Task<ActionResult<Model.Response.MediaHeader>> Update(Model.Request.MediaHeader requestedHeader)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                try
                {
                    var addedMediaHeader = await AddOrUpdateHeaderAsync(requestedHeader, loggedUser);
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
        private async Task<Dal.Model.MediaHeader> AddOrUpdateHeaderAsync(Model.Request.MediaHeader requestedHeader, Dal.Model.User loggedUser)
        {
            Dal.Model.MediaHeader header = requestedHeader.MediaHeaderId == null ? new Dal.Model.MediaHeader() :
                await context.MediaHeaders.SingleOrDefaultAsync(h => h.MediaHeaderId == (int)requestedHeader.MediaHeaderId);

            // Change all values except PK (MediaHeaderId)
            header.Name = requestedHeader.Name;
            header.Description = requestedHeader.Description;
            header.ReferenceLink = !string.IsNullOrWhiteSpace(requestedHeader.ReferenceLink) ? new Uri(requestedHeader.ReferenceLink) : null;
            header.UserId = loggedUser.UserId;

            if (requestedHeader.MediaHeaderId == null)
                await context.MediaHeaders.AddAsync(header);
            await context.SaveChangesAsync();                       // Save to get MediaId.
            requestedHeader.MediaHeaderId = header.MediaHeaderId;   // Populate id in request structure, so it can be used later if needed.

            // Header tags
            await AddHeaderTagsAsync(requestedHeader);

            // Parts
            await AddPartsAsync(requestedHeader, header, loggedUser);

            return header;
        }

        private async Task AddHeaderTagsAsync(Model.Request.MediaHeader requestedHeader)
        {
            if (requestedHeader.Tags != null)
                foreach (var reqHeaderTag in requestedHeader.Tags)
                {
                    int mediaTagId;
                    var oldTag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqHeaderTag.Name);
                    if (oldTag == null)
                    {
                        var newTag = new Dal.Model.MediaTag { Name = reqHeaderTag.Name };
                        var savedTag = await context.MediaTags.AddAsync(newTag);
                        await context.SaveChangesAsync();
                        mediaTagId = savedTag.Entity.MediaTagId;
                    }
                    else
                    {
                        mediaTagId = oldTag.MediaTagId;
                    }

                    // Header-Tag link
                    await context.MediaHeaderTags.AddAsync(new Dal.Model.MediaHeaderTag
                    {
                        MediaHeaderId = (int)requestedHeader.MediaHeaderId,
                        MediaTagId = mediaTagId
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
                    Dal.Model.MediaPart part = requestedPart.MediaPartId == null ? new Dal.Model.MediaPart() :
                        await context.MediaParts.SingleOrDefaultAsync(p => p.MediaPartId == requestedPart.MediaPartId);

                    part.MediaHeaderId = addedHeader.MediaHeaderId;
                    part.Season = requestedPart.Season;
                    part.Episode = requestedPart.Episode;
                    part.Name = requestedPart.Name;
                    part.Description = requestedPart.Description;
                    part.ReferenceLink = !string.IsNullOrWhiteSpace(requestedPart.ReferenceLink) ? new Uri(requestedPart.ReferenceLink) : null;
                    part.UserId = loggedUser.UserId;
                    part.Created = DateTime.Now;

                    if (requestedPart.MediaPartId == null)
                        await context.MediaParts.AddAsync(part);
                    await context.SaveChangesAsync();
                    requestedPart.MediaPartId = part.MediaPartId;

                    // Part tags
                    await AddPartTagsAsync(requestedPart);

                    // Media versions and tags
                    await AddVersionAsync(requestedPart, part, loggedUser);
                }
        }

        private async Task AddPartTagsAsync(Model.Request.MediaPart reqPart)
        {
            if (reqPart.Tags != null)
            {
                foreach (var reqPartTag in reqPart.Tags)
                {
                    int mediaTagId;
                    var oldTag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqPartTag.Name);
                    if (oldTag == null)
                    {
                        var newTag = new Dal.Model.MediaTag { Name = reqPartTag.Name };
                        var savedTag = await context.MediaTags.AddAsync(newTag);
                        await context.SaveChangesAsync();
                        mediaTagId = savedTag.Entity.MediaTagId;
                    }
                    else
                    {
                        mediaTagId = oldTag.MediaTagId;
                    }

                    // Part-Tag link
                    await context.MediaPartTags.AddAsync(new Dal.Model.MediaPartTag
                    {
                        MediaPartId = (int)reqPart.MediaPartId,
                        MediaTagId = mediaTagId
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
                    int mediaTagId;
                    var oldTag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqVersionTag.Name);
                    if (oldTag == null)
                    {
                        var newTag = new Dal.Model.MediaTag { Name = reqVersionTag.Name };
                        var savedTag = await context.MediaTags.AddAsync(newTag);
                        await context.SaveChangesAsync();
                        mediaTagId = savedTag.Entity.MediaTagId;
                    }
                    else
                    {
                        mediaTagId = oldTag.MediaTagId;
                    }

                    // Part-Tag link
                    await context.MediaVersionTags.AddAsync(new Dal.Model.MediaVersionTag
                    {
                        MediaVersionId = (int)requestedPart.Version.MediaVersionId,
                        MediaTagId = mediaTagId
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
                        MediaVersionId = (int)requestedVersion.MediaVersionId,
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
            // Header tags
            responseHeader.Tags = new List<Model.Response.MediaTag>();
            foreach (var headerTag in await context.MediaHeaderTags.Where(ht => ht.MediaHeaderId == mediaHeaderId).ToListAsync())
            {
                Model.Response.MediaTag newResponseHeaderTag = new Model.Response.MediaTag { Name = (await context.MediaTags.SingleOrDefaultAsync(t => t.MediaTagId == headerTag.MediaTagId)).Name };
                ((List<Model.Response.MediaTag>)responseHeader.Tags).Add(newResponseHeaderTag);
            }

            //Parts
            responseHeader.Parts = new List<Model.Response.MediaPart>();
            var parts = await context.MediaParts.Where(p => p.MediaHeaderId == header.MediaHeaderId).ToListAsync();
            foreach (var part in parts)
            {
                Model.Response.MediaPart newResponsePart = new();
                part.MapResponseMediaPart(ref newResponsePart);
                ((List<Model.Response.MediaPart>)responseHeader.Parts).Add(newResponsePart);
                // Part tags
                newResponsePart.Tags = new List<Model.Response.MediaTag>();
                foreach (var partTag in await context.MediaPartTags.Where(pt => pt.MediaPartId == part.MediaPartId).ToListAsync())
                {
                    Model.Response.MediaTag newResponsePartTag = new Model.Response.MediaTag { Name = (await context.MediaTags.SingleOrDefaultAsync(t => t.MediaTagId == partTag.MediaTagId)).Name };
                    ((List<Model.Response.MediaTag>)newResponsePart.Tags).Add(newResponsePartTag);
                }

                // Versions
                newResponsePart.Versions = new List<Model.Response.MediaVersion>();
                var versions = await context.MediaVersions.Where(v => v.MediaPartId == part.MediaPartId).ToListAsync();
                foreach (var version in versions)
                {
                    Model.Response.MediaVersion newResponseVersion = new();
                    version.MapResponseMediaVersion(ref newResponseVersion);
                    ((List<Model.Response.MediaVersion>)newResponsePart.Versions).Add(newResponseVersion);
                    // Version tags
                    newResponseVersion.Tags = new List<Model.Response.MediaTag>();
                    foreach (var versionTag in await context.MediaVersionTags.Where(pt => pt.MediaVersionId == version.MediaVersionId).ToListAsync())
                    {
                        Model.Response.MediaTag newResponseVersionTag = new Model.Response.MediaTag { Name = (await context.MediaTags.SingleOrDefaultAsync(t => t.MediaTagId == versionTag.MediaTagId)).Name };
                        ((List<Model.Response.MediaTag>)newResponseVersion.Tags).Add(newResponseVersionTag);
                    }

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