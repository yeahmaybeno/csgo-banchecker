using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yes
{
    class AccountDetails
    {
        public string login = string.Empty;
        public bool banned = false;
        public int penalty_reason = -1;
        public int penalty_seconds = -1;
        public int wins = -1;
        public int rank = -1;
    }
}
