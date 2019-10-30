using MicroRabbitMQ.Transfer.Data.Context;
using MicroRabbitMQ.Transfer.Domain.Interfaces;
using MicroRabbitMQ.Transfer.Domain.Models;
using System.Collections.Generic;

namespace MicroRabbitMQ.Transfer.Data.Repository
{
    public class TransferRepository : ITransferRepository
    {

        private readonly TransferDbContext _ctx;
        public TransferRepository(TransferDbContext ctx)
        {
            _ctx = ctx;
        }
        
        public void Add(TransferLog transferLog)
        {
            _ctx.TransferLogs.Add(transferLog);
            _ctx.SaveChanges();
        }

        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return _ctx.TransferLogs;
        }
    }
}
