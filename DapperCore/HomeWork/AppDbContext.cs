using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Console.WriteLine("1 - insert dog\n2 - read dogs\n3 - search by\n4 - update dog\n0 - quit");
                int choice = int.Parse(Console.ReadLine());
                switch(choice)
                {
                    case 1:
                        Insert(connection);
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
                IsAdopted integer default 0 not null check(IsAdopted in (0, 1))
            );");
        }

        public void Insert(SqliteConnection connection)
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
            connection.Execute(insertDog, new { Name = name, Age = age, Breed = breed, IsAdopted = adopted });
        }
        public void ReadAdopted(SqliteConnection connection)
        {
            string dogsQuery = @"
                        select * from Dogs d where d.IsAdopted = 1;";
            var dogs = connection.Query<Dog>(dogsQuery).ToList();
            foreach (var d in dogs)
            {
                Console.WriteLine(d.ToString());
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
