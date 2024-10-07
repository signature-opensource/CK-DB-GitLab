using CK.Core;
using CK.DB.Actor;
using CK.DB.Auth;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserGitLab.Tests;

[TestFixture]
public class UserGitLabTests
{
    [Test]
    public void create_GitLab_user_and_check_read_info_object_method()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var p = SharedEngine.Map.StObjs.Obtain<UserGitLabTable>();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGitLabInfo>>();
        Throw.DebugAssert( user != null && p != null && infoFactory != null );
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            var userName = Guid.NewGuid().ToString();
            int userId = user.CreateUser( ctx, 1, userName );
            var googleAccountId = Guid.NewGuid().ToString( "N" );

            var info = infoFactory.Create();
            info.GitLabAccountId = googleAccountId;
            var created = p.CreateOrUpdateGitLabUser( ctx, 1, userId, info );
            created.OperationResult.Should().Be( UCResult.Created );
            var info2 = p.FindKnownUserInfo( ctx, googleAccountId );
            Throw.DebugAssert( info2 != null );
            info2.UserId.Should().Be( userId );
            info2.Info.GitLabAccountId.Should().Be( googleAccountId );

            p.FindKnownUserInfo( ctx, Guid.NewGuid().ToString() ).Should().BeNull();
            user.DestroyUser( ctx, 1, userId );
            p.FindKnownUserInfo( ctx, googleAccountId ).Should().BeNull();
        }
    }

    [Test]
    public async Task create_GitLab_user_and_check_read_info_object_method_Async()
    {
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        var p = SharedEngine.Map.StObjs.Obtain<UserGitLabTable>();
        var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGitLabInfo>>();
        Throw.DebugAssert( user != null && p != null && infoFactory != null );
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            var userName = Guid.NewGuid().ToString();
            int userId = await user.CreateUserAsync( ctx, 1, userName );
            var googleAccountId = Guid.NewGuid().ToString( "N" );

            var info = infoFactory.Create();
            info.GitLabAccountId = googleAccountId;
            var created = await p.CreateOrUpdateGitLabUserAsync( ctx, 1, userId, info );
            created.OperationResult.Should().Be( UCResult.Created );
            var info2 = await p.FindKnownUserInfoAsync( ctx, googleAccountId );
            Throw.DebugAssert( info2 != null );
            info2.UserId.Should().Be( userId );
            info2.Info.GitLabAccountId.Should().Be( googleAccountId );

            (await p.FindKnownUserInfoAsync( ctx, Guid.NewGuid().ToString() )).Should().BeNull();
            await user.DestroyUserAsync( ctx, 1, userId );
            (await p.FindKnownUserInfoAsync( ctx, googleAccountId )).Should().BeNull();
        }
    }

    [Test]
    public void GitLab_AuthProvider_is_registered()
    {
        Auth.Tests.AuthTests.CheckProviderRegistration( "GitLab" );
    }

    [Test]
    public void vUserAuthProvider_reflects_the_user_GitLab_authentication()
    {
        var u = SharedEngine.Map.StObjs.Obtain<UserGitLabTable>();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        Throw.DebugAssert( u != null && user != null );
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            string userName = "GitLab auth - " + Guid.NewGuid().ToString();
            var googleAccountId = Guid.NewGuid().ToString( "N" );
            var idU = user.CreateUser( ctx, 1, userName );
            u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='GitLab'" )!
                .Rows.Should().BeEmpty();
            var info = u.CreateUserInfo<IUserGitLabInfo>();
            info.GitLabAccountId = googleAccountId;
            u.CreateOrUpdateGitLabUser( ctx, 1, idU, info );
            u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='GitLab'" )
                .Should().Be( 1 );
            u.DestroyGitLabUser( ctx, 1, idU );
            u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='GitLab'" )!
                .Rows.Should().BeEmpty();
        }
    }

    [Test]
    public void standard_generic_tests_for_GitLab_provider()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
        // With IUserGitLabInfo POCO.
        var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGitLabInfo>>();
        Throw.DebugAssert( auth != null && f != null );
        CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
            auth,
            "GitLab",
            payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.GitLabAccountId = "GitLabAccountIdFor:" + userName ),
            payloadForLogin: ( userId, userName ) => f.Create( i => i.GitLabAccountId = "GitLabAccountIdFor:" + userName ),
            payloadForLoginFail: ( userId, userName ) => f.Create( i => i.GitLabAccountId = "NO!" + userName )
            );
        // With a KeyValuePair.
        CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
            auth,
            "GitLab",
            payloadForCreateOrUpdate: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "GitLabAccountId", "IdFor:" + userName)
            },
            payloadForLogin: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "GitLabAccountId", "IdFor:" + userName)
            },
            payloadForLoginFail: ( userId, userName ) => new[]
            {
                new KeyValuePair<string,object>( "GitLabAccountId", ("IdFor:" + userName).ToUpperInvariant())
            }
            );
    }

    [Test]
    public async Task standard_generic_tests_for_GitLab_provider_Async()
    {
        var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
        var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserGitLabInfo>>();
        Throw.DebugAssert( auth != null && f != null );
        await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
            auth,
            "GitLab",
            payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.GitLabAccountId = "GitLabAccountIdFor:" + userName ),
            payloadForLogin: ( userId, userName ) => f.Create( i => i.GitLabAccountId = "GitLabAccountIdFor:" + userName ),
            payloadForLoginFail: ( userId, userName ) => f.Create( i => i.GitLabAccountId = "NO!" + userName )
            );
    }

}

