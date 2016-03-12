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
        bool ReturnsVoid { get; }
    }

    public class Answer<T> : IAnswer
    {
        public SentenceAttribute Attribute { get; private set; }
        public bool IsAsync { get; private set; }
        public Type ReturnType { get { return typeof(T); } }
        public bool ReturnsVoid { get { return false; } }

        protected MethodInfo Method { get; set; }
        protected object[] Parameters { get; set; }
        protected object Invoker { get; set; }

        internal Answer(Answer a)
        {
            IsAsync = a.IsAsync;
            Method = a.Method;
            Parameters = a.Parameters;
            Invoker = a.Invoker;
        }

        public T Call()
        {
            if (IsAsync)
                return CallAsync().GetAwaiter().GetResult();
            else
                return (T)Method.Invoke(Invoker, Parameters);
        }

        public async Task<T> CallAsync()
        {
            if (!IsAsync)
                return Call();
            else
                return await (Task<T>)Method.Invoke(Invoker, Parameters);
        }
    }

    public class Answer : IAnswer
    {
        public SentenceAttribute Attribute { get; private set; }
        public bool IsAsync { get; private set; }
        public Type ReturnType { get; private set; }
        public bool ReturnsVoid { get { return ReturnType == typeof(void); } }

        internal MethodInfo Method { get; set; }
        internal object[] Parameters { get; set; }
        internal object Invoker { get; set; }

        internal Answer(MethodInfo method, object[] parameters, object invoker = null)
        {
            IsAsync = method.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task`");
            ReturnType = IsAsync ? method.ReturnType.GenericTypeArguments[0] : method.ReturnType;
            Method = method;
            Parameters = parameters;
            Invoker = invoker;
        }

        public T Call<T>()
        {
            if (IsAsync)
                return CallAsync<T>().GetAwaiter().GetResult();
            else if (ReturnType != typeof(T))
                throw new InvalidCastException(String.Format("{0} cannot be converted to {1}", Method.ReturnType, typeof(T)));
            else
                return (T)Method.Invoke(Invoker, Parameters);
        }

        public object Call()
        {
            if (IsAsync)
                return CallAsync().GetAwaiter().GetResult();
            else
                return Method.Invoke(Invoker, Parameters);
        }

        public async Task<T> CallAsync<T>()
        {
            if (!IsAsync)
                return Call<T>();
            else if (ReturnType != typeof(T))
                throw new InvalidCastException(String.Format("{0} cannot be converted to {1}", Method.ReturnType, typeof(T)));
            else
                return await (Task<T>)Method.Invoke(Invoker, Parameters);
        }

        public async Task<object> CallAsync()
        {
            if (!IsAsync)
                return Call();
            else
                return await (Task<object>)Method.Invoke(Invoker, Parameters);
        }
    }
}
