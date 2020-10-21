using CK.Core;

namespace CK.DB.User.UserGitLab
{
    /// <summary>
    /// Holds information stored for a GitLab user.
    /// </summary>
    public interface IUserGitLabInfo : IPoco
    {
        /// <summary>
        /// Gets or sets the GitLab account identifier.
        /// </summary>
        string GitLabAccountId { get; set; }
    }

}
