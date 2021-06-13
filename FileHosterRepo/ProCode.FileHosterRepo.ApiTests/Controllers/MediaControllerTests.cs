using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.ApiTests;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace ProCode.FileHosterRepo.Api.Controllers.Tests
{
    [TestClass]
    public class MediaControllerTests
    {
        #region Fields
        private readonly string token;
        #endregion

        #region Constructors
        public MediaControllerTests()
        {
            Config.RecreateDatabaseAsync().Wait();  // Need empty database at beginning of each test method.

            // Register administrator. In order to register user, an administrator needs to be registered.
            HttpResponseMessage response = Config.Client.PostAsync(Common.Routes.Admin.Register,
                new StringContent(JsonSerializer.Serialize(new Common.Api.Request.UserRegister
                {
                    Email = "admin@admin.com",
                    Password = "admin"
                }), Encoding.UTF8, Config.HttpMediaTypeJson)).Result;
            this.token = response.Content.ReadAsStringAsync().Result;
            Config.Client.SetToken(this.token);
            Config.Client.GetAsync(Common.Routes.Admin.Logout).Wait();
            Config.Client.SetToken();   // Clear token.

            // Register user.
            response = Config.Client.PostAsync(Common.Routes.User.Register,
                new StringContent(JsonSerializer.Serialize(new Common.Api.Request.UserRegister
                {
                    Email = "user@user.com",
                    Password = "user",
                    Nickname = "User"
                }), Encoding.UTF8, Config.HttpMediaTypeJson)).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            this.token = response.Content.ReadAsStringAsync().Result;
            Config.Client.SetToken(this.token);                             // All test here assumes that user is logged.
        }
        #endregion

        #region Tests
        [TestMethod()]
        public async Task Add_Single_Media_And_Check_All_Content()
        {
            // For the sake of simplicity I'll use request object and convert it to JSON before calling API.
            var requestMedia = ExampleRequest_BreakingBad();

            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(JsonSerializer.Serialize(requestMedia),Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseMedia = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseMedia);

            CompareRequestAndResponse(requestMedia, responseMedia);
        }

        [TestMethod()]
        public async Task Add_Two_Medias()
        {
            // Add "Breaking Bad" Series
            var requestMedia = ExampleRequest_BreakingBad();
            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(requestMedia),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseMedia = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseMedia);
            CompareRequestAndResponse(requestMedia, responseMedia);

            // Add "American Beauty" movie.
            requestMedia = ExampleRequest_AmericanBeauty();
            response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(requestMedia),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            responseMedia = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseMedia);
            CompareRequestAndResponse(requestMedia, responseMedia);
        }

        [TestMethod()]
        public async Task Add_Same_Media_Twice()
        {
            // Add "Breaking Bad" Series
            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(ExampleRequest_BreakingBad()),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseMedia = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseMedia);

            // Add "American Beauty" movie.
            response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(ExampleRequest_BreakingBad()),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            responseMedia = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseMedia);
        }

        [TestMethod()]
        public async Task Get_Single()
        {
            // Add something to update in second step.
            var requestHeader = ExampleRequest_BreakingBad();
            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(requestHeader),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            // Get by id.
            response = await Config.Client.GetAsync(Common.Routes.Media.GetId(responseHeader.MediaHeaderId));
            responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            CompareRequestAndResponse(requestHeader, responseHeader);
        }

        [TestMethod()]
        public async Task Update_Single_Media()
        {
            // For the sake of sanity I'll use request object and convert it to JSON before calling API.

            // Always set token first.
            Config.Client.SetToken(this.token);

            // Make some changes so we can correct it later with update command.
            var wrongRequest = ExampleRequest_BreakingBad();
            wrongRequest.Name += " ~some wrong text~";
            wrongRequest.Parts.ToArray()[0].Name += " ~some wrong text~";

            // Add wrong data.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(wrongRequest),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            // Change request data, with correct values.
            Common.Api.Request.MediaHeader correctedRequestHeader = ResponseToRequest(responseHeader);
            correctedRequestHeader.Name = "Breaking Bad (2008)";
            correctedRequestHeader.Parts.ToArray()[0].Name = "Pilot";

            // Update. Structures must have id's.
            response = await Config.Client.PostAsync(Common.Routes.Media.Update,
                new StringContent(
                    JsonSerializer.Serialize(correctedRequestHeader),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);


            // Header
            Assert.AreEqual("Breaking Bad (2008)", responseHeader.Name);
            Assert.IsNotNull(responseHeader.Description);
            Assert.IsTrue(responseHeader.Description.Length > 0);

            // Parts
            Assert.AreEqual(2, responseHeader.Parts.Count);
            Assert.AreEqual("Pilot", responseHeader.Parts.ToArray()[0].Name);
            Assert.AreEqual("Cat's in the Bag...", responseHeader.Parts.ToArray()[1].Name);

            // Part[0] / Versions
            Assert.AreEqual(1, responseHeader.Parts.ToArray()[0].Versions.Count);
            Assert.AreEqual(2, responseHeader.Parts.ToArray()[0].Versions.ToArray()[0].Links.Count);
            // Part[1] / Versions
            Assert.AreEqual(1, responseHeader.Parts.ToArray()[1].Versions.Count);
            Assert.AreEqual(2, responseHeader.Parts.ToArray()[1].Versions.ToArray()[0].Links.Count);
        }

        [TestMethod()]
        public async Task Remove_One_Version_Tag()
        {
            // For the sake of sanity I'll use request object and convert it to JSON before calling API.

            // Add wrong data.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(ExampleRequest_BreakingBad()),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            // Delete one tag from first part.
            Common.Api.Request.MediaHeader updatedRequest = ResponseToRequest(responseHeader);
            var ver1Tags = (List<Common.Api.Request.MediaTag>)((List<Common.Api.Request.MediaPart>)updatedRequest.Parts).First().Version.Tags;
            var oldVersionTagCount = ver1Tags.Count;
            ver1Tags.Remove(ver1Tags.LastOrDefault());

            // Update. Structures must have id's.
            response = await Config.Client.PostAsync(Common.Routes.Media.Update,
                new StringContent(
                    JsonSerializer.Serialize(updatedRequest),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            Assert.AreEqual(--oldVersionTagCount, responseHeader.Parts.First().Versions.First().Tags.Count);
        }

        [TestMethod]
        public async Task Remove_One_Header_Tag()
        {
            // For the sake of sanity I'll use request object and convert it to JSON before calling API.

            // Add wrong data.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(ExampleRequest_BreakingBad()),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            // Delete one tag from header.
            Common.Api.Request.MediaHeader updatedRequest = ResponseToRequest(responseHeader);
            var oldHeaderTagsCount = updatedRequest.Tags.Count;
            ((IList<Common.Api.Request.MediaTag>)updatedRequest.Tags).RemoveAt(updatedRequest.Tags.Count - 1);

            // Update. Structures must have id's.
            response = await Config.Client.PostAsync(Common.Routes.Media.Update,
                 new StringContent(
                     JsonSerializer.Serialize(updatedRequest),
                     Encoding.UTF8, Config.HttpMediaTypeJson));
            responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            Assert.AreEqual(--oldHeaderTagsCount, responseHeader.Tags.Count);
        }

        [TestMethod]
        public async Task Remove_One_Part_Tag()
        {
            // For the sake of sanity I'll use request object and convert it to JSON before calling API.

            // Add wrong data.
            HttpResponseMessage response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                new StringContent(
                    JsonSerializer.Serialize(ExampleRequest_BreakingBad()),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            Common.Api.Response.MediaHeader responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            // Delete one tag from first part.
            Common.Api.Request.MediaHeader updatedRequest = ResponseToRequest(responseHeader);
            var part1Tags = ((List<Common.Api.Request.MediaTag>)((List<Common.Api.Request.MediaPart>)updatedRequest.Parts).First().Tags);
            var oldPart1TagCount = part1Tags.Count;
            part1Tags.Remove(part1Tags.LastOrDefault());

            // Update. Structures must have id's.
            response = await Config.Client.PostAsync(Common.Routes.Media.Update,
                new StringContent(
                    JsonSerializer.Serialize(updatedRequest),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            responseHeader = JsonSerializer.Deserialize<Common.Api.Response.MediaHeader>(responseMessage);
            Assert.IsNotNull(responseHeader);

            Assert.AreEqual(--oldPart1TagCount, ((IList<Common.Api.Response.MediaTag>)((IList<Common.Api.Response.MediaPart>)responseHeader.Parts).First().Tags).Count);
        }
        [TestMethod()]

        public async Task Last10()
        {
            HttpResponseMessage response;

            // Add 20 records.
            for (int i = 0; i < 20; i++)
            {
                var requestHeader = i % 2 == 0 ? ExampleRequest_BreakingBad() : ExampleRequest_AmericanBeauty();
                response = await Config.Client.PostAsync(Common.Routes.Media.Add,
                    new StringContent(
                        JsonSerializer.Serialize(requestHeader),
                        Encoding.UTF8, Config.HttpMediaTypeJson));
                Thread.Sleep(500);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }

            // Get last 10              
            response = await Config.Client.GetAsync(Common.Routes.Media.Last10);
            var responseMessage = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, responseMessage);
            var responseHeaderList = JsonSerializer.Deserialize<IEnumerable<Common.Api.Response.MediaHeader>>(responseMessage);
            Assert.IsNotNull(responseHeaderList);
            Assert.AreEqual(10, responseHeaderList.GroupBy(h=>h.MediaHeaderId).Count());    // Expect 10 unique header ids.
        }
        #endregion

        #region Methods
        private static Common.Api.Request.MediaHeader ExampleRequest_BreakingBad()
        {
            return new Common.Api.Request.MediaHeader
            {
                Name = "Breaking Bad",
                Year = 2008,
                Description = "A high school chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine in order to secure his family's future.",
                ReferenceLink = "https://www.imdb.com/title/tt0903747/",
                Tags = new List<Common.Api.Request.MediaTag>
                {
                    new Common.Api.Request.MediaTag { Name = "TV Series" },
                    new Common.Api.Request.MediaTag { Name = "Crime" },
                    new Common.Api.Request.MediaTag { Name = "Drama" },
                    new Common.Api.Request.MediaTag { Name = "Thriller" }
                },
                Parts = new List<Common.Api.Request.MediaPart>
                {
                    new Common.Api.Request.MediaPart
                    {
                        Season = 1,
                        Episode = 1,
                        Name = "Pilot",
                        Description = "Diagnosed with terminal lung cancer, chemistry teacher Walter White teams up with former student Jesse Pinkman to cook and sell crystal meth.",
                        ReferenceLink = "https://www.imdb.com/title/tt0959621/?ref_=ttep_ep1",
                        Version = new Common.Api.Request.MediaVersion
                        {
                            VersionComment = "Has English title in package.",
                            Links = new List<Common.Api.Request.MediaLink>
                            {
                                new Common.Api.Request.MediaLink
                                {
                                    LinkOrderId = 1,
                                    Link = "https://www.imdb.com/title/tt0959621/?ref_=ttep_ep1"
                                },
                                new Common.Api.Request.MediaLink
                                {
                                    LinkOrderId = 2,
                                    Link = "https://m.media-amazon.com/images/M/MV5BNTZlMGY1OWItZWJiMy00MTZlLThhMGItNDQ2ODM3YzNkOWU5XkEyXkFqcGdeQXVyNzgyOTQ4MDc@._V1_UY268_CR147,0,182,268_AL_.jpg"
                                }
                            },
                            Tags = new List<Common.Api.Request.MediaTag>
                            {
                                new Common.Api.Request.MediaTag { Name = "DVDRip" },
                                new Common.Api.Request.MediaTag { Name = "YIFY" },
                                new Common.Api.Request.MediaTag { Name = "x264" }
                            }
                        },
                        Tags = new List<Common.Api.Request.MediaTag>
                        {
                            new Common.Api.Request.MediaTag { Name = "Recommend" },
                        }
                    },
                    new Common.Api.Request.MediaPart
                    {
                        Season = 1,
                        Episode = 2,
                        Name = "Cat's in the Bag...",
                        Description = "After their first drug deal goes terribly wrong, Walt and Jesse are forced to deal with a corpse and a prisoner. Meanwhile, Skyler grows suspicious of Walt's activities.",
                        ReferenceLink = "https://www.imdb.com/title/tt1054724/?ref_=ttep_ep2",
                        Version = new Common.Api.Request.MediaVersion
                        {
                            VersionComment = "High audio quality 5.1.",
                            Links = new List<Common.Api.Request.MediaLink>
                            {
                                new Common.Api.Request.MediaLink
                                {
                                    LinkOrderId = 1,
                                    Link = "https://www.imdb.com/title/tt1054724/?ref_=ttep_ep2"
                                },
                                new Common.Api.Request.MediaLink
                                {
                                    LinkOrderId = 2,
                                    Link = "https://m.media-amazon.com/images/M/MV5BNmI5MTU3OTAtYTczMC00MDE5LTg3YjMtMjA3NWEyMmYyZWQwXkEyXkFqcGdeQXVyNjk1MzkzMzM@._V1_UY268_CR87,0,182,268_AL_.jpg"
                                }
                            },
                            Tags = new List<Common.Api.Request.MediaTag>
                            {
                                new Common.Api.Request.MediaTag { Name = "DVDRip" },
                                new Common.Api.Request.MediaTag { Name = "x264" }
                            }
                        }
                    }
                }
            };
        }

        private static Common.Api.Request.MediaHeader ExampleRequest_AmericanBeauty()
        {
            return new Common.Api.Request.MediaHeader
            {
                Name = "American Beauty",
                Year = 1999,
                Description = "A sexually frustrated suburban father has a mid-life crisis after becoming infatuated with his daughter's best friend.",
                ReferenceLink = "https://www.imdb.com/title/tt0169547",
                Tags = new List<Common.Api.Request.MediaTag>
                {
                    new Common.Api.Request.MediaTag { Name = "Drama" }
                },
                Parts = new List<Common.Api.Request.MediaPart>
                {
                    new Common.Api.Request.MediaPart
                    {
                        Season = 0,
                        Episode = 0,
                        Name = null,
                        Description = null,
                        ReferenceLink = null,
                        Version = new Common.Api.Request.MediaVersion
                        {
                            VersionComment = "Re-upload upon requests.",
                            Links = new List<Common.Api.Request.MediaLink>
                            {
                                new Common.Api.Request.MediaLink
                                {
                                    LinkOrderId = 1,
                                    Link = "https://m.media-amazon.com/images/M/MV5BMTY1NzMyODc3Nl5BMl5BanBnXkFtZTgwNzE2MzA1NDM@._V1_UY44_CR11,0,32,44_AL_.jpg"
                                },
                                new Common.Api.Request.MediaLink
                                {
                                    LinkOrderId = 2,
                                    Link = "https://m.media-amazon.com/images/M/MV5BMTc4ODQ1ODM5Ml5BMl5BanBnXkFtZTcwOTU2NDk3OQ@@._V1_UX32_CR0,0,32,44_AL_.jpg"
                                }
                            },
                            Tags = new List<Common.Api.Request.MediaTag>
                            {
                                new Common.Api.Request.MediaTag { Name = "BRRip" },
                                new Common.Api.Request.MediaTag { Name = "YIFY" },
                                new Common.Api.Request.MediaTag { Name = "x264" }
                            }
                        },
                        Tags = new List<Common.Api.Request.MediaTag>
                        {
                            new Common.Api.Request.MediaTag { Name = "Awesome" },
                        }
                    }
                }
            };
        }

        private static Common.Api.Request.MediaHeader ResponseToRequest(Common.Api.Response.MediaHeader responseHeader)
        {
            return new()
            {
                MediaHeaderId = responseHeader.MediaHeaderId,
                Name = responseHeader.Name,
                Description = responseHeader.Description,
                ReferenceLink = responseHeader.ReferenceLink,
                Parts = responseHeader.Parts.Select(p => new Common.Api.Request.MediaPart
                {
                    MediaPartId = p.MediaPartId,
                    Season = p.Season,
                    Episode = p.Episode,
                    Name = p.Name,
                    Description = p.Description,
                    ReferenceLink = p.ReferenceLink,
                    Version = new Common.Api.Request.MediaVersion
                    {
                        MediaVersionId = p.Versions.FirstOrDefault().MediaVersionId,
                        VersionComment = p.Versions.FirstOrDefault().VersionComment,
                        Links = p.Versions.FirstOrDefault().Links.Select(l => new Common.Api.Request.MediaLink
                        {
                            MediaLinkId = l.MediaLinkId,
                            LinkOrderId = l.LinkOrderId,
                            Link = l.Link
                        }).ToList(),
                        Tags = p.Versions.FirstOrDefault().Tags.Select(t => new Common.Api.Request.MediaTag
                        {
                            Name = t.Name
                        }).ToList()
                    },
                    Tags = p.Tags.Select(t => new Common.Api.Request.MediaTag
                    {
                        Name = t.Name
                    }).ToList()
                }).ToList(),
                Tags = responseHeader.Tags.Select(t => new Common.Api.Request.MediaTag
                {
                    Name = t.Name
                }).ToList()
            };
        }

        private static void CompareRequestAndResponse(Common.Api.Request.MediaHeader requestMedia, Common.Api.Response.MediaHeader responseMedia)
        {
            // Header
            Assert.AreEqual(requestMedia.Name, responseMedia.Name);
            Assert.AreEqual(requestMedia.Year, responseMedia.Year);
            Assert.AreEqual(requestMedia.Description, responseMedia.Description);
            Assert.AreEqual(requestMedia.ReferenceLink, responseMedia.ReferenceLink);
            if (requestMedia.Tags != null)
            {
                Assert.AreEqual(requestMedia.Tags.Count, responseMedia.Tags.Count);
                foreach (var requestTag in requestMedia.Tags)
                    Assert.IsTrue((responseMedia.Tags as IList<Common.Api.Response.MediaTag>).Any(responseTag => responseTag.Name == requestTag.Name));
            }

            // Parts
            if (requestMedia.Parts != null)
            {
                Assert.AreEqual(requestMedia.Parts.Count, responseMedia.Parts.Count);
                foreach (var requestPart in requestMedia.Parts)
                {
                    var responsePart = responseMedia.Parts.SingleOrDefault(p => p.Season == requestPart.Season && p.Episode == requestPart.Episode);
                    Assert.IsNotNull(responsePart, "Can't find response part.");

                    if (requestPart.Name != null)
                        Assert.AreEqual(requestPart.Name, responsePart.Name);
                    if (requestPart.Description != null)
                        Assert.AreEqual(requestPart.Description, responsePart.Description);
                    if (requestPart.ReferenceLink != null)
                        Assert.AreEqual(requestPart.ReferenceLink, responsePart.ReferenceLink);

                    // Part Tags
                    if (requestPart.Tags != null)
                    {
                        Assert.AreEqual(requestPart.Tags.Count, responsePart.Tags.Count);
                        foreach (var requestTag in requestPart.Tags)
                            Assert.IsTrue((responsePart.Tags as IList<Common.Api.Response.MediaTag>).Any(responseTag => responseTag.Name == requestTag.Name));
                    }

                    // Version
                    Assert.AreEqual(requestPart.Version.VersionComment, responsePart.Versions.FirstOrDefault().VersionComment);

                    // Version Tags
                    if (requestPart.Version.Tags != null)
                    {
                        Assert.AreEqual(requestPart.Version.Tags.Count, responsePart.Versions.FirstOrDefault().Tags.Count);
                        foreach (var requestTag in requestPart.Version.Tags)
                            Assert.IsTrue((responsePart.Versions.FirstOrDefault().Tags as IList<Common.Api.Response.MediaTag>).Any(responseTag => responseTag.Name == requestTag.Name));
                    }

                    // Links
                    Assert.AreEqual(requestPart.Version.Links.Count, responsePart.Versions.FirstOrDefault().Links.Count);
                    foreach (var requestLink in requestPart.Version.Links)
                    {
                        var responseLink = responsePart.Versions.FirstOrDefault().Links.SingleOrDefault(l => l.LinkOrderId == requestLink.LinkOrderId);
                        Assert.IsNotNull(responseLink);
                        Assert.AreEqual(requestLink.Link, responseLink.Link);
                    }
                }
            }
        }
        #endregion
    }
}