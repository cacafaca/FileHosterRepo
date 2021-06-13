namespace ProCode.FileHosterRepo.Common.Api.Request
{
    public class UserRegister : User
    {
        public string Nickname { get; set; }
        public override string ToString()
        {
            return $"Email={Email};Nickname={Nickname}";
        }
    }
}
