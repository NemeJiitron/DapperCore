using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapper;
using DapperCore.Entities;
using DapperCore.HomeWork.Entities;
using Microsoft.Data.Sqlite;

namespace DapperCore.HomeWork
{
    public class AppDbContext
    {
        public void Menu(SqliteConnection connection)
        {
            while(true)
            {
                Console.WriteLine("1 - insert dog\n2 - read dogs\n3 - search dog by\n4 - update dog\n5 - insert adopter\n6 - sheltering\n0 - quit");
                int choice = int.Parse(Console.ReadLine());
                switch(choice)
                {
                    case 1:
                        InsertDog(connection);
                        break;
                    case 2:
                        Console.WriteLine("1 - read not adopted\n2 - read adopted");
                        int choice2 = int.Parse(Console.ReadLine());
                        if (choice2 == 1)
                        {
                            ReadNotAdopted(connection);
                        }
                        else if(choice2 == 2)
                        {
                            ReadAdopted(connection);
                        }
                        break;
                    case 3:
                        SearchDogBy(connection);
                        break;
                    case 4:
                        Console.WriteLine("Enter dog Id:");
                        int id = int.Parse(Console.ReadLine());
                        UpdateDogBy(connection, id);
                        break;
                    case 5:
                        InsertAdopter(connection);
                        break;
                    case 6:
                        Console.WriteLine("Enter adopter Id:");
                        int adopterid = int.Parse(Console.ReadLine());
                        AdoptADog(connection, adopterid);
                        break;
                }

                if(choice == 0)
                {
                    break;
                }
            }
        }

