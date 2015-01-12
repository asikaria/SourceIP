using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SourceIPService;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            Service s = new Service();
            s.DoStart();

            Console.ReadLine();
            s.DoStop();
        }
    }
}
