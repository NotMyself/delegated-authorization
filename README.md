# Delegated Authorization

**Scenario:** I am building an application with no database.
It will mainly consume 3rd party APIs on behalf of a user.
That user will do authentication using Auth0 through OpenID Connect.
Then the user will authorize Github and Twitch API access via OAuth 2.

## Getting Started

As long as you have the development dependencies installed, the application can be run on your bare metal machine.

1. Clone the repository: `git clone git clone https://github.com/NotMyself/delegated-authorization.git`
1. Change directory into the cloned repository `cd bivrost delegated-authorization`
1. Run the command `dotnet user-secrets set Auth0:Domain {auth0-tenant-domain}`.
1. Run the command `dotnet user-secrets set Auth0:ClientId {auth0-client-id}`.
1. Run the command `dotnet user-secrets set Auth0:ClientSecret {auth0-client-secret}`.
1. Run the command `dotnet user-secrets set Github:ClientId {github-client-id}`.
1. Run the command `dotnet user-secrets set Github:ClientSecret {github-client-secret}`.
1. Run the command `dotnet user-secrets set  Twitch:ClientId {twitch-client-id}`.
1. Run the command `dotnet user-secrets set Twitch:ClientSecret {twitch-client-secret}`.
1. Run the command `dotnet restore src/server`
1. Run the command `dotnet watch src/server`

## Obtaining Client Credentials

You will need to create an Auth0 tenant and configure a client application for OIDC authentication. You will also need to create OAuth 2 client credentials for Twitch and Github.

| Service  |      Callback URI     |
|----------------|:-------------:|
| [Auth0](https://auth0.com/) (OIDC) | https://localhost:5001/callback |
| [Twitch](https://dev.twitch.tv/console) (OAuth) |https://localhost:5001/authorize-twitch |
| [Github](https://github.com/settings/developers) (OAuth) |https://localhost:5001/authorize-github |

## Demo

[![Demo](http://img.youtube.com/vi/Xn5e4-a9QQw/0.jpg)](http://www.youtube.com/watch?v=Xn5e4-a9QQw "Demo")
