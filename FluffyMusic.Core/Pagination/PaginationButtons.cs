

using FluffyMusic.Core.Options;

namespace FluffyMusic.Core.Pagination
{
    public static class PaginationButtons
    {
        public const string Next = "Next";
        public const string Prev = "Prev";
        public const string Start = "Start";
        public const string End = "End";
        public const string Close = "Close";
        

        public const string NextId = ButtonPrefixes.QueueButton + "Next";
        public const string PrevId = ButtonPrefixes.QueueButton + "Prev";
        public const string StartId = ButtonPrefixes.QueueButton + "Start";
        public const string EndId = ButtonPrefixes.QueueButton + "End";
        public const string CloseId = ButtonPrefixes.QueueButton + "Close";
    }
}
