#!/bin/bash

# Default values
load_earthquakes=true
load_ephemeris=true
evaluate_chi_squared_for_earthquake_intervals=true

load_data_start_date="1/1/1900"
load_data_end_date="12/31/2030"
evaluate_start_date="1/1/1960"
evaluate_end_date="12/31/2024"

min_magnitude=7

minimum_interval=5
maximum_interval=45
interval_offset_start=-120
interval_offset_end=120

target_bodies=("jupiter" "saturn" "luna" "venus" "mercury" "mars")

# Navigate to project directory
pushd ../src > /dev/null || exit 1

# Ensure output directory exists
output_dir="output"
mkdir -p "$output_dir"

# Load earthquakes
if [ "$load_earthquakes" = true ]; then
    echo "Loading earthquakes data with magnitude >= $min_magnitude"
    dotnet run -- --command-type LoadEarthquakes \
        --minimum-magnitude "$min_magnitude" \
        --start-date "$load_data_start_date" \
        --end-date "$load_data_end_date"
else
    echo "Skipping earthquakes data loading."
fi

# Load ephemeris
if [ "$load_ephemeris" = true ]; then
    for target_body in "${target_bodies[@]}"; do
        echo "Loading ephemeris data for target body: $target_body"
        dotnet run -- --command-type LoadEphemeris \
            --center-body earth \
            --target-body "$target_body" \
            --start-date "$load_data_start_date" \
            --end-date "$load_data_end_date"
    done
else
    echo "Skipping ephemeris data loading."
fi

# Evaluate Chi Squared
if [ "$evaluate_chi_squared_for_earthquake_intervals" = true ]; then
    for target_body in "${target_bodies[@]}"; do
        echo "Evaluating Chi Squared (onside) for $target_body"
        dotnet run -- --command-type EvaluateChiSquaredForEarthquakeIntervals \
            --center-body earth \
            --target-body "$target_body" \
            --start-date "$evaluate_start_date" \
            --end-date "$evaluate_end_date" \
            --interval-offset-start "$interval_offset_start" \
            --interval-offset-end "$interval_offset_end" \
            --minimum-interval "$minimum_interval" \
            --maximum-interval "$maximum_interval" \
            --alignment onside

        echo "Evaluating Chi Squared (offside) for $target_body"
        dotnet run -- --command-type EvaluateChiSquaredForEarthquakeIntervals \
            --center-body earth \
            --target-body "$target_body" \
            --start-date "$evaluate_start_date" \
            --end-date "$evaluate_end_date" \
            --interval-offset-start "$interval_offset_start" \
            --interval-offset-end "$interval_offset_end" \
            --minimum-interval "$minimum_interval" \
            --maximum-interval "$maximum_interval" \
            --alignment offside
    done
fi

popd > /dev/null
