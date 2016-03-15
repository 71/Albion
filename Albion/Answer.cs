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

        internal Answer(Answer a)
        {
            IsAsync = a.IsAsync;
            Method = a.Method;
            Parameters = a.Parameters;
            ObjectType = a.ObjectType;
        }

        public T Call(object invoker)
        {
            if (ObjectType != null && invoker?.GetType() != ObjectType)
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
            if (ObjectType != null && invoker?.GetType() != ObjectType)
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
            if (ObjectType != null && invoker?.GetType() != ObjectType)
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
