﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tik4net.Api;
using tik4net.Objects;
using tik4net.Objects.Ip.Firewall;

namespace tik4net.examples
{
    class Program
    {
        static void Main(string[] args)
        {
            using (ITikConnection connection = new ApiConnection())
            {
                connection.OnReadRow += Connection_OnReadRow;
                connection.OnWriteRow += Connection_OnWriteRow;
                connection.Open(ConfigurationManager.AppSettings["host"], ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["pass"]);

                //Identity(connection);

                //Torch(connection);

                //Log(connection);

                //PrintAddressList(connection);
                //CreateOrUpdateAddressList(connection);
                //PrintAddressList(connection);
                //CreateOrUpdateAddressList(connection);
                //PrintAddressList(connection);
                //DeleteAddressList(connection);
                //PrintAddressList(connection);

                PrintAddressList(connection);
                CreateOrUpdateAddressListMulti(connection);
                PrintAddressList(connection);
                CreateOrUpdateAddressListMulti(connection);
                PrintAddressList(connection);
                DeleteAddressListMulti(connection);
                PrintAddressList(connection);

                Console.WriteLine("Finito - press ENTER");
                Console.ReadLine();
            }
        }

        private static void Connection_OnWriteRow(object sender, string word)
        {
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.WriteLine(">" + word);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        private static void Connection_OnReadRow(object sender, string word)
        {
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("<" + word);
            Console.BackgroundColor = ConsoleColor.Black;

        }

        private static void Identity(ITikConnection connection)
        {
            ApiCommand cmd = new ApiCommand(connection, "/system/identity/print");
            var identity = cmd.ExecuteSingleRow();
            Console.WriteLine("Identity: " + identity.GetResponseField("name"));

            Console.WriteLine("Press ENTER");
            Console.ReadLine();
        }

        private static void Torch(ITikConnection connection)
        {
            ApiCommand torchCmd = new ApiCommand(connection, "/tool/torch", new ApiCommandParameter("interface", "ether1"));
            torchCmd.ExecuteAsync(response =>
            {
                Console.WriteLine("Row: " + response.GetResponseField("tx"));
            });
            Console.WriteLine("Press ENTER");
            Console.ReadLine();
            torchCmd.Cancel();
        }

        private static void Log(ITikConnection connection)
        {
            var logs = connection.LoadList<Log>();
            foreach (Log log in logs)
            {
                Console.WriteLine("{0}[{1}]: {2}", log.Time, log.Topics, log.Message);
            }
        }


        const string listName = "TEST_LIST";
        const string ipAddress = "192.168.1.1";
        const string ipAddress2 = "192.168.1.2";
        private static void PrintAddressList(ITikConnection connection)
        {
            var addressLists = connection.LoadList<AddressList>(
                connection.CreateParameter("list", listName));
            foreach (AddressList addressList in addressLists)
            {
                Console.WriteLine("{0}{1}: {2} {3} ({4})", addressList.Disabled ? "X" : " ", addressList.Dynamic ? "D" : " ", addressList.Address, addressList.List, addressList.Comment);
            }
        }

        private static void CreateOrUpdateAddressList(ITikConnection connection)
        {
            var existingAddressList = connection.LoadList<AddressList>(
                connection.CreateParameter("list", listName),
                connection.CreateParameter("address", ipAddress)).SingleOrDefault();
            if (existingAddressList == null)
            {
                //Create
                var newAddressList = new AddressList()
                {
                    Address = ipAddress,
                    List = listName,
                };
                connection.Save(newAddressList);
            }
            else
            {
                //Update
                existingAddressList.Comment = "Comment update: " + DateTime.Now.ToShortTimeString();

                connection.Save(existingAddressList);
            }
        }

        private static void DeleteAddressList(ITikConnection connection)
        {
            var existingAddressList = connection.LoadList<AddressList>(
                connection.CreateParameter("list", listName),
                connection.CreateParameter("address", ipAddress)).SingleOrDefault();

            if (existingAddressList != null)
                connection.Delete(existingAddressList);
        }

        private static void CreateOrUpdateAddressListMulti(ITikConnection connection)
        {
            var existingAddressList = connection.LoadList<AddressList>(
                connection.CreateParameter("list", listName)).ToList();
            var listClonedBackup = existingAddressList.CloneEntityList(); //creates clone of all entities in list

            if (existingAddressList.Count() <= 0)
            {
                //Create (just in memory)
                existingAddressList.Add(
                    new AddressList()
                    {
                        Address = ipAddress,
                        List = listName,
                    });
                existingAddressList.Add(
                    new AddressList()
                    {
                        Address = ipAddress2,
                        List = listName,
                    });
            }
            else
            {
                //Update (just in memory)
                foreach (var addressList in existingAddressList)
                {
                    addressList.Comment = "Comment update: " + DateTime.Now.ToShortTimeString();
                }
            }

            //save differences into mikrotik  (existingAddressList=modified, listClonedBackup=unmodified)
            connection.SaveListDifferences(existingAddressList, listClonedBackup);
        }

        private static void DeleteAddressListMulti(ITikConnection connection)
        {
            var existingAddressList = connection.LoadList<AddressList>(
                connection.CreateParameter("list", listName)).ToList();
            var listClonedBackup = existingAddressList.CloneEntityList(); //creates clone of all entities in list

            existingAddressList.Clear();

            //save differences into mikrotik  (existingAddressList=modified, listClonedBackup=unmodified)
            connection.SaveListDifferences(existingAddressList, listClonedBackup);
        }

    }
}
