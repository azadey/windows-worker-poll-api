using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NightFisionAutomatedPrintAndPickList
{
    internal class UnleashedExceptionService
    {
        public async Task HandleExceptionAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        // Handle bad request error
                        break;
                    case HttpStatusCode.Unauthorized:
                        // Handle unauthorized error
                        break;
                    case HttpStatusCode.NotFound:
                        // Handle not found error
                        break;
                    // Add more cases as needed
                    default:
                        // Handle other errors
                        break;
                }
            }
        }

        public async Task HandleExceptionAsync(Exception ex)
        {
            // Handle exception
        }
    }
}
