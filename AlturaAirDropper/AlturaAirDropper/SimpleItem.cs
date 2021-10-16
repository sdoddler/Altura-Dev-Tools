using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlturaAirDropper
{
   public class SimpleItem
    {
        public string name;
        public string itemCollectionName;
      public  int tokenId;
      public  Holder[] holders;
    }

    public class Holder
    {
        public string address;
        public int balance;
    }
}
