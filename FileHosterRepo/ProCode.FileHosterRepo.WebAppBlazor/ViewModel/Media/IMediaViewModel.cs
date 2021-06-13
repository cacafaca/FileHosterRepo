using System.Threading.Tasks;
using Request = ProCode.FileHosterRepo.Common.Api.Request;
using Response = ProCode.FileHosterRepo.Common.Api.Response;

namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel.Media
{
    public interface IMediaViewModel: ICacheStream
    {
        public Task<Response.MediaHeader> AddAsync(Request.MediaHeader mediaHeader);
    }
}
