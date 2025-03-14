using CK.Core;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.DB.Auth.AuthScope;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserGitLab.AuthScope.Tests;

[TestFixture]
public class UserGitLabAuthScopeTests
{

    [Test]
    public async Task non_user_google_ScopeSet_is_null_Async()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        Throw.DebugAssert( user != null && p != null );
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            var id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
            (await p.ReadScopeSetAsync( ctx, id )).ShouldBeNull();
        }
    }

    [Test]
    public async Task setting_default_scopes_impact_new_users_Async()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var p = SharedEngine.Map.StObjs.Obtain<Package>();
        var factory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGitLabInfo>>();
        Throw.DebugAssert( user != null && p != null && factory != null );
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            AuthScopeSet original = await p.ReadDefaultScopeSetAsync( ctx );
            original.Contains( "nimp" ).ShouldBeFalse();
            original.Contains( "thing" ).ShouldBeFalse();
            original.Contains( "other" ).ShouldBeFalse();

            {
                int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                IUserGitLabInfo userInfo = factory.Create();
                userInfo.GitLabAccountId = Guid.NewGuid().ToString();
                await p.UserGitLabTable.CreateOrUpdateGitLabUserAsync( ctx, 1, id, userInfo );
                var info = await p.UserGitLabTable.FindKnownUserInfoAsync( ctx, userInfo.GitLabAccountId );
                Throw.DebugAssert( info != null );
                AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, info.UserId );
                userSet.ToString().ShouldBe( original.ToString() );
            }
            AuthScopeSet replaced = original.Clone();
            replaced.Add( new AuthScopeItem( "nimp" ) );
            replaced.Add( new AuthScopeItem( "thing", ScopeWARStatus.Rejected ) );
            replaced.Add( new AuthScopeItem( "other", ScopeWARStatus.Accepted ) );
            await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, replaced );
            var readback = await p.ReadDefaultScopeSetAsync( ctx );
            readback.ToString().ShouldBe( replaced.ToString() );
            // Default scopes have non W status!
            // This must not impact new users: their satus must always be be W.
            readback.ToString().ShouldContain( "[R]thing" );
            readback.ToString().ShouldContain( "[A]other" );

            {
                int id = await user.CreateUserAsync( ctx, 1, Guid.NewGuid().ToString() );
                IUserGitLabInfo? userInfo = p.UserGitLabTable.CreateUserInfo<IUserGitLabInfo>();
                userInfo.GitLabAccountId = Guid.NewGuid().ToString();
                await p.UserGitLabTable.CreateOrUpdateGitLabUserAsync( ctx, 1, id, userInfo, UCLMode.CreateOnly | UCLMode.UpdateOnly );
                userInfo = (IUserGitLabInfo?)(await p.UserGitLabTable.FindKnownUserInfoAsync( ctx, userInfo.GitLabAccountId ))?.Info;
                Throw.DebugAssert( userInfo != null );
                AuthScopeSet userSet = await p.ReadScopeSetAsync( ctx, id );
                userSet.ToString().ShouldContain( "[W]thing" );
                userSet.ToString().ShouldContain( "[W]other" );
                userSet.ToString().ShouldContain( "[W]nimp" );
            }
            await p.AuthScopeSetTable.SetScopesAsync( ctx, 1, original );
        }
    }

}

