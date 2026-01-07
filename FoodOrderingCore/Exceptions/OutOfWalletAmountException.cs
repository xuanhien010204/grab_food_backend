using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodOrderingCore.Exceptions
{
    public class OutOfWalletAmountException : Exception
    {
        public OutOfWalletAmountException() : base() { }
        public OutOfWalletAmountException(string message) : base(message) { }
    }
}
