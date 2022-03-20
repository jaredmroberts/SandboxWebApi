using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SandboxWebApi.Filters
{
    public class VendorCheckFilter : ActionFilterAttribute
    {
        public Type DataViewModel { get; set; }
        protected int? VendorId { get; set; }
        protected int? ClaimVendorId { get; set; } = 5;


        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.ContainsKey("VendorId"))
            {
                VendorId = (int?)context.ActionArguments["VendorId"];
            }

            if(!IsPreValid())
                throw new UnauthorizedAccessException();

            if (!CheckContextAsync(context))
                throw new UnauthorizedAccessException();

            await next();
        }

        protected virtual bool CheckContextAsync(ActionExecutingContext context)
        {
            bool isValidAttempt = true;
            foreach (var arg in context.ActionArguments)
            {
                if (arg.Value.GetType().IsClass)
                {
                    //Class Object Checks
                    switch (arg.Value.GetType().Name)
                    {
                        case "CustomersListFilterDataView":
                            isValidAttempt = CheckCustomersListFilterDataView((CustomersListFilterDataView)arg.Value);
                            break;
                        case "CustomerDataView":
                            isValidAttempt = CheckCustomerDataView((CustomerDataView)arg.Value);
                            break;
                        case "LocationDataView":
                            isValidAttempt = CheckLocationDataView((LocationDataView)arg.Value);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    //QueryString / Path Checks
                    switch (arg.Key.ToLower())
                    {
                        case "customerid":
                            isValidAttempt = CheckCustomer((int)arg.Value);
                            break;
                        case "vendorid":
                            isValidAttempt = CheckVendor((int)arg.Value);
                            break;
                        default:
                            break;
                    }
                }
                //If Anything Fails EXIT WITH FALSE
                if (!isValidAttempt)
                    return false;
            }

            return isValidAttempt;
        }

        private bool IsPreValid()
        {
            if (!ClaimVendorId.HasValue)
            {
                return false;
            }

            //Path or QueryString Test
            if (VendorId.HasValue && VendorId != ClaimVendorId)
            {
                return false;
            }

            return true;
        }

        #region Vendor
        private bool CheckVendor(int vendorId)
        {
            return vendorId == ClaimVendorId;
        }
        #endregion

        #region Customer

        /// <summary>
        /// Has VendorId in main entity and can short circuit out. These are search filters and will be using these to query the DB already.
        /// </summary>
        private bool CheckCustomersListFilterDataView(CustomersListFilterDataView dv)
        {
            return dv.VendorId == ClaimVendorId;
        }

        /// <summary>
        /// Has VendorId in main entity and can short circuit out
        /// </summary>
        private bool CheckCustomerDataView(CustomerDataView dv)
        {
            if(dv.VendorId != ClaimVendorId)
                return false;   

            return GetCustomers()
                .Where(x => x.Id == dv.Id && x.VendorId == ClaimVendorId)
                .Any();
        }

        private bool CheckCustomer(int customerId)
        {
            return GetCustomers()
                .Where(x => x.Id == customerId && x.VendorId == ClaimVendorId)
                .Any();
        }

        #endregion

        #region Location

        /// <summary>
        /// DOES NOT have VendorId in main entity and must call db first
        /// </summary>
        private bool CheckLocationDataView(LocationDataView dv)
        {
            return GetCustomers()
                .Where(x => x.Id == dv.CustomerId && x.VendorId == ClaimVendorId)
                .Any();
        }

        #endregion

        #region DataSamples

        private List<CustomerDataView> GetCustomers()
        {
            var customers = new List<CustomerDataView>() {
                new CustomerDataView() { Id = 1, VendorId = 5 },
                new CustomerDataView() { Id = 2, VendorId = 5 },
                new CustomerDataView() { Id = 3, VendorId = 5 },
                new CustomerDataView() { Id = 4, VendorId = 5 },
                new CustomerDataView() { Id = 5, VendorId = 5 },
                new CustomerDataView() { Id = 6, VendorId = 6 },
                new CustomerDataView() { Id = 7, VendorId = 6 },
                new CustomerDataView() { Id = 8, VendorId = 6 },
                new CustomerDataView() { Id = 9, VendorId = 6 },
                new CustomerDataView() { Id = 10, VendorId = 6 }
            };
            return customers;
        }

        #endregion

    }
}
