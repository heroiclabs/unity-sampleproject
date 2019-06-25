Unity Demo Project
==================

> A sample game called Jolly Roger for Unity engine built with Nakama server.

[Nakama](https://github.com/heroiclabs/nakama) is an open-source server designed to power modern games and apps. Features include user accounts, chat, social, matchmaker, realtime multiplayer, and much [more](https://heroiclabs.com).

This game uses most features of the game server with our official [Unity client](https://github.com/heroiclabs/nakama-unity) and is inspired by the popular Clash Royale mobile game. The code is offered as a learning resource for developers to prototype, learn Nakama, and build upon with their own games.

Full server documentation is online - https://heroiclabs.com/docs

## Getting Started

You'll need to setup Unity engine and install Docker engine with our [quickstart guide](https://heroiclabs.com/docs/install-docker-quickstart) for the fastest development experience.

We use docker-compose to make it simple for the database server and game server to be loaded and started together and updates can be easily downloaded from Docker Hub to upgrade. 

To start the game server once Docker is setup navigate to the "ServerModules" folder and run:

```
docker-compose -f ./docker-compose-postgres.yml up
```

Some other useful docker-compose commands are:

* "down" - will stop all running containers defined by the compose file.
* "down -v" - same as above but will also purge the volumes which store data for the containers.

__NOTE:__ You may need to adjust the folder path within the compose file on Windows. For example:

```yml
...
    nakama:
    ...
        volumes:
        - /c/Users/<username>/nakama/data:/nakama/data	# <- Edit
```



## Run Game

The Jolly Roger game is a Unity project that can be opened in Unity version 2018.1 or newer. All project code is within the "JollyRoger" folder.

1. Open the "MainScene.unity" file in "Assets/DemoGame/Scenes".

   If you're on Windows and use Docker Toolbox you must edit the NakamaSessionManager to set the IP address in the code. It is required because Docker Toolbox does not forward ports by default yet.

2. Run the game within the editor or select File > Build and run.
3. Try out any of the other scenes to experiment with them separately from the main game scene.

## Contribute

The collaboration on this code is managed as GitHub issues and pull requests are welcome. If you're interested to enhance the code please open an issue to discuss the changes or drop in and discuss it in the [community forum](https://forum.heroiclabs.com).

### License

This project source code and documentation is licensed under the [Apache-2 License](https://github.com/heroiclabs/unity-sampleproject/blob/master/LICENSE). All images, graphics, and other non-code resources are licensed under [CC BY-NC-ND](https://creativecommons.org/licenses/by-nc-nd/4.0/). Please reach out on a GitHub issue if you have any questions.

All 3rd-party assets and libraries used in this project retain all rights under their respective licenses. See the ""
