using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SandboxWebApi.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SandboxWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILogger<CustomerController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [VendorCheckFilter()]
        [Route("{customerId}")]
        public async Task<CustomerDataView> Get(int customerId)
        {
            return new CustomerDataView() { Id = customerId, VendorId = 5 };
        }

        [HttpGet]
        [VendorCheckFilter()]
        [Route("")]
        public async Task<CustomerDataView> GetCustomer([FromQuery]int customerId)
        {
            return new CustomerDataView() { Id = customerId, VendorId = 5 };
        }

        [HttpPost]
        [VendorCheckFilter()]
        public async Task<CustomerDataView> Post(int vendorId, CustomerDataView entity)
        {
            return entity;
        }

        [HttpPost]
        [Route("search")]
        [VendorCheckFilter()]
        public async Task<CustomersListFilterDataView> Search(CustomersListFilterDataView test2)
        {
            return test2;
        }

        [HttpPost]
        [Route("{vendorId}/search2")]
        [VendorCheckFilter()]
        public async Task<CustomersListFilterDataView> Search2(int vendorId, CustomersListFilterDataView test2)
        {
            return test2;
        }



        [HttpPost]
        [Route("location")]
        [VendorCheckFilter()]
        public async Task<LocationDataView> LocationPost(LocationDataView entity)
        {
            return entity;
        }
    }
}
