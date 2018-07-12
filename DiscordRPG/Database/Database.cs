using DiscordRPG.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPG.Database
{
    public class Database
    {
        /// <summary>
        /// The path of the file
        /// </summary>
        public string DatabaseFile { get; set; }

        /// <summary>
        /// The stored users
        /// </summary>
        public List<User> Users { get; private set; }

        /// <summary>
        /// The constructor of the Database
        /// </summary>
        /// <param name="DatabaseFile"></param>
        public Database(string DatabaseFile)
        {
            this.DatabaseFile = DatabaseFile;
            Users = new List<User>();
            Load();
        }

        /// <summary>
        /// Ads user to the stored ones
        /// </summary>
        /// <param name="User">The User object</param>
        public bool AddUser(User User)
        {
            if (Users.FindIndex(t => t.UserId == User.UserId) >= 0)
                return false;

            Users.Add(User);
            return true;
        }

        /// <summary>
        /// Updates a user in the database
        /// </summary>
        /// <param name="User">The user to update</param>
        public void UpdateUser(User User)
        {
            Users[Users.FindIndex(t => t.UserId == User.UserId)] = User;
            Save();
        }

        /// <summary>
        /// Removes a user by user Id
        /// </summary>
        /// <param name="UserId">The Id of the user</param>
        public bool RemoveUser(ulong UserId)
        {
            int Count = Users.RemoveAll(t => t.UserId == UserId);

            return Count > 0;
        }

        /// <summary>
        /// Check if the database has a specified user
        /// </summary>
        /// <param name="UserId">The id of the user</param>
        /// <returns>Returns true if user found, otherwise false</returns>
        public bool HasUser(ulong UserId)
        {
            int Index = Users.FindIndex(t => t.UserId == UserId);
            if (Index >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Scans the database for a specified user
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns>Returns the user if found, otherwise null</returns>
        public User FindUser(ulong UserId)
        {
            int Index = Users.FindIndex(t => t.UserId == UserId);
            if (Index >= 0)
                return Users[Index];
            else
                return null;
        }
        
        /// <summary>
        /// Saves the database
        /// </summary>
        public void Save()
        {
            if (File.Exists(DatabaseFile))
                File.Delete(DatabaseFile);

            File.WriteAllText(DatabaseFile, JsonConvert.SerializeObject(Users, Formatting.Indented));
        }

        /// <summary>
        /// Loads the database
        /// </summary>
        public void Load()
        {
            if (File.Exists(DatabaseFile))
            {
                Users.AddRange(JsonConvert.DeserializeObject<User[]>(File.ReadAllText(DatabaseFile)));
            }
        }
    }
}
