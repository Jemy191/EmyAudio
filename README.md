

### Build instruction
* Go to [Google OAuth guide](https://developers.google.com/youtube/v3/guides/auth/installed-apps)
* Download you OAuth credential
* Put it at ```/Core/YoutubeOAuthCred.json```(That file is gitIgnore)


### Creating the license file
Use ```dotnet-project-licenses -i .\ConsoleClient\ --outfile ConsoleClientLicense.txt -t```
in the root folder