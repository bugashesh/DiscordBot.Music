using System;

namespace FluffyMusic.Core.Pagination
{
    public class PageInfo
    {
        public int CurrentPage { get; set; }
        public int RecordsPerPage { get; set; }
        public int TotalRecords { get; set; }
        public int RecordsOnPage { get; set; }
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value == 0 ? 1 : value;
            }
        }

        public readonly ulong MessageOwnerId;

        private int _totalPages;

        public PageInfo(int totalRecords, int recordsPerPage, ulong messageOwnerId)
        {
            RecordsPerPage = recordsPerPage;
            CurrentPage = 1;
            MessageOwnerId = messageOwnerId;
            UpdateInfo(totalRecords);
        }

        public void UpdateInfo(int totalRecords)
        {
            TotalPages = (int)Math.Ceiling(totalRecords / (double)RecordsPerPage);
            TotalRecords = totalRecords;
            RecordsOnPage = totalRecords >= RecordsPerPage ? RecordsPerPage : totalRecords;
        }

        private void UpdateRecordsOnPage()
        {
            RecordsOnPage = TotalRecords >= RecordsPerPage * CurrentPage
            ? RecordsPerPage * CurrentPage
            : TotalRecords;
        }

        public void ToStart()
        {
            CurrentPage = 1;
            UpdateRecordsOnPage();
        }

        public void ToEnd()
        {
            CurrentPage = TotalPages;
            UpdateRecordsOnPage();
        }

        public bool PageUp()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
                UpdateRecordsOnPage();
                return true;
            }
            return false;
        }

        public bool PageDown()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
                UpdateRecordsOnPage();
                return true;
            }
            return false;
        }

        public int Start()
        {
            return (CurrentPage - 1) * RecordsPerPage;
        }

        public int End(int totalRecords)
        {
            if (totalRecords != 0)
            {
                int offset = totalRecords - Start() > RecordsPerPage
                    ? RecordsPerPage
                    : totalRecords % RecordsPerPage == 0
                    ? RecordsPerPage :
                    totalRecords % RecordsPerPage;

                return Start() + offset;
            }
            return totalRecords;
        }
    }
}
