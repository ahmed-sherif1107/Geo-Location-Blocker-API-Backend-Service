namespace Countries.Dtos
{

    /*
    this class is used to receive input from API clients for:
    - Which page of data they want (Page)
    - How many items per page (PageSize)
    - An optional search term (SearchTerm)


    */
    public class PaginationRequest
    {
        private int _page = 1;
        private int _pageSize = 10;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
        }

        public string? SearchTerm { get; set; }
    }
}