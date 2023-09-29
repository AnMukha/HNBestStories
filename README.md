# HNBestStories

This application serves as a middleware RESTful API that fetches the first n best stories from the Hacker News API and returns them in descending order based on their score.

Installation and Running

Clone the repository to your local machine:

git clone https://github.com/AnMukha/HNBestStories

Navigate to the directory of the project:

cd HNBestStories

Restore the necessary packages

dotnet restore

Build the application:

dotnet build

Run the application:

cd HNBestStories

dotnet run

Or simply open the solution in Visual Studio and press F5 to run.

API Usage

Make a GET request to:

http://localhost:5177/api/get-best-stories?n=30

Where n=30 is the number of best stories you'd like to retrieve.

Or open Swagger page:
http://localhost:5177/swagger/index.html


App confuguration (appsettings.json):

  "Options": {
    "IdsCacheExpirationSecounds": 10,
    "StoriesCacheExpirationSecounds": 60,
    "NumberOfParallelRequests": 10
  }

These parameters limit the HN API load. Data is requested from the HN API only if cached data has not yet expired.

Potential Enhancements or Changes

- Add error handling for scenarios where the Hacker News API is unreachable or returns unexpected data.
- Add several unit tests and some integration tests.
- Add logging to a file or another method (currently has console-only logging).
- 
