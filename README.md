# The Crazy Idea

## Prerequisites

* .NET 9.0 SDK
* PostgreSQL 15.0 or later

---

## Installation

1. **Clone the repository:**

   ```bash
   git clone git@github.com:ericonice/the-crazy-idea.git
   cd the-crazy-idea
   ```

2. **Restore NuGet packages:**

   ```bash
   dotnet restore
   ```

3. **Build the solution:**

   ```bash
   dotnet build
   ```

4. **Set required environment variables:**

   | Variable                               | Description                        | Example Value                                                                            |
   | -------------------------------------- | ---------------------------------- | ---------------------------------------------------------------------------------------- |
   | `ConnectionStrings__DefaultConnection` | PostgreSQL connection string       | `Host=localhost;Port=5432;Database=the_crazy_idea;Username=your_user;Password=your_pass` |
   | `OutputDirectory`                      | Optional path for generated output | `C:\projects\the-crazy-idea\output`                                                      |

   ### 🔧 How to set environment variables:

   #### 🗾 PowerShell

   ```powershell
   $env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=5432;Database=the_crazy_idea;Username=your_user;Password=your_pass"
   $env:OutputDirectory = "C:\path\to\output"
   ```

   #### 🗉️ Bash (macOS, Linux, Git Bash)

   ```bash
   export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=the_crazy_idea;Username=your_user;Password=your_pass"
   export OutputDirectory="./output"
   ```

---

## Usage

To run the application:

```bash
dotnet run -- --command-type EvaluateChiSquaredForEarthquakeIntervals --center-body earth --target-body jupiter --minimum-magnitude 7 --start-date 1/1/1960 --end-date 12/31/2024 --interval-offset-start -60 --interval-offset-end 60 --minimum-interval 5 --maximum-interval 60
```

To see available options:

```bash
dotnet run -- --help
```

### Help Output

```
--alignment                Alignment type (All, Onside, Offside)
--center-body              (Default: earth) Center Body
--command-type             Required. Type of command to perform (LoadEarthquakes, GetEarthquakes, GetEarthquakesWithEphemeris,
                           LoadEphemeris, GetEphemeris, EvaluateChiSquaredForEarthquakeIntervals, DetermineEarthquakesInInterval)
--end-date                 End Date, e.g. 01/30/2010
--interval-offset-end      (Default: 15) Interval Offset End in Days
--interval-offset-start    (Default: -30) Interval Offset Start in Days
--maximum-interval         (Default: 60) Maximum Interval in Days
--minimum-interval         (Default: 5) Minimum Interval in Days
--minimum-magnitude        Minimum magnitude
--start-date               Start Date, e.g. 1/1/1960
--target-body              Target Body
--help                     Display this help screen.
--version                  Display version information.
```

---

## Initial Data Load

Use the provided script:

* **Windows:**

  ```powershell
  ./run.ps1
  ```

* **macOS/Linux:**

  ```bash
  ./run.sh
  ```

> ⚠️ Note: The application will automatically create the database and schema on first run — no manual setup required.
