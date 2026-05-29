using Microsoft.AspNetCore.Hosting;

namespace CodingCell.YARPad.Hosting;

public static class IWebHostEnvironmentExtensions
{
    extension(IWebHostEnvironment env)
    {
        public string GetDataRootFullPath(YARPadProxyOptions options)
        {
            var dataPath = options.RootDataPath ?? ".";
            if (dataPath == ".")
                dataPath = env.ContentRootPath;

            return dataPath;
        }

        public string GetAcmeChallengeRootPath(YARPadProxyOptions options) => Path.Combine(env.GetDataRootFullPath(options), "acme");

        public string GetAcmeChallengePath(YARPadProxyOptions options) => Path.Combine(env.GetAcmeChallengeRootPath(options), ".well-known", "acme-challenge");

        public string GetLetsEncryptRootPath(YARPadProxyOptions options) => Path.Combine(env.GetDataRootFullPath(options), options.LetsEncrypt.DataPath);
    }
}