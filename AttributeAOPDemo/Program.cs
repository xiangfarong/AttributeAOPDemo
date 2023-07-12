using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttributeAOPDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TestA t = new TestA();
            t.TestAsyncFunc("这是一个用 Attribute 特性 实现 AOP 实例测试 ");
            Console.ReadLine();
        }
    }
}