        public void initDb(SqliteConnection connection)
        {
            connection.Execute(@"
            create table if not exists Dogs
            (
                Id integer primary key autoincrement,
                Name text not null,
                Age integer not null,
                Breed text not null,
                IsAdopted integer default 0 not null check(IsAdopted in (0, 1)),
                AdopterId int null,
                foreign key (AdopterId) references Adopters(Id)
            );
            create table if not exists Adopters
            (
                Id integer primary key autoincrement,
                Name text not null,
                PhoneNumber text not null
            );");
        }

        public void InsertDog(SqliteConnection connection)
        {
            string insertDog = @"
                    insert into Dogs(Name, Age, Breed, IsAdopted)
                    values (@Name, @Age, @Breed, @IsAdopted);";
            Console.WriteLine("Dog name:");
            string name = Console.ReadLine();
            Console.WriteLine("Dog age:");
            int age = int.Parse(Console.ReadLine());
            Console.WriteLine("Dog breed:");
            string breed = Console.ReadLine();
            Console.WriteLine("Is dog adopted (0 or 1):");
            int adopted = int.Parse(Console.ReadLine());
            if(adopted == 1)
            {
                Console.WriteLine("Id of dog adopter:");
                int adopter = int.Parse(Console.ReadLine());
                if(connection.Query<Adopter>($"select * from Adopters a where a.Id = {adopter};").ToList().Any())
                {
                    string insertAdoptedDog = @"
                    insert into Dogs(Name, Age, Breed, IsAdopted, AdopterId)
                    values (@Name, @Age, @Breed, @IsAdopted, @AdopterId);";
                    connection.Execute(insertAdoptedDog, new { Name = name, Age = age, Breed = breed, IsAdopted = adopted, AdopterId = adopter });
                } else { Console.WriteLine("Adopter is non-existant. Try again"); }
            }
            else 
            {
                connection.Execute(insertDog, new { Name = name, Age = age, Breed = breed, IsAdopted = adopted });
            }
        }

        public void InsertAdopter(SqliteConnection connection)
        {
            string insertAdopter = @"
                    insert into Adopters(Name, PhoneNumber)
                    values (@Name, @PhoneNumber);";
            Console.WriteLine("Adopter name:");
            string name = Console.ReadLine();
            Console.WriteLine("Adopter phonenumber:");
            string phone = Console.ReadLine();
            connection.Execute(insertAdopter, new { Name = name, PhoneNumber = phone });
        }

        public void AdoptADog(SqliteConnection connection, int adopterId)
        {
            string adoptersQuery = @$"
                        select * from Adopters a where a.Id = {adopterId};";
            var adopters = connection.Query<Adopter>(adoptersQuery).ToList();
            if (adopters.Any())
            {
                Console.WriteLine("Dog id: ");
                int dogId = int.Parse(Console.ReadLine());
                if (connection.Query<Dog>($"select * from Dogs d where d.Id = {dogId} and d.IsAdopted = 0;").ToList().Any())
                {
                    string updateDog = @$"
                    update Dogs
                    set AdopterId = @AdopterId, IsAdopted = 1
                    where Id = {dogId};";
                    connection.Execute(updateDog, new { AdopterId = adopterId });
                }
                else { Console.WriteLine("Dog is adopted or non-existant"); }
            }
            else { Console.WriteLine("Adopter is non-existant"); }
        }

        private static void ReadAdopted(SqliteConnection connection)
        {
            string dogsQuery = @"
                    select a.Id, a.Name, a.PhoneNumber, d.Id, d.Name, d.Age, d.Breed, d.IsAdopted, d.AdopterId from Adopters a
                    join Dogs d on d.AdopterId = a.Id;";

            var dogsMap = new Dictionary<int, Adopter>();

            var adopters = connection.Query<Adopter, Dog, Adopter>(
                dogsQuery,
                (a, d) =>
                {
                    if (!dogsMap.TryGetValue(a.Id, out Adopter adopter))
                    {
                        adopter = a;
                        adopter.Dogs = new List<Dog>();
                        dogsMap.Add(adopter.Id, adopter);
                    }
                    if (d != null)
                        adopter.Dogs.Add(d);

                    return adopter;
                },
                splitOn: "Id"
                ).Distinct().ToList();
            foreach (var adopter in adopters)
            {
                Console.WriteLine(adopter.Id.ToString() + ". " + adopter.Name + ": ");
                foreach (var dog in adopter.Dogs)
                {
                    Console.WriteLine(dog.ToString());
                }
            }
        }

        public void ReadNotAdopted(SqliteConnection connection)
        {
            string dogsQuery = @"
                        select * from Dogs d where d.IsAdopted = 0;";
            var dogs = connection.Query<Dog>(dogsQuery).ToList();
            foreach (var d in dogs)
            {
                Console.WriteLine(d.ToString());
            }
        }

        public void UpdateDogBy(SqliteConnection connection, int dogId)
        {
            string dogsQuery = @$"
                        select * from Dogs d where d.Id = {dogId};";
            var dogs = connection.Query<Dog>(dogsQuery).ToList();
            if(dogs.Any())
            {
                Console.WriteLine("Dog name:");
                string name = Console.ReadLine();
                Console.WriteLine("Dog age:");
                int age = int.Parse(Console.ReadLine());
                Console.WriteLine("Dog breed:");
                string breed = Console.ReadLine();
                Console.WriteLine("Is dog adopted (0 or 1):");
                int adopted = int.Parse(Console.ReadLine());
                string updateDog = @$"
                    update Dogs
                    set Name = @Name, Age = @Age, Breed = @Breed, IsAdopted = @IsAdopted
                    where Id = {dogId};";
                connection.Execute(updateDog, new { Name = name, Age = age, Breed = breed, IsAdopted = adopted });
            }
            else
            {
                Console.WriteLine("Dog not found");
            }
        }

        private static void SearchDogBy(SqliteConnection connection)
        {
            Console.WriteLine("Search by:\n\t1 - Name\n\t2 - Id\n\t3 - Breed");
            int choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    string name = Console.ReadLine();
                    var dogs1 = connection.Query<Dog>($"select * from Dogs where Name = '{name}';").ToList();
                    foreach(var dog in dogs1)
                    {
                        Console.WriteLine(dog.ToString());
                    }
                    break;
                case 2:
                    int id = int.Parse(Console.ReadLine());
                    var dogs2 = connection.Query<Dog>($"select * from Dogs where Id = {id};").ToList();
                    foreach (var dog in dogs2)
                    {
                        Console.WriteLine(dog.ToString());
                    }
                    break;
                case 3:
                    string breed = Console.ReadLine();
                    var dogs3 = connection.Query<Dog>($"select * from Dogs where Breed = '{breed}'").ToList();
                    foreach (var dog in dogs3)
                    {
                        Console.WriteLine(dog.ToString());
                    }
                    break;
            }
        }

    }
}
