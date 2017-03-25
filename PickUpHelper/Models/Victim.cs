using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catel.Data;

namespace PickUpHelper.Models
{
    public class Victim
    {
        public string Firstname;
        public string Lastname;
        public string Uid;

        public Victim(string firstname, string lastname, string uid)
        {
            Firstname = firstname;
            Lastname = lastname;
            Uid = uid;
        }
    }
}
