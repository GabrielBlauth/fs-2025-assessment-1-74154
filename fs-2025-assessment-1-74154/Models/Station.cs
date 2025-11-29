namespace fs_2025_assessment_1_74154.Models;

public class Station
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Position Position { get; set; } = new();
    public int BikeStands { get; set; }
    public int AvailableBikeStands { get; set; }
    public int AvailableBikes { get; set; }
    public string Status { get; set; } = "OPEN";
    public long LastUpdate { get; set; }

    public string id { get; set; } = string.Empty; //PROPERTY FOR COSMOSDB

    // Computed properties
    public string LastUpdateLocal
    {
        get
        {
            var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(LastUpdate);
            var dublinTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dateTime, "Europe/Dublin");
            return dublinTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public double Occupancy
    {
        get
        {
            return BikeStands > 0 ? (double)AvailableBikes / BikeStands : 0;
        }
    }
}

public class Position
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}