﻿using Database.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class DbClient
    {
        private readonly IMongoCollection<Publication> _publications;
        private readonly IMongoCollection<Account> _accounts;
        public DbClient(IOptions<DbConfig> publicationsDbConfig)
        {
            var client = new MongoClient(publicationsDbConfig.Value.Connection_String);
            var database = client.GetDatabase(publicationsDbConfig.Value.Database_Name);
            _publications = database.GetCollection<Publication>(publicationsDbConfig.Value.Publications_Collection_Name);
            _accounts = database.GetCollection<Account>(publicationsDbConfig.Value.Accounts_Collection_Name);
        }
        public IMongoCollection<Publication> GetPubsCollection()
        {
            return _publications;
        }
        public IMongoCollection<Account> GetAccountCollection()
        {
            return _accounts;
        }
    }
}
