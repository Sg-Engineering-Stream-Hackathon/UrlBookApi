using ModelService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalService
{
    public interface IFunctionalSvc
    {
        Task CreateDefaultUser(string role);
         
    }
}
