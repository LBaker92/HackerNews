# HackerNews

A project using Angular 13 and a .NET Core 6 Web Api to grab the latest stories from Hacker News and display them in a paginated list.

## Key Features
- Utilizes an in-memory cache to cache the initial load of stories from the Hacker News API, making subsiquent calls for news data significantly faster.
- Contains back-end unit and integration tests.

## Important Notes
- .NET Core 6 displays a lot of error messages regarding Angular 13 files. This is a known issue and can be ignored.

## How to Run
### Visual Studio Code
  1. Clone the repo and open the root directory in VS Code.
  2. In the terminal, navigate into the HackerNews subdirectory.
  3. If VS Code does not prompt you to restore packages, run dotnet restore and npm i from the terminal.
  4. Press F5 to begin running.

### Visual Studio
  (Visual Studio may contain a lot of error messages regarding Angular. Ignore these, as they are a known issue with Angular 13.)
  1. Clone the repo and open the root directory in File Explorer.
  2. Navigate to the HackerNews subdirectory.
  3. Find and open the HackerNews.sln in Visual Studio.
  4. Press F5 to begin running. (Visual Studio should restore NuGet packages and run npm install for you.)
