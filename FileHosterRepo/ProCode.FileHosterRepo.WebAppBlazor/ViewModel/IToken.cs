namespace ProCode.FileHosterRepo.WebAppBlazor.ViewModel
{
    public interface IToken
    {
        public void SetToken(string token = null);
        public void ClearToken();
        public string GetToken();
    }
}