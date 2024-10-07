using CK.Core;
using System;

namespace CK.DB.User.UserGitLab.AuthScope;

/// <summary>
/// Extends <see cref="UserGitLab.IUserGitLabInfo"/> with ScopeSet identifier.
/// </summary>
public interface IUserGitLabInfo : UserGitLab.IUserGitLabInfo
{
    /// <summary>
    /// Gets or sets the scope set identifier.
    /// Note that the ScopeSetId is intrinsic: a new ScopeSetId is acquired 
    /// and set only when a new UserGitLab is created (by copy from 
    /// the default one - the ScopeSet of the UserGitLab 0).
    /// </summary>
    int ScopeSetId { get; set; }
}
