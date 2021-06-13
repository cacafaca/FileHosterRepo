using System.IO;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public interface ICacheStream
    {
        public void WriteCacheMediaHeader(Common.Api.Response.MediaHeader mediaHeader);
        public Common.Api.Response.MediaHeader ReadCacheMediaHeader();
    }
}