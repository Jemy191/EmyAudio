# Emy audio

This is a little audio/music player that I made for myself.  
The goal is to provide a rich experience using **tags** instead of playlist.  
The main idea is to combine **tags** to make playlists instead of having rigid playlists that can't be mixed.

There also other interesting feature like:
* [ ] A smart shuffler that use multiple datapoint to make the randomness more pleasant:
  * [ ] Score.
  * [ ] Last time played.
  * [ ] Number of time played.
  * [ ] Linked audio. -> useful for listening to non music video.
* [ ] Tags group and tags random group. -> To keep similar audio near and randomly near.
* [ ] Cross-platform
* [x] Use your own postgres database

### Explanation
Instead of having:
```
{
    "Playlists":
    [
        [
          "Rock music 1",
          "Rock music 2"
        ],
        [
          "Pop music 1",
          "Pop music 2"
        ],
        [
          "Rock music 1",
          "Pop music 2",
          "Rick Astley - Never Gonna Give You Up"
        ]
    ]
}
```
You have:
```
{
    "Music":
    [
        { "Name": "Rock music 1", "Tags": ["Rock"] },
        { "Name": "Rock music 2", "Tags": ["Rock", "Favorite"] },
        { "Name": "Pop music 1", "Tags": ["Pop", "Favorite"] },
        { "Name": "Pop music 2", "Tags": ["Pop"] },
        { "Name": "Rick Astley - Never Gonna Give You Up", "Tags": ["Pop", "Meme", "Favorite"] },
    ],
    "Tag":
    [
      "Rock",
      "Pop",
      "Favorite",
      "Meme"
    ],
    "Playlist":
    [
      ["Pop", "Rock"],
      ["Rock", "Meme"],
      ["Favorite"]
    ]
}
```

### Installation
* Get the latest version in [Release](https://github.com/Jemy191/EmyAudio/releases)
* Install it where you want
* [Get your google cred](#creating-and-downloading-google-oauth-credential)
* Put it in the installation folder
* [Set up a postgres db](#set-up-a-postgres-db)
* Enjoy 😁

### Build instruction
* [Get your google cred](#creating-and-downloading-google-oauth-credential)
* Put it at `/Core/YoutubeOAuthCred.json` (That file is gitIgnore)
* Enjoy 😁

### Creating and downloading google OAuth credential
* Go to [Google OAuth guide](https://developers.google.com/youtube/v3/guides/auth/installed-apps)
* Download you OAuth credential

### Set up a postgres db
Eiter set up a postgres db with a cloud provider or with the provided docker-compose

### Creating the license file
Use ```dotnet-project-licenses -i .\ConsoleClient\ --outfile ConsoleClientLicense.txt -t```
in the root folder

#### Feel free to ask for feature or question.

#### Pull Request are welcome.