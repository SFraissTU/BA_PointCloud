using System;
using NUnit.Framework;
using NUnitLite;
using DataStructures;
using System.Collections.Generic;

namespace Tests {

    [TestFixture]
    public class QueueTest {

        public static void Run() {
            new AutoRun().Execute(new String[0]);
            //Console.ReadKey();
        }
        

        [Test]
        public void TestExampleHeapQueue() {
            PriorityQueue<double, int> queue = new HeapPriorityQueue<double, int>();
            int[] elements = new int[] { 4, 9, 8, 7, 3, 22, 4, 9, -20, -40, -100, 102, 4, 5 };
            for (int i = 0; i < elements.Length; i++) {
                queue.Enqueue(elements[i], elements[i]);
            }
            Assert.AreEqual(queue.Count, elements.Length);
            int[] expected = new int[] { 102, 22, 9, 9, 8, 7, 5, 4, 4, 4, 3, -20, -40, -100 };
            for (int i = 0; i < expected.Length; i++) {
                Assert.AreEqual(expected[i], queue.Peek());
                Assert.AreEqual(expected[i], queue.Dequeue());
            }
            Assert.AreEqual(queue.Count, 0);
            Assert.IsTrue(queue.IsEmpty());
        }

        [Test]
        public void TestEmptyHeapQueue() {
            PriorityQueue<double, string> queue = new HeapPriorityQueue<double, string>();
            queue.Enqueue("1", 1);
            queue.Enqueue("2", 2);
            Assert.AreEqual(queue.Dequeue(), "2");
            Assert.AreEqual(queue.Dequeue(), "1");
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
            queue.Enqueue("3", 3);
            Assert.AreEqual(queue.Dequeue(), "3");
        }

        [Test]
        public void TestEnumeratorHeapQueue() {
            PriorityQueue<double, int> queue = new HeapPriorityQueue<double, int>();
            int[] elements = new int[] { 4, 9, 8, 7, 3, 22, 4, 9, -20, -40, -100, 102, 4, 5 };
            for (int i = 0; i < elements.Length; i++) {
                queue.Enqueue(elements[i], elements[i]);
            }
            int[] expected = new int[] { 102, 22, 9, 9, 8, 7, 5, 4, 4, 4, 3, -20, -40, -100 };
            int j = 0;
            foreach (int element in queue) {
                Assert.AreEqual(expected[j], element);
                j++;
            }
        }

        [Test]
        public void TestEnumeratorWhileChangingHeapQueue() {
            PriorityQueue<double, int> queue = new HeapPriorityQueue<double, int>();
            int[] elements = new int[] { 4, 9, 8, 7, 3, 22, 4, 9, -20, -40, -100, 102, 4, 5 };
            for (int i = 0; i < elements.Length; i++) {
                queue.Enqueue(elements[i], elements[i]);
            }
            int[] expected = new int[] { 102, 22, 9, 9, 8, 7, 5, 4, 4, 4, 3, -20, -40, -100 };
            int j = 0;
            foreach (int element in queue) {
                Assert.AreEqual(expected[j], element);
                j++;
                if ((j + 1) % 2 == 0) {
                    queue.Dequeue();
                }
            }
        }

