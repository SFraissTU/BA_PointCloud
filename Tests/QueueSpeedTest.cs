using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;

namespace Tests {
    class QueueSpeedTest {

        private static int SEED = 123;

        public static void RunDeq() {
            List<QueueCommand<int, int>> enqueueCommands = CreateCommandsEnqueuing(1000000);
            List<QueueCommand<int, int>> dequeueCommands = CreateCommandsDequeuing(1000000);
            List<QueueCommand<int, int>> enqdequCommands = CreateCommandsEnqueDequeue(1000000);
            PriorityQueue<int, int> dictQueue = new DictionaryPriorityQueue<int, int>();
            PriorityQueue<int, int> listQueue = new ListPriorityQueue<int, int>();
            PriorityQueue<int, int> heapQueue = new HeapPriorityQueue<int, int>();
            Console.WriteLine("Enqueuing Test:");
            Console.WriteLine("  DictQueue: " + DoCommands(dictQueue, enqueueCommands) + "ms");
            Console.WriteLine("  ListQueue: " + DoCommands(listQueue, enqueueCommands) + "ms");
            Console.WriteLine("  HeapQueue: " + DoCommands(heapQueue, enqueueCommands) + "ms");
            Console.WriteLine("Seq Dequeuing Test:");
            Console.WriteLine("  DictQueue: " + DoCommands(dictQueue, dequeueCommands) + "ms");
            Console.WriteLine("  ListQueue: " + DoCommands(listQueue, dequeueCommands) + "ms");
            Console.WriteLine("  HeapQueue: " + DoCommands(heapQueue, dequeueCommands) + "ms");
            Console.WriteLine("Enqueuing and Dequeuing Test:");
            Console.WriteLine("  DictQueue: " + DoCommands(dictQueue, enqdequCommands) + "ms");
            Console.WriteLine("  ListQueue: " + DoCommands(listQueue, enqdequCommands) + "ms");
            Console.WriteLine("  HeapQueue: " + DoCommands(heapQueue, enqdequCommands) + "ms");
            //Console.ReadKey();
        }

        public static void RunRem() {
            List<QueueCommand<int, int>> enqueueCommands = CreateCommandsEnqueuing(1000000);
            List<QueueCommand<int, int>> removeCommands = CreateCommandsRemoveByElement(enqueueCommands, 1000);
            PriorityQueue<int, int> dictQueue = new DictionaryPriorityQueue<int, int>();
            PriorityQueue<int, int> listQueue = new ListPriorityQueue<int, int>();
            PriorityQueue<int, int> heapQueue = new HeapPriorityQueue<int, int>();
            Console.WriteLine("Enqueuing Test:");
            Console.WriteLine("  DictQueue: " + DoCommands(dictQueue, enqueueCommands) + "ms");
            Console.WriteLine("  ListQueue: " + DoCommands(listQueue, enqueueCommands) + "ms");
            Console.WriteLine("  HeapQueue: " + DoCommands(heapQueue, enqueueCommands) + "ms");
            Console.WriteLine("Removing Test:");
            Console.WriteLine("  DictQueue: " + DoCommands(dictQueue, removeCommands) + "ms");
            Console.WriteLine("  ListQueue: " + DoCommands(listQueue, removeCommands) + "ms");
            Console.WriteLine("  HeapQueue: " + DoCommands(heapQueue, removeCommands) + "ms");
        }

        private static List<QueueCommand<int,int>> CreateCommandsEnqueuing(int size) {
            List<QueueCommand<int, int>> list = new List<QueueCommand<int, int>>();
            Random r = new Random(SEED);
            for (int i = 0; i < size; i++) {
                list.Add(new QueueCommand<int, int>(CommandType.ENQUEUE, new KeyValuePair<int, int>(r.Next(), r.Next())));
            }
            return list;
        }

        private static List<QueueCommand<int,int>> CreateCommandsDequeuing(int size) {
            List<QueueCommand<int, int>> list = new List<QueueCommand<int, int>>();
            for (int i = 0; i < size; i++) {
                list.Add(new QueueCommand<int, int>(CommandType.DEQUEUE));
            }
            return list;
        }

        private static List<QueueCommand<int,int>> CreateCommandsEnqueDequeue(int size) {
            List<QueueCommand<int, int>> list = new List<QueueCommand<int, int>>();
            Random r = new Random(SEED);
            int qcount = 0;
            for (int i = 0; i < size; i++) {
                if (qcount == 0 || r.NextDouble() < 0.7) {
                    list.Add(new QueueCommand<int, int>(CommandType.ENQUEUE, new KeyValuePair<int, int>(r.Next(), r.Next())));
                    ++qcount;
                } else {
                    list.Add(new QueueCommand<int, int>(CommandType.DEQUEUE));
                    --qcount;
                }
            }
            return list;
        }

        private static List<QueueCommand<int,int>> CreateCommandsRemoveByElement(List<QueueCommand<int, int>> enqueues, int skip) {
            List<QueueCommand<int, int>> list = new List<QueueCommand<int, int>>();
            int i = 1;
            foreach (QueueCommand<int,int> c in enqueues) {
                if (i%skip == 0) {
                    list.Add(new QueueCommand<int, int>(CommandType.REMOVE_BY_ELEMENT, new KeyValuePair<int, int>(c.entry.Key, c.entry.Value)));
                }
                ++i;
            }
            //Shuffle
            Random r = new Random();
            int n = list.Count;
            while (n > 1) {
                int k = r.Next(0, n) % n;
                n--;
                QueueCommand<int, int> val = list[k];
                list[k] = list[n];
                list[n] = val;
            }
            return list;
        }

        private static int DoCommands<I, T> (PriorityQueue<I,T> queue, List<QueueCommand<I,T>> commands) where I : IComparable<I>{
            int start = Environment.TickCount;
            foreach (QueueCommand<I, T> command in commands) {
                switch (command.type) {
                    case CommandType.ENQUEUE:
                        queue.Enqueue(command.entry.Value, command.entry.Key);
                        break;
                    case CommandType.DEQUEUE:
                        queue.Dequeue();
                        break;
                    case CommandType.REMOVE_BY_ELEMENT:
                        queue.Remove(command.entry.Value);
                        break;
                }
                if (Environment.TickCount - start > 10000) {
                    return -1;
                }
            }
            return Environment.TickCount - start;
        }


        class QueueCommand<I,T> {
            public CommandType type;
            public KeyValuePair<I, T> entry;

            public QueueCommand(CommandType type) {
                this.type = type;
            }

            public QueueCommand(CommandType type, KeyValuePair<I,T> entry) {
                this.type = type;
                this.entry = entry;
            }
        }

        enum CommandType {
            ENQUEUE,
            DEQUEUE,
            REMOVE_BY_ELEMENT
        }
    }
}
