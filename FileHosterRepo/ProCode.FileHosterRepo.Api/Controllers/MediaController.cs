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
    [Route(Common.Routes.Media.ControlerName)] 
    public class MediaController : BaseController
    {
        #region Constructor
        public MediaController(Dal.DataAccess.FileHosterRepoContext context, IJwtAuthenticationManager authenticationManager)
            : base(context, authenticationManager) { }
        #endregion

        #region Actions
        [HttpPost]
        [Route(Common.Routes.Media.Add)]
        public async Task<ActionResult<Common.Api.Response.MediaHeader>> Add(Common.Api.Request.MediaHeader requestedHeader)
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

        [HttpGet]
        [Route(Common.Routes.Media.Get)]
        public async Task<ActionResult<Common.Api.Response.MediaHeader>> Get(int mediaHeaderId)
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

        [HttpPost]
        [Route(Common.Routes.Media.Update)]
        public async Task<ActionResult<Common.Api.Response.MediaHeader>> Update(Common.Api.Request.MediaHeader requestedHeader)
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

        /// <summary>
        /// Returns last 10 medias.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Common.Routes.Media.Last10)]
        public async Task<ActionResult<IEnumerable<Common.Api.Response.MediaHeader>>> Last10()
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                return await GetLast10();
            }
            else
            {
                return GetUnauthorizedLoginResponse();
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route(Common.Routes.Media.Last10Anonymous)]
        public async Task<ActionResult<IEnumerable<Common.Api.Response.MediaHeader>>> Last10Anonymous()
        {
            return await GetLast10();
        }
        #endregion

        #region Methods
        private async Task<Dal.Model.MediaHeader> AddOrUpdateHeaderAsync(Common.Api.Request.MediaHeader requestedHeader, Dal.Model.User loggedUser)
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

        private async Task AddHeaderTagsAsync(Common.Api.Request.MediaHeader requestedHeader)
        {
            if (requestedHeader.Tags != null)
            {
                List<Dal.Model.MediaTag> tagsInRequestHeader = new();
                foreach (var reqHeaderTag in requestedHeader.Tags)
                {
                    var tag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqHeaderTag.Name);
                    if (tag == null)
                    {
                        tag = new Dal.Model.MediaTag { Name = reqHeaderTag.Name };
                        await context.MediaTags.AddAsync(tag);
                        await context.SaveChangesAsync();
                    }
                    tagsInRequestHeader.Add(tag);

                    // Header-Tag link
                    if (!await context.MediaHeaderTags.AnyAsync(ht => ht.MediaHeaderId == (int)requestedHeader.MediaHeaderId && ht.MediaTagId == tag.MediaTagId))
                    {
                        await context.MediaHeaderTags.AddAsync(new Dal.Model.MediaHeaderTag
                        {
                            MediaHeaderId = (int)requestedHeader.MediaHeaderId,
                            MediaTagId = tag.MediaTagId
                        });
                        await context.SaveChangesAsync();
                    }
                }

                // Remove tags that are not in request.
                var allTagsForHeader = await context.MediaHeaderTags.Where(ht => ht.MediaHeaderId == (int)requestedHeader.MediaHeaderId).ToListAsync();
                context.MediaHeaderTags.RemoveRange(allTagsForHeader.Where(headerTag => !tagsInRequestHeader.Any(requestTag => requestTag.MediaTagId == headerTag.MediaTagId)));
                await context.SaveChangesAsync();
            }
        }

        private async Task AddPartsAsync(Common.Api.Request.MediaHeader requestedHeader,
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

        private async Task AddPartTagsAsync(Common.Api.Request.MediaPart requestedPart)
        {
            if (requestedPart.Tags != null)
            {
                List<Dal.Model.MediaTag> tagsInRequestPart = new();
                foreach (var reqPartTag in requestedPart.Tags)
                {
                    var tag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqPartTag.Name);
                    if (tag == null)
                    {
                        tag = new Dal.Model.MediaTag { Name = reqPartTag.Name };
                        await context.MediaTags.AddAsync(tag);
                        await context.SaveChangesAsync();
                    }
                    tagsInRequestPart.Add(tag);

                    // Part-Tag link
                    if (!await context.MediaPartTags.AnyAsync(pt => pt.MediaPartId == (int)requestedPart.MediaPartId && pt.MediaTagId == tag.MediaTagId))
                    {
                        await context.MediaPartTags.AddAsync(new Dal.Model.MediaPartTag
                        {
                            MediaPartId = (int)requestedPart.MediaPartId,
                            MediaTagId = tag.MediaTagId
                        });
                        await context.SaveChangesAsync();
                    }
                }

                // Remove tags that are not in request.
                var allTagsForPart = await context.MediaPartTags.Where(pt => pt.MediaPartId == (int)requestedPart.MediaPartId).ToListAsync();
                context.MediaPartTags.RemoveRange(allTagsForPart.Where(partTag => !tagsInRequestPart.Any(requestTag => requestTag.MediaTagId == partTag.MediaTagId)));
                await context.SaveChangesAsync();
            }
        }

        private async Task AddVersionAsync(Common.Api.Request.MediaPart requestedPart,
            Dal.Model.MediaPart addedPart, Dal.Model.User loggedUser)
        {
            var version = requestedPart.Version.MediaVersionId == null ? new Dal.Model.MediaVersion() :
                await context.MediaVersions.SingleOrDefaultAsync(v => v.MediaVersionId == requestedPart.Version.MediaVersionId);

            version.MediaPartId = addedPart.MediaPartId;
            version.VersionComment = requestedPart.Version.VersionComment;
            version.UserId = loggedUser.UserId;
            version.Created = DateTime.Now;

            if (requestedPart.Version.MediaVersionId == null)
                await context.MediaVersions.AddAsync(version);

            await context.SaveChangesAsync();
            requestedPart.Version.MediaVersionId = version.MediaVersionId;

            // Media version tags
            await AddVersionTagsAsync(requestedPart);

            // Links
            await AddLinksAsync(requestedPart.Version, loggedUser);
        }

        private async Task AddVersionTagsAsync(Common.Api.Request.MediaPart requestedPart)
        {
            if (requestedPart.Version.Tags != null)
            {
                List<Dal.Model.MediaTag> tagsInRequestVersion = new();
                foreach (var reqVersionTag in requestedPart.Version.Tags)
                {
                    var tag = await context.MediaTags.SingleOrDefaultAsync(t => t.Name == reqVersionTag.Name);
                    if (tag == null)
                    {
                        tag = new Dal.Model.MediaTag { Name = reqVersionTag.Name };
                        await context.MediaTags.AddAsync(tag);
                        await context.SaveChangesAsync();
                    }
                    tagsInRequestVersion.Add(tag);

                    // Version-Tag link
                    if (!await context.MediaVersionTags.AnyAsync(vt => vt.MediaVersionId == (int)requestedPart.Version.MediaVersionId && vt.MediaTagId == tag.MediaTagId))
                    {
                        await context.MediaVersionTags.AddAsync(new Dal.Model.MediaVersionTag
                        {
                            MediaVersionId = (int)requestedPart.Version.MediaVersionId,
                            MediaTagId = tag.MediaTagId
                        });
                        await context.SaveChangesAsync();
                    }
                }

                // Remove tags that are not in request.
                var allTagsForVersion = await context.MediaVersionTags.Where(vt => vt.MediaVersionId == (int)requestedPart.Version.MediaVersionId).ToListAsync();
                context.MediaVersionTags.RemoveRange(allTagsForVersion.Where(versionTag => !tagsInRequestVersion.Any(requestTag => requestTag.MediaTagId == versionTag.MediaTagId)));
                await context.SaveChangesAsync();
            }
        }

        private async Task AddLinksAsync(Common.Api.Request.MediaVersion requestedVersion, Dal.Model.User loggedUser)
        {
            if (requestedVersion.Links != null)
            {
                int linkIndex = 1;
                foreach (var requestedLink in requestedVersion.Links)
                {
                    var link = requestedLink.MediaLinkId == null ? new Dal.Model.MediaLink() :
                        await context.MediaLinks.SingleOrDefaultAsync(l => l.MediaLinkId == requestedLink.MediaLinkId);

                    link.MediaVersionId = (int)requestedVersion.MediaVersionId;
                    link.LinkOrderId = linkIndex++;
                    link.Link = new Uri(requestedLink.Link);
                    link.UserId = loggedUser.UserId;

                    if (requestedLink.MediaLinkId == null)
                        await context.MediaLinks.AddAsync(link);

                    await context.SaveChangesAsync();
                    requestedLink.MediaLinkId = link.MediaLinkId;
                }
            }
        }

        private async Task<Common.Api.Response.MediaHeader> BuildResponseHeaderAsync(int mediaHeaderId)
        {
            Common.Api.Response.MediaHeader responseHeader = new();

            // Header
            var header = await context.MediaHeaders.Include("User").SingleOrDefaultAsync(h => h.MediaHeaderId == mediaHeaderId);            
            header.MapResponseMedia(ref responseHeader);
            // Header tags
            responseHeader.Tags = new List<Common.Api.Response.MediaTag>();
            foreach (var headerTag in await context.MediaHeaderTags.Where(ht => ht.MediaHeaderId == mediaHeaderId).ToListAsync())
            {
                Common.Api.Response.MediaTag newResponseHeaderTag = new() { Name = (await context.MediaTags.SingleOrDefaultAsync(t => t.MediaTagId == headerTag.MediaTagId)).Name };
                ((List<Common.Api.Response.MediaTag>)responseHeader.Tags).Add(newResponseHeaderTag);
            }

            //Parts
            responseHeader.Parts = new List<Common.Api.Response.MediaPart>();
            var parts = await context.MediaParts.Where(p => p.MediaHeaderId == header.MediaHeaderId).ToListAsync();
            foreach (var part in parts)
            {
                Common.Api.Response.MediaPart newResponsePart = new();
                part.MapResponseMediaPart(ref newResponsePart);
                ((List<Common.Api.Response.MediaPart>)responseHeader.Parts).Add(newResponsePart);
                // Part tags
                newResponsePart.Tags = new List<Common.Api.Response.MediaTag>();
                foreach (var partTag in await context.MediaPartTags.Where(pt => pt.MediaPartId == part.MediaPartId).ToListAsync())
                {
                    Common.Api.Response.MediaTag newResponsePartTag = new() { Name = (await context.MediaTags.SingleOrDefaultAsync(t => t.MediaTagId == partTag.MediaTagId)).Name };
                    ((List<Common.Api.Response.MediaTag>)newResponsePart.Tags).Add(newResponsePartTag);
                }

                // Versions
                newResponsePart.Versions = new List<Common.Api.Response.MediaVersion>();
                var versions = await context.MediaVersions.Where(v => v.MediaPartId == part.MediaPartId).ToListAsync();
                foreach (var version in versions)
                {
                    Common.Api.Response.MediaVersion newResponseVersion = new();
                    version.MapResponseMediaVersion(ref newResponseVersion);
                    ((List<Common.Api.Response.MediaVersion>)newResponsePart.Versions).Add(newResponseVersion);
                    // Version tags
                    newResponseVersion.Tags = new List<Common.Api.Response.MediaTag>();
                    foreach (var versionTag in await context.MediaVersionTags.Where(pt => pt.MediaVersionId == version.MediaVersionId).ToListAsync())
                    {
                        Common.Api.Response.MediaTag newResponseVersionTag = new() { Name = (await context.MediaTags.SingleOrDefaultAsync(t => t.MediaTagId == versionTag.MediaTagId)).Name };
                        ((List<Common.Api.Response.MediaTag>)newResponseVersion.Tags).Add(newResponseVersionTag);
                    }

                    // Links
                    newResponseVersion.Links = new List<Common.Api.Response.MediaLink>();
                    var links = await context.MediaLinks.Where(l => l.MediaVersionId == version.MediaVersionId).ToListAsync();
                    foreach (var link in links)
                    {
                        Common.Api.Response.MediaLink newResponseLink = new();
                        link.MapResponseMediaLink(ref newResponseLink);
                        ((List<Common.Api.Response.MediaLink>)newResponseVersion.Links).Add(newResponseLink);
                    }
                }
            }
            return responseHeader;
        }

        private async Task<ActionResult<IEnumerable<Common.Api.Response.MediaHeader>>> GetLast10()
        {
            try
            {
                // This probably needs optimization! :)
                var lastHeaderIds = context.MediaHeaders.Join(context.MediaParts,
                    h => h.MediaHeaderId,
                    p => p.MediaHeaderId,
                    (h, p) => new { h.MediaHeaderId, p.MediaPartId })
                    .Join(context.MediaVersions,
                        hp => hp.MediaPartId,
                        v => v.MediaPartId,
                        (hp, v) => new { hp.MediaHeaderId, VersionCreated = v.Created })
                    .OrderByDescending(x => x.VersionCreated)
                    .AsEnumerable()
                    .GroupBy(id => id.MediaHeaderId)
                    .Select(h => h.Key)
                    .Take(10)
                    .ToList();
                var lastHeaders = new List<Common.Api.Response.MediaHeader>();
                foreach (var id in lastHeaderIds)
                    lastHeaders.Add(await BuildResponseHeaderAsync(id));

                if (lastHeaders == null)
                    lastHeaders = new List<Common.Api.Response.MediaHeader>();

                return Ok(lastHeaders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}