# assignment-eight-chatting-kj_hunter_game
assignment-eight-chatting-kj_hunter_game created by GitHub Classroom
```
Author:     Kenneth Morse 
Partner:    Hunter Simmons
Start Date: 11-Nov-2025
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  kjmorse2, HunterDSimmons
Repo:       https://github.com/uofu-cs3500-20-fall2025/assignment-eight-chatting-kj_hunter_game.git
Commit Date: 4-Dec-2025
Solution:   Snake 
Copyright:  CS 3500, Kenneth Morse,and Hunter Simmons - This work may not be copied for use in Academic Coursework.
```
# Assignment 9:

### Overview of the functionality
This project uses the NetworkConnection project from Assignment 8 to communicate with a SnakeGame 
server, along with a new Snake project to render that game and allow the user to play with the other
people connected to the same server. 

### Time Expenditures (in hours):
Expected Hours: 30 hours total (15 each) 
Actual Hours 38 total (about 19 each):
- 11/11/25
    - Paired programming:
        - 3 : Added starter code, read through assignment, and started on model classes for json deserialization.
- 11/12/25
    - Paired programming:
        - 2 : Started communication with server, received walls and powerups, not rendered yet though 
    - KJ:
        - 1: Worked on some connection logic, and also starting on rendering, but does not work
    - Hunter:
        -  1: Draws snake name on head, and worked on death animation 
- 11/15/25: 
    - Hunter:  
        - 1: Comments on code so far, worked on drawing more, still bottom right corner only (no translation) 
    - KJ:
        - 1: Clean up code for stylecop errors, and experimenting with javascript for rendering (should have started with this)
- 11/16/25:
    - Paired programming: 
        - 3: Decided on javascript for animation, added fps counter back, and draws snakes on view, still not centered though.  
- 11/ 17/25:
    - KJ:
        - 1: Polished snake drawing (for now)
        - 1: Added control command functionality 
        - 1: World is translated correctly, but not zoomed
        - 1: Added comments to code so far, and polished fps counter 
    - Hunter 
        - 1: Implemented snake color changing for up to 8 players.
    - Paired:
        - 2: Fixed disconnection bugs, program now flows smoothly through all control paths
- 11/18/25:
    - Paired: 
        - 0.5: Removed unused code and refactored some names.
        - 0.5: Arrow keys no longer scroll on webpage  
        - 0.5: Added scoreboard
        - 0.5: Used css to make scoreboard look better. 
        - 1: General cleanup, added TODO's for rest of project.
- 11/19/25
    - Hunter: 
        - 1: Fixed bug on his end, merged his changes to all project was up-to-date 
- 11/20/25:
    - Paired programming
        - 1: Final code review, checked specs to make sure we adhered to them, and polished comments and readmes.
### Sources:
[Microsoft thread safe collections documents](https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2?view=net-9.0)
[Task v.s. Thread](https://stackoverflow.com/questions/4130194/what-is-the-difference-between-task-and-thread)
[Cancelation Tokens](https://learn.microsoft.com/en-us/dotnet/api/system.threading.cancellationtokensource?view=net-9.0)
[Asyncronous and Await](https://www.geeksforgeeks.org/c-sharp/async-and-await-in-c-sharp/)

# Assignment 10:

### Overview of the functionality
This project adds support for recording snake game data from assignment 9 into an SQL database. That data
can then be displayed through a web server.
The snake client now logs each game session, player names, player ids, when players enter and leave, and max scores.
A new Webserver project is created, which reads HTTP requests and sends back the correct HTML to display the webpage.
A Homepage, games pages, or individual game page can be displayed.

### Time Expenditures (in hours):
Expected Hours: 16 hours total (8 hours each) 
Actual Hours 38 total (about 19 each):

- 11/24/25
    - KJ:
        - 1: Initial setup, added WebServer project, readMe, and stylecop.
- 11/25/25
    - Paired programming
        - 2: Setup secrets file, added the GameTable and Players tables to the database.
- 12/2/25
    - Paired programming:
        - 3 :  Basic implementation for adding GameID to the GamesTable. Began working on implementation for adding endtime to gamesTable.
            -  Began setting up the webserver. Basic Homepage and games page setup.
- 12/3/25
    -KJ:
        - 2: Completed Database interface class. GamesTable and Players tables are now updated correctly.
           - Began work on different Webserver approach from Hunter.
    - Paired programming:
        - 3 : All three pages displaying. Still need the functionally to choose and display correct page. 
            - Project Readme complete, some documentation added to new methods.
- 12/4/25: 
    - KJ:
        - 2: Added small readonly classes for DataBase Interface returns instead of out.
           - Webserver pages work as intended.
    -Paired Programming:
        - 2: Added documentation to everything that needed it. Documentation complete.
           - Fixed formatting and other bugs.
           - Solution Readme complete.

### Sources:
[System.Data.SqlClient Namespace](https://learn.microsoft.com/en-us/dotnet/api/system.data.sqlclient?view=netframwork-4.8.1)
[Hypertext Transfer Protocol (HTTP)](http://www.khanacademy.org/computing/computers-and-internet/xcae6f4a7ff015e7d:the-internet/xcae6f4a7ff015e7d:web-protocols/a/hypertext-transfer-protocol-http)
