using MicroRabbitMQ.Banking.Data.Context;
using MicroRabbitMQ.Banking.Domain.Interfaces;
using MicroRabbitMQ.Banking.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbitMQ.Banking.Data.Repository
{
    public class AccountRepository : IAccountRepository
    {

        private readonly BankingDbContext _ctx;
        public AccountRepository(BankingDbContext ctx)
        {
            _ctx = ctx;
        }
        
        public IEnumerable<Account> GetAccounts()
        {
            return _ctx.Accounts;
        }
    }
}
