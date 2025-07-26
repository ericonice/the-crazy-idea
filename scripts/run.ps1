param (
    [bool]$load_earthquakes = $true,
    [bool]$load_ephemeris = $true,    
    [bool]$evaluate_chi_squared_for_earthquake_intervals = $true,

    [string]$load_data_start_date = "1/1/1900",
    [string]$load_data_end_date = "12/31/2030",
    [string]$evaluate_start_date = "1/1/1960",
    [string]$evaluate_end_date = "12/31/2024",

    [int]$min_magnitude = 7,

    [int]$minimum_interval = 5,
    [int]$maximum_interval = 45,
    [int]$interval_offset_start = -120,
    [int]$interval_offset_end = 120,

    [ValidateSet("jupiter", "saturn", "luna", "venus", "mercury", "mars")]
    [string[]]$target_bodies = @("jupiter", "saturn", "luna", "venus", "mercury", "mars")
)

# Navigate to the project directory
Push-Location ../src

# Ensure output directory exists
$outputDir = "output"
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Earthquake loading section
if ($load_earthquakes) {
    Write-Host "Loading earthquakes data with magnitude >= $min_magnitude"
    dotnet run -- --command-type LoadEarthquakes --minimum-magnitude $min_magnitude --start-date $load_data_start_date --end-date $load_data_end_date
} else {
    Write-Host "Skipping earthquakes data loading."
}

# Ephemeris loading section (for multiple target bodies)
if ($load_ephemeris) {
    foreach ($target_body in $target_bodies) {
        Write-Host "Loading ephemeris data for target body: $target_body"
        dotnet run -- --command-type LoadEphemeris --center-body earth --target-body $target_body --start-date $load_data_start_date --end-date $load_data_end_date
    }
} else {
    Write-Host "Skipping ephemeris data loading."
}

# Chi Squared evaluation section
if ($evaluate_chi_squared_for_earthquake_intervals) {
    foreach ($target_body in $target_bodies) {
        Write-Host "Evaluating Chi Squared for earthquake intervals for target body: $target_body"
        dotnet run -- --command-type EvaluateChiSquaredForEarthquakeIntervals `
                     --center-body earth `
                     --target-body $target_body `
                     --start-date $evaluate_start_date `
                     --end-date $evaluate_end_date `
                     --interval-offset-start $interval_offset_start `
                     --interval-offset-end $interval_offset_end `
                     --minimum-interval $minimum_interval `
                     --maximum-interval $maximum_interval `
                     --alignment onside

        dotnet run -- --command-type EvaluateChiSquaredForEarthquakeIntervals `
                     --center-body earth `
                     --target-body $target_body `
                     --start-date $evaluate_start_date `
                     --end-date $evaluate_end_date `
                     --interval-offset-start $interval_offset_start `
                     --interval-offset-end $interval_offset_end `
                     --minimum-interval $minimum_interval `
                     --maximum-interval $maximum_interval `
                     --alignment offside
    }
}

Pop-Location