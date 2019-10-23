using System;
using System.Collections.Generic;
using System.Text;

namespace MicroRabbitMQ.Banking.Domain.Models
{
    public class AccountTransfer
    {
        public int FromAccount { get; set; }
        public int ToAccount { get; set; }
        public decimal TransferAmount { get; set; }
    }
}
