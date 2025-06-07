using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCore.Entities
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public override string ToString()
        {
            return $"Id: {Id}, Title: {Title}, Author: {Author}";
        }
    }
}
