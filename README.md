# Cyber-Puzzle

It is recommended to use Unity Hub to import and manage this project.
Project Unity version: 2020.3.30f1c1

## Preparation
1. Download and install Unity Hub from Unity official website.
2. In Unity Hub, Install Unity 2020.3.30f1c1.
3. In Unity Hub, Sign in and apply for a personal free license.

## Import step
1. Clone this repository, or download zip and unzip this project to your local computer.
2. Start Unity Hub and click 'ADD' to select this project folder.
3. Start this project.
4. In the Project window, Open Asset -> Trivia 365 -> Scene and double click Scene to load game scene.
5. Click the Run button at the top to run the game.

#### LeaderBoard Note
- LeaderBoard feature is still connected to my rented cloud server so it may fail down. But this function will not affect the basic gameplay.
- If you want to fix/implement the LeaderBoard, you should have a server installing a database to get data and return data to the game.
- My solution is rent a could server, install nginx, mysql, php and relevant dependency, then write 2 php scripts, one is to receive data and write data into mysql, one is read data from mysql and return to the game request. Then change the url in Asset/Trivia 365/Script/Controller.cs (search for "http://").
