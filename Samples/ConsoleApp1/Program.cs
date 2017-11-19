﻿using System;
using Nightingale;
using Nightingale.Extensions;
using Nightingale.Logging;
using Nightingale.Sessions;
using Nightingale.SQLite;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionFactory = new SQLiteConnectionFactory {DataSource = "persons.s3db"};
            var sessionFactory = new SessionFactory(connectionFactory) {Logger = new TraceLogger(LogLevel.Debug)};

            // register entity listeners
            sessionFactory.EntityListeners.Add(new PersonEntityService());
            sessionFactory.EntityListeners.Add(new AddressEntityService());

            var session = sessionFactory.GetCurrentSession();

            // create tables if not available
            session.GetTable<Person>().Recreate();
            session.GetTable<Address>().Recreate();

            // create entities
            var person = new Person
            {
                FirstName = "Max",
                Name = "Mustermann",
                Age = 21
            };

            var address = new Address
            {
                ValidFrom = DateTime.Today,
                Zip = "0815",
                Town = "SampleTown",
                Street = "Samplestreet 75a",
                Type = AddressType.Business
            };

            person.Addresses.Add(address);

            using (session.BeginTransaction())
            {
                session.SaveOrUpdate(person);
                session.Commit();
            }

            session.Dispose();

            // creating a new session so that the entities aren't cached anymore
            session = sessionFactory.GetCurrentSession();

            var loadedPerson = session.Get<Person>(1);

            Console.WriteLine($"Person: {loadedPerson.FullName}, IsLegalAge: {loadedPerson.IsLegalAge}");

            foreach (var addr in loadedPerson.ValidAddresses)
                Console.WriteLine($"{addr.Street}, Type={addr.Type}");

            using (session.BeginTransaction())
            {
                try
                {
                    // try to remove the only address from person
                    var addr = loadedPerson.Addresses[0];
                    session.Delete(addr);
                    session.Commit();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"The address could not be deleted. {e.Message}");
                }
            }

            Console.WriteLine("Press any key to continue ..");
            Console.ReadLine();
        }
    }
}
