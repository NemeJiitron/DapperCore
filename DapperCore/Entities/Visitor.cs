using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCore.Entities
{
    public class Visitor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Phonenumber { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public Passport Passport { get; set; }
        public List<LoanInfo> Loans { get; set; } = null!;

    }
}
