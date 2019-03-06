using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicSenActAppCoreWebApi
{
    public class StateMachine
    {
        private int i;

        public int I { get { return i; } }
        
        public StateMachine()
        {
            i = 1;
        }

        public void IncI()
        {
            if (++i > 30)
                i /= 2;
        }
    }
}
