using fs_2025_assessment_1_74154.Models;

namespace fs_2025_assessment_1_74154.Helpers
{
    public static class StationSortingHelper
    {
        public static IEnumerable<Station> Sort(IEnumerable<Station> stations, string sort, string dir)
        {
            return (sort.ToLower(), dir.ToLower()) switch
            {
                ("name", "asc") => stations.OrderBy(s => s.Name),
                ("name", "desc") => stations.OrderByDescending(s => s.Name),
                ("availablebikes", "asc") => stations.OrderBy(s => s.AvailableBikes),
                ("availablebikes", "desc") => stations.OrderByDescending(s => s.AvailableBikes),
                ("occupancy", "asc") => stations.OrderBy(s => s.Occupancy),
                ("occupancy", "desc") => stations.OrderByDescending(s => s.Occupancy),
                (_, "asc") => stations.OrderBy(s => s.Name),
                (_, "desc") => stations.OrderByDescending(s => s.Name),
                _ => stations.OrderBy(s => s.Name)
            };
        }
    }
}
