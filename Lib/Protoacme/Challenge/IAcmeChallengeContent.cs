using Protoacme.Models;

namespace Protoacme.Challenge
{
    public interface IAcmeChallengeContent
    {
        AcmeAccount Account { get; set; }
        AcmeChallenge Challenge { get; set; }
        string AuthorizationKey { get; set; }
        string Token { get; set; }
        string Identifier { get; set; }

        void SaveToFile(string filePath);
    }
}