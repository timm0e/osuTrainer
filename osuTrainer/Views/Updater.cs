using System;
using System.Threading.Tasks;
using Octokit;

namespace osuTrainer
{
    public static class Updater
    {
        public static async Task<Version> Check()
        {
            var github = new GitHubClient(new ProductHeaderValue("osuTrainer"));
            var releases = await github.Release.GetAll("condone", "osuTrainer");
            return new Version(releases[0].Name);
        }
    }
}