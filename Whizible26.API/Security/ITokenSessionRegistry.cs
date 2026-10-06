namespace Whizible26.API.Security
{
    public interface ITokenSessionRegistry
    {
        void RegisterToken(string loginId, string clientBinding, string jti, TimeSpan lifetime);

        bool IsTokenActive(string loginId, string clientBinding, string jti);

        void RevokeLogin(string loginId);
    }
}
