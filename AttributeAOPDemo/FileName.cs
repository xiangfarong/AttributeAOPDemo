using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttributeAOPDemo
{
    /// <summary>
    /// （使用示例）需要拦截插入代码的类，要绑定ContextBoundObject 上下文基类
    /// </summary>
    [AOPContext]
    public class TestA : ContextBoundObject
    {
        [AOPMethod]
        public string TestAsyncFunc(string param)
        {
            Thread.Sleep(1000);
            Console.WriteLine("DoSometing");
            return "complete";
        }
    }


    /// <summary>
    /// 定义一个类特性，继承 ContextAttribute 特性、实现 IContributeObjectSink拦截接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AOPContextAttribute : ContextAttribute, IContributeObjectSink
    {

        public AOPContextAttribute() : base("AOPContext")
        {
        }

        /// <summary>
        /// 重写拦截处理
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="nextSink"></param>
        /// <returns></returns>
        public IMessageSink GetObjectSink(MarshalByRefObject obj, IMessageSink nextSink)
        {
            return new AOPHandler(nextSink);
        }
    }

    /// <summary>
    /// 定义一个方法特性，用于区分该方法是否拦截，所以没有内容可以
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AOPMethodAttribute : Attribute
    {
    }

    /// <summary>
    /// 实现消息接收器接口
    /// </summary>
    public class AOPHandler : IMessageSink
    {
        /// <summary>
        /// 下一个消息接收器
        /// </summary>
        private readonly IMessageSink _nextSink;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AOPHandler(IMessageSink nextSink)
        {
            _nextSink = nextSink;
        }

        public IMessageSink NextSink
        {
            get { return _nextSink; }
        }

        /// <summary>
        /// 同步处理方法  
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public IMessage SyncProcessMessage(IMessage msg)
        {
            IMessage message = null;
            IMethodCallMessage callMessage = msg as IMethodCallMessage;
            // 判断方法是否拦截（判断方法有没有上面定义的 AOPMethodAttribute 特性）
            if (callMessage != null && (Attribute.GetCustomAttribute(callMessage.MethodBase, typeof(AOPMethodAttribute))) != null)
            {
                // 执行前
                PreProceed(msg);
                // invoke 执行原方法
                message = _nextSink.SyncProcessMessage(msg);
                // 执行后
                PostProceed(message);
            }
            else
            {
                message = _nextSink.SyncProcessMessage(msg);
            }
            return message;
        }

        /// <summary>
        /// 异步处理方法
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="replySink"></param>
        /// <returns></returns>
        public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            return null;
        }

        /// <summary>
        /// 方法执行前
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="before"></param>
        public void PreProceed(IMessage msg)
        {
            var message = msg as IMethodMessage;
            var typeName = message.TypeName.Split(',')[0];
            var methodName = message.MethodName;
            // 获取到的参数列表 object[]
            var paramss = message.Args;
            Console.WriteLine($"{typeName}类{methodName}方法执行前，入参为：{string.Join(",", paramss.ToArray())}");
            //System.Diagnostics.Debug.WriteLine($"{typeName}类{methodName}方法执行前，入参为：{string.Join(",", paramss.ToArray())}");//MVC调试的输出
        }

        /// <summary>
        /// 方法执行后
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="after"></param>
        public void PostProceed(IMessage msg)
        {
            var message = msg as IMethodReturnMessage;
            var typeName = message.TypeName.Split(',')[0];
            var methodName = message.MethodName;
            // 获取到的返回值
            var param = message.ReturnValue;
            Console.WriteLine($"{typeName}类{methodName}方法执行后，返参为：{param.ToString()}");
            //System.Diagnostics.Debug.WriteLine($"{typeName}类{methodName}方法执行后，返参为：{param.ToString()}");//MVC调试的输出
        }
    }
}
