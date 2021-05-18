using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProCode.FileHosterRepo.Api.Model;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<bool>> Add(Model.Request.Media newMedia)
        {
            // Always check at beginning!
            var loggedUser = await GetLoggedUserAsync();
            if (loggedUser != null)
            {
                try
                {
                    // Media
                    var newDbMedia = await context.Medias.AddAsync(new Dal.Model.Media
                    {
                        Name = newMedia.Name,
                        Description = newMedia.Description,
                        ReferenceLink = new Uri(newMedia.ReferenceLink),
                        UserId = User.GetUserId()
                    });
                    await context.SaveChangesAsync();   // Save to get MediaId.

                    // Media part
                    await context.MediaParts.AddRangeAsync(newMedia.Parts.Select(p =>
                      new Dal.Model.MediaPart
                      {
                          MediaId = newDbMedia.Entity.MediaId,
                          Season = p.Season,
                          Episode = p.Episode,
                          Name = p.Name,
                          Description = p.Description,
                          ReferenceLink = new Uri(p.ReferenceLink),
                          UserId = User.GetUserId()
                      }));
                    await context.SaveChangesAsync();

                    // Links
                    var newDbMediaParts = context.MediaParts.Where(p => p.MediaId == newDbMedia.Entity.MediaId);
                    var allLinksOfCreatedParts = context.MediaLinks
                        .Where(l => newDbMediaParts.Any(p => p.MediaPartId == l.MediaPartId));
                    int maxVersion = allLinksOfCreatedParts.Count() > 0 ? allLinksOfCreatedParts.Max(mp => mp.VersionId) : 1;
                    foreach (var newDbMediaPart in newDbMediaParts)
                        await context.MediaLinks.AddRangeAsync(newMedia.Parts.Where(p => p.Season == newDbMediaPart.Season && p.Episode == newDbMediaPart.Episode).FirstOrDefault().Links.Select(l =>
                            new Dal.Model.MediaLink
                            {
                                MediaPartId = newDbMediaPart.MediaPartId,
                                VersionId = maxVersion,
                                LinkId = l.LinkId,
                                Link = new Uri(l.Link),
                                Created = DateTime.Now,
                                UserId = User.GetUserId()
                            }));
                    await context.SaveChangesAsync();

                    return Ok(true);
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
        #endregion
    }
}