        [Test]
        public void TestEnumeratorWhileChangingEntriesWithSamePriorityHeapQueue() {
            PriorityQueue<int, string> queue = new HeapPriorityQueue<int, string>();
            int[] keys = new int[] { 9, 9, 9, 7, 7 };
            string[] values = new string[] { "9a", "9b", "9c", "7a", "7b" };
            for (int i = 0; i < values.Length; i++) {
                queue.Enqueue(values[i], keys[i]);
            }
            IEnumerator<string> enumerator = queue.GetEnumerator();
            //First moving, then deleting
            enumerator.MoveNext();
            queue.Remove("9a");
            string val1 = enumerator.Current;
            Assert.IsTrue(val1.Equals("9a") || val1.Equals("9b") || val1.Equals("9c"));
            enumerator.MoveNext();
            string val2 = enumerator.Current;
            Assert.IsTrue(val1.Equals("9b") || val2.Equals("9b") || val2.Equals("9c"));
            Assert.AreNotEqual(val2, val1);
            enumerator.MoveNext();
            //First deleting, then moving
            queue.Remove("7a");
            enumerator.MoveNext();
            val1 = enumerator.Current;
            enumerator.MoveNext();
            val2 = enumerator.Current;
            Assert.IsTrue(val1.Equals("7a") || val1.Equals("7b"));
            Assert.IsTrue(val2.Equals("7a") || val2.Equals("7b"));
            Assert.AreNotEqual(val1, val2);
        }

        [Test]
        public void TestSamePrioritiesHeapQueue() {
            PriorityQueue<int, string> queue = new HeapPriorityQueue<int, string>();
            int[] keys = { 1, 3, 5, 3, 4, 3, 2, 4 };
            string[] vals = { "1", "3a", "5", "3b", "4a", "3c", "2", "4b" };
            for (int i = 0; i < keys.Length; i++) {
                queue.Enqueue(vals[i], keys[i]);
            }
            Assert.AreEqual(queue.Dequeue(), "5");
            string first4 = queue.Dequeue();
            Assert.IsTrue(first4.StartsWith("4"));
            string second4 = queue.Dequeue();
            Assert.IsTrue(second4.StartsWith("4"));
            Assert.AreNotEqual(first4, second4);
            string first3 = queue.Dequeue();
            Assert.IsTrue(first3.StartsWith("3"));
            string second3 = queue.Dequeue();
            Assert.IsTrue(second3.StartsWith("3"));
            string third3 = queue.Dequeue();
            Assert.IsTrue(third3.StartsWith("3"));
            Assert.AreEqual(queue.Dequeue(), "2");
            Assert.AreEqual(queue.Dequeue(), "1");
        }

        [Test]
        public void TestRemoveWithPriorityHeapQueue() {
            PriorityQueue<int, int> queue = new HeapPriorityQueue<int, int>();
            int[] keys = new int[] { 4, 9, 8, 7, 3, 22, 4, 9, -20, -40, -100, 102, 4, 5 };
            int[] values = new int[] { 4, 9, 8, 69, 3, 22, 9, 9, -20, -40, 3, 102, 4, 5 };
            for (int i = 0; i < keys.Length; i++) {
                queue.Enqueue(values[i], keys[i]);
            }
            queue.Remove(22, 22);
            queue.Remove(4, 4);
            queue.Remove(9, 4);
            queue.Remove(69, 7);
            queue.Remove(3, -100); //<- Doesn't exist
            queue.Remove(102, 102);
            int[] expected = new int[] { 9, 9, 8, 5, 4, 3, -20, -40 };
            foreach (int el in expected) {
                Assert.AreEqual(el, queue.Dequeue());
            }
        }

        [Test]
        public void TestRemoveWithoutPriorityHeapQueue() {
            PriorityQueue<int, int> queue = new HeapPriorityQueue<int, int>();
            int[] keys = new int[] { 4, 9, 8, 7, 3, 22, 4, 9, -20, -40, -100, 102, 4, 5 };
            int[] values = new int[] { 4, 9, 8, 7, 3, 22, 4, 9, -20, -40, -100, 102, 0, 5 };
            for (int i = 0; i < keys.Length; i++) {
                queue.Enqueue(values[i], keys[i]);
            }
            queue.Remove(22);
            queue.Remove(0);
            queue.Remove(9);
            queue.Remove(9);
            queue.Remove(6); //<- Doesn't exist
            queue.Remove(102);
            int[] expected = new int[] { 8, 7, 5, 4, 4, 3, -20, -40, -100 };
            foreach (int el in expected) {
                Assert.AreEqual(el, queue.Dequeue());
            }
        }

    }
}