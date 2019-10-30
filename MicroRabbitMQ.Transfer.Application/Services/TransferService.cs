using MicroRabbitMQ.Transfer.Application.Interfaces;
using MicroRabbitMQ.Transfer.Domain.Models;
using System.Collections.Generic;
using MicroRabbitMQ.Transfer.Domain.Interfaces;

namespace MicroRabbitMQ.Transfer.Application.Services
{
    public class TransferService : ITransferService
    {
        private readonly ITransferRepository _transferRepository;
        public TransferService(ITransferRepository transferRepository)
        {
            _transferRepository = transferRepository;
        }        

        public IEnumerable<TransferLog> GetTransferLogs()
        {
            return _transferRepository.GetTransferLogs();
        }
        
    }
}
