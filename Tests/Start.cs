using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests {
    class Start {
        public static void Main(string[] args) {
            Console.WriteLine("Choose Test:");
            Console.WriteLine("  1: Queue Unit Tests");
            Console.WriteLine("  2: Queue Speed Tests For Dequeuing");
            Console.WriteLine("  3: Queue Speed Tests For Removing");
            Console.WriteLine("  0: Exit");
            while (true) {
                Console.Write(">> ");
                char cin = Console.ReadKey().KeyChar;
                Console.WriteLine();
                if (cin == '1') {
                    QueueTest.Run();
                } else if (cin == '2') {
                    QueueSpeedTest.RunDeq();
                } else if (cin == '3') {
                    QueueSpeedTest.RunRem();
                } else if (cin == '0') {
                    return;
                }
            }
        }
    }
}
