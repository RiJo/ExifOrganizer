using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserInterface
{
    public class CLI
    {
        public CLI()
        {
            Console.WriteLine("ExifOrganizer CLI");
        }

        public void Run(string[] args)
        {
            foreach (string arg in args)
                Console.WriteLine(String.Format(" * {0}", arg));
        } 
    }
}
