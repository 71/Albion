using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    public interface IAnswer
    {
        SentenceAttribute Attribute { get; }
        bool IsAsync { get; }
        Type ReturnType { get; }
        Type ObjectType { get; }
    }

    public class Answer<T> : IAnswer
    {
        public SentenceAttribute Attribute { get; private set; }
        public bool IsAsync { get; private set; }
        public Type ReturnType { get { return typeof(T); } }
        public Type ObjectType { get; private set; }

        protected MethodInfo Method { get; set; }
        protected object[] Parameters { get; set; }
        
        internal Func<dynamic, T> Handler { get; set; }

        internal Answer(Answer a)
        {
            IsAsync = a.IsAsync;
            Method = a.Method;
            Parameters = a.Parameters;
            ObjectType = a.ObjectType;
            Handler = a.Handler_2 as Func<dynamic, T>;
        }
        
        public T Call(object invoker)
        {
            if (Handler != null)
                return Handler(Parameters[0]);
            else if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            else if (IsAsync)
                return CallAsync(invoker).GetAwaiter().GetResult();
            else
                return (T)Method.Invoke(invoker, Parameters);
        }

        public async Task<T> CallAsync(object invoker)
        {
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            else if (!IsAsync)
                return Call(invoker);
            else
                return await (Task<T>)Method.Invoke(invoker, Parameters);
        }

        public T Call()
        {
            return Call(null);
        }

        public async Task<T> CallAsync()
        {
            return await CallAsync(null);
        }

    }

    public class Answer : IAnswer
    {
        public SentenceAttribute Attribute { get; private set; }
        public bool IsAsync { get; private set; }
        public Type ReturnType { get; private set; }
        public Type ObjectType { get; private set; }

        internal MethodInfo Method { get; set; }
        internal object[] Parameters { get; set; }

        internal Action<dynamic> Handler_1 { get; set; }
        internal Func<dynamic, object> Handler_2 { get; set; }

        internal Answer(Action<dynamic> handler, object parameter)
        {
            var method = handler.GetMethodInfo();

            IsAsync = false;
            ReturnType = typeof(void);
            Method = method;
            Parameters = new object[] { parameter };
            ObjectType = null;
            Handler_1 = handler;
        }

        internal Answer(Func<dynamic, object> handler, object parameter)
        {
            var method = handler.GetMethodInfo();
            IsAsync = false;
            ReturnType = method.ReturnType;
            Method = method;
            Parameters = new object[] { parameter };
            ObjectType = null;
            Handler_2 = handler;
        }

        internal Answer(MethodInfo method, object[] parameters)
        {
            IsAsync = method.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task`1");
            ReturnType = IsAsync ? method.ReturnType.GenericTypeArguments[0] : method.ReturnType;
            Method = method;
            Parameters = parameters;
            ObjectType = method.IsStatic ? null : method.DeclaringType;
        }

        public T Call<T>()
        {
            return Call<T>(null);
        }

        public object Call()
        {
            return Call(null);
        }

        public async Task<T> CallAsync<T>()
        {
            return await CallAsync<T>(null);
        }

        public async Task<object> CallAsync()
        {
            return await CallAsync(null);
        }

        public T Call<T>(object invoker)
        {
            if (Handler_2 != null && ReturnType == typeof(T))
                return (T)Handler_2(Parameters[0]);
            else if (Handler_1 != null || ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            else if (IsAsync)
                return CallAsync<T>(invoker).GetAwaiter().GetResult();
            else if (ReturnType != typeof(T))
                throw new InvalidCastException(String.Format("{0} cannot be converted to {1}", Method.ReturnType, typeof(T)));
            else
                return (T)Method.Invoke(invoker, Parameters);
        }

        public object Call(object invoker)
        {
            if (Handler_1 != null)
            {
                Handler_1(Parameters[0]);
                return null;
            }
            else if (Handler_2 != null)
                return Handler_2(Parameters[0]);
            else if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            else if (IsAsync)
                return CallAsync(invoker).GetAwaiter().GetResult();
            else
                return Method.Invoke(invoker, Parameters);
        }

        public async Task<T> CallAsync<T>(object invoker)
        {
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            else if (!IsAsync)
                return Call<T>(invoker);
            else if (ReturnType != typeof(T))
                throw new InvalidCastException(String.Format("{0} cannot be converted to {1}", Method.ReturnType, typeof(T)));
            else
                return await (Task<T>)Method.Invoke(invoker, Parameters);
        }

        public async Task<object> CallAsync(object invoker)
        {
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            else if (!IsAsync)
                return Call(invoker);
            else
                return await (Task<object>)Method.Invoke(invoker, Parameters);
        }
    }
}
