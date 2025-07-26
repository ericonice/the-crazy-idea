# The Crazy Idea

## Prerequisites

- .NET 9.0 SDK
- PostgreSQL 15.0 or later

## Installation

1. Clone the repository:
   ```
   git clone git@github.com:ericonice/the-crazy-idea.git
   ```
2. Restore NuGet packages:
   ```
   dotnet restore
   ```
3. Build the solution:
   ```
   dotnet build
   ```
4. Modify the `appsettings.json` file to configure your PostgreSQL connection string:
   ```json
   {
	 "ConnectionStrings": {
	   "DefaultConnection": "Host=localhost;Port=5432;Database=the_crazy_idea;Username=your_username;Password=your_password"
	 }
   }
   ```
   Note: The first time the application is run, it will create the database and schema automatically, so there is not need to create the database manually.

## Usage

Run the application with a command type, e.g.:

```
dotnet run -- --command-type EvaluateChiSquaredForEarthquakeIntervals --center-body earth --target-body jupiter --minimum-magnitude 7 --start-date 1/1/1960 --end-date 12/31/2024 --interval-offset-start -60 --interval-offset-end 60 --minimum-interval 5 --maximum-interval 60
```

Run application with help for more info:
```
dotnet run -- --help
```

## Initial Data Load and Data Collection for Windows
```
./run.ps1
```

