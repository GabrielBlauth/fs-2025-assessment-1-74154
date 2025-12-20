using fs_2025_assessment_1_74154.Models;

namespace fs_2025_assessment_1_74154.Helpers
{
    public static class StationValidator
    {
        public static List<string> Validate(Station station)
        {
            var errors = new List<string>();

            if (station.Number <= 0)
                errors.Add("Number must be a positive integer.");

            if (string.IsNullOrWhiteSpace(station.Name))
                errors.Add("Name is required.");

            if (string.IsNullOrWhiteSpace(station.Address))
                errors.Add("Address is required.");

            if (station.BikeStands < 0)
                errors.Add("BikeStands cannot be negative.");

            if (station.AvailableBikes < 0)
                errors.Add("AvailableBikes cannot be negative.");

            if (station.AvailableBikeStands < 0)
                errors.Add("AvailableBikeStands cannot be negative.");

            if (station.BikeStands > 0)
            {
                if (station.AvailableBikes > station.BikeStands)
                    errors.Add("AvailableBikes cannot exceed BikeStands.");

                if (station.AvailableBikeStands > station.BikeStands)
                    errors.Add("AvailableBikeStands cannot exceed BikeStands.");

                if (station.AvailableBikes + station.AvailableBikeStands != station.BikeStands)
                    errors.Add("AvailableBikes + AvailableBikeStands must equal BikeStands.");
            }

            if (string.IsNullOrWhiteSpace(station.Status))
            {
                errors.Add("Status is required.");
            }
            else
            {
                var status = station.Status.ToUpperInvariant();
                if (status != "OPEN" && status != "CLOSED")
                    errors.Add("Status must be 'OPEN' or 'CLOSED'.");
            }

            return errors;
        }
    }
}
