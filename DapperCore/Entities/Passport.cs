using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCore.Entities
{
    public class Passport
    {
        public int Id { get; set; }
        public string PassportNumber { get; set; } = null!;
        public int VisitorId { get; set; }
        public Visitor Visitor { get; set; } = null!;
    }
}
