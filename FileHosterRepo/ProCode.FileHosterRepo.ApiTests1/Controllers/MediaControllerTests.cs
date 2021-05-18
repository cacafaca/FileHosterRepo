using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProCode.FileHosterRepo.ApiTests;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

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
            Config.Client.PostAsync("/Admin/Register",
                new StringContent(
                    @"{ 
                        ""Email"": ""admin@admin.com"",
                        ""Password"": ""admin""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson)).Wait();

            // Register user.
            HttpResponseMessage response = Config.Client.PostAsync("/Users/Register",
                new StringContent(
                    @"{ 
                        ""Email"": ""user@user.com"",
                        ""Password"": ""user"",
                        ""Nickname"": ""User""
                    }",
                Encoding.UTF8, Config.HttpMediaTypeJson)).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            this.token = response.Content.ReadAsStringAsync().Result;
        }
        #endregion

        [TestMethod()]
        public async Task Add_Single_Media()
        {
            // Always set token first.
            Config.Client.SetToken(this.token);

            Model.Request.Media newMedia = new Model.Request.Media
            {
                Name = "Breaking Bad (2008)",
                Description = "A high school chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine in order to secure his family's future.",
                ReferenceLink = "https://www.imdb.com/title/tt0903747/",
                Parts = new List<Model.Request.MediaPart>
                {
                    new Model.Request.MediaPart
                    {
                        Season = 1,
                        Episode = 1,
                        Name = "Pilot",
                        Description = "Diagnosed with terminal lung cancer, chemistry teacher Walter White teams up with former student Jesse Pinkman to cook and sell crystal meth.",
                        ReferenceLink = "https://www.imdb.com/title/tt0959621/?ref_=ttep_ep1",
                        Links = new List<Model.Request.MediaLink>
                        {
                            new Model.Request.MediaLink
                            {
                                LinkId = 1,
                                Link = "https://www.imdb.com/title/tt0959621/?ref_=ttep_ep1"
                            },
                            new Model.Request.MediaLink
                            {
                                LinkId = 2,
                                Link = "https://m.media-amazon.com/images/M/MV5BNTZlMGY1OWItZWJiMy00MTZlLThhMGItNDQ2ODM3YzNkOWU5XkEyXkFqcGdeQXVyNzgyOTQ4MDc@._V1_UY268_CR147,0,182,268_AL_.jpg"
                            }
                        }
                    },
                    new Model.Request.MediaPart
                    {
                        Season = 1,
                        Episode = 2,
                        Name = "Cat's in the Bag...",
                        Description = "After their first drug deal goes terribly wrong, Walt and Jesse are forced to deal with a corpse and a prisoner. Meanwhile, Skyler grows suspicious of Walt's activities.",
                        ReferenceLink = "https://www.imdb.com/title/tt1054724/?ref_=ttep_ep2",
                        Links = new List<Model.Request.MediaLink>
                        {
                            new Model.Request.MediaLink
                            {
                                LinkId = 1,
                                Link = "https://www.imdb.com/title/tt1054724/?ref_=ttep_ep2"
                            },
                            new Model.Request.MediaLink
                            {
                                LinkId = 2,
                                Link = "https://m.media-amazon.com/images/M/MV5BNmI5MTU3OTAtYTczMC00MDE5LTg3YjMtMjA3NWEyMmYyZWQwXkEyXkFqcGdeQXVyNjk1MzkzMzM@._V1_UY268_CR87,0,182,268_AL_.jpg"
                            }
                        }
                    }
                }
            };
            HttpResponseMessage response = await Config.Client.PostAsync("/Media/Add",
                new StringContent(
                    JsonSerializer.Serialize(newMedia),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var message = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, message);
        }

        [TestMethod()]
        public async Task Add_Twice()
        {
            // Always set token first.
            Config.Client.SetToken(this.token);

            // First step. Add part 1.
            Model.Request.Media newMedia = new Model.Request.Media
            {
                Name = "Breaking Bad (2008)",
                Description = "A high school chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine in order to secure his family's future.",
                ReferenceLink = "https://www.imdb.com/title/tt0903747/",
                Parts = new List<Model.Request.MediaPart>
                {
                    new Model.Request.MediaPart
                    {
                        Season = 1,
                        Episode = 1,
                        Name = "Pilot",
                        Description = "Diagnosed with terminal lung cancer, chemistry teacher Walter White teams up with former student Jesse Pinkman to cook and sell crystal meth.",
                        ReferenceLink = "https://www.imdb.com/title/tt0959621/?ref_=ttep_ep1",
                        Links = new List<Model.Request.MediaLink>
                        {
                            new Model.Request.MediaLink
                            {
                                LinkId = 1,
                                Link = "https://www.imdb.com/title/tt0959621/?ref_=ttep_ep1"
                            },
                            new Model.Request.MediaLink
                            {
                                LinkId = 2,
                                Link = "https://m.media-amazon.com/images/M/MV5BNTZlMGY1OWItZWJiMy00MTZlLThhMGItNDQ2ODM3YzNkOWU5XkEyXkFqcGdeQXVyNzgyOTQ4MDc@._V1_UY268_CR147,0,182,268_AL_.jpg"
                            }
                        }
                    }
                }
            };
            HttpResponseMessage response = await Config.Client.PostAsync("/Media/Add",
                new StringContent(
                    JsonSerializer.Serialize(newMedia),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var message = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, message);

            // Second step. Add part 2.
            newMedia = new Model.Request.Media
            {
                Name = "Breaking Bad (2008)",
                Description = "A high school chemistry teacher diagnosed with inoperable lung cancer turns to manufacturing and selling methamphetamine in order to secure his family's future.",
                ReferenceLink = "https://www.imdb.com/title/tt0903747/",
                Parts = new List<Model.Request.MediaPart>
                {
                    new Model.Request.MediaPart
                    {
                        Season = 1,
                        Episode = 2,
                        Name = "Cat's in the Bag...",
                        Description = "After their first drug deal goes terribly wrong, Walt and Jesse are forced to deal with a corpse and a prisoner. Meanwhile, Skyler grows suspicious of Walt's activities.",
                        ReferenceLink = "https://www.imdb.com/title/tt1054724/?ref_=ttep_ep2",
                        Links = new List<Model.Request.MediaLink>
                        {
                            new Model.Request.MediaLink
                            {
                                LinkId = 1,
                                Link = "https://www.imdb.com/title/tt1054724/?ref_=ttep_ep2"
                            },
                            new Model.Request.MediaLink
                            {
                                LinkId = 2,
                                Link = "https://m.media-amazon.com/images/M/MV5BNmI5MTU3OTAtYTczMC00MDE5LTg3YjMtMjA3NWEyMmYyZWQwXkEyXkFqcGdeQXVyNjk1MzkzMzM@._V1_UY268_CR87,0,182,268_AL_.jpg"
                            }
                        }
                    }
                }
            };
            response = await Config.Client.PostAsync("/Media/Add",
                new StringContent(
                    JsonSerializer.Serialize(newMedia),
                    Encoding.UTF8, Config.HttpMediaTypeJson));
            var message = response.Content.ReadAsStringAsync().Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, message);
        }
    }
}