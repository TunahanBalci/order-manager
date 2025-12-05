using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderApi.Models
{
    public enum OrderStatus
    {
       
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
    
    }
}
