using System;

namespace SandboxWebApi
{
    public class CustomersListFilterDataView
    {
        public string SearchQuery { get; set; }
        public string CustomerTypeIds { get; set; }
        public string CustomerStatusTypeIds { get; set; }
        public int? VendorId { get; set; }
    }

    public class CustomerDataView
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
    }

    public class LocationDataView
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
    }
}
