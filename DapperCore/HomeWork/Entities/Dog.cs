using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperCore.HomeWork.Entities
{
    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Breed { get; set; }
        public int IsAdopted{ get; set; }
        public int? AdopterId { get; set; }
        public Adopter? Adopter { get; set; }

        public override string ToString()
        {
            return $"{Id}. Name: {Name}, Age: {Age}, Breed: {Breed}, Adopted: {(IsAdopted == 1 ? $"Yes, AdopterId: {AdopterId}" : "No")}";
        }

    }
}
