using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeAuth.Models
{
    class User
    {
        public string Name;
        public string BadgeCIN;
        public long Balance;

        public long NumBags;
        public long NumMilks;
        public long NumShots;
        public long NumLogins;

        public string PictureUrl;

        public override string ToString()
        {
            return Name + "\t\t" + Balance + "$"; 
        }
    }
}
