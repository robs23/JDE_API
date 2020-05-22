using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JDE_API.Interfaces
{
    public interface IProcessable
    {
        int? Length { get; }
        bool? IsActive { get; set; }
        bool? IsFrozen { get; set; }
        bool? IsCompleted { get; set; }
        bool? IsSuccessfull { get; set; }
        string AssignedUserNames { get; }
    }
}
