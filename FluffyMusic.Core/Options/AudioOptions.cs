namespace FluffyMusic.Core.Options
{
    public class AudioOptions
    {
        public int MinPagesForSkips { get; set; } = 3;
        public int TracksPerPage { get; set; } = 10;
        //  In seconds
        public int PaginationResetDelay { get; set; } = 60;
    }
}
