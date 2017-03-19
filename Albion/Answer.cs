using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Albion
{
    /// <summary>
    /// Represents a generic answer returned by <see cref="Engine.Ask(string)"/>.
    /// </summary>
    public class Answer<T>
    {
        internal readonly Engine owner;
        internal readonly object target;
        internal readonly object[] arguments;

        /// <summary>
        /// Gets the method that will be called to get a result.
        /// </summary>
        public MethodInfo Method { get; }

        /// <summary>
        /// The description provided by <see cref="SentenceAttribute.Description"/> or <see cref="SentenceBuilder.Description(string)"/>.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The language of the matched sentence.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// The ID provided by <see cref="SentenceAttribute.ID"/> or <see cref="SentenceBuilder.ID(string)"/>.
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// Whether this method should be called asynchronously.
        /// </summary>
        public bool IsAsync { get; }

        /// <summary>
        /// The <see cref="Type"/> of the object returned by <see cref="Call()"/>.
        /// </summary>
        public Type ReturnType => IsAsync ? Method.ReturnType.GenericTypeArguments[0] : Method.ReturnType;

        /// <summary>
        /// The <see cref="Type"/> of the class which defined the method to call.
        /// </summary>
        public Type ObjectType => Method.IsStatic ? null : Method.DeclaringType;


        internal Answer(Engine engine, string description, string language, string id, MethodInfo method, object target, params object[] args)
        {
            Description = description;
            Language = language;
            ID = id;

            IsAsync = typeof(Task).GetTypeInfo().IsAssignableFrom(method.ReturnType.GetTypeInfo());
            Method = method;

            this.owner = engine;
            this.target = target;
            this.arguments = args;

            if (target == null && !method.IsStatic)
                this.target = engine.InstanceFor(ObjectType);
        }

        /// <summary>
        /// Call the method, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public T Call() => Call(target);

        /// <summary>
        /// Call the method asynchronously, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public async Task<T> CallAsync() => await CallAsync(target);

        /// <summary>
        /// Call the method, and cast the result to T.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <exception cref="InvalidCastException"></exception>
        public T Call(object invoker) => IsAsync
            ? CallAsync(invoker).GetAwaiter().GetResult()
            : (T)Method.Invoke(invoker, arguments);

        /// <summary>
        /// Call the method asynchronously, and cast the result to T.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <exception cref="InvalidCastException"></exception>
        public async Task<T> CallAsync(object invoker) => IsAsync
            ? await (Task<T>)Method.Invoke(invoker, arguments)
            : Call(invoker);
    }

    /// <summary>
    /// Represents an answer returned by <see cref="Engine.Ask(string)"/>.
    /// </summary>
    public class Answer : Answer<object>
    {
        internal Answer(Engine engine, string description, string language, string id, MethodInfo method, object target, params object[] args)
            : base(engine, description, language, id, method, target, args)
        {
        }

        /// <summary>
        /// Call the method, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public T Call<T>() => Call<T>(owner?.InstanceFor(ObjectType));

        /// <summary>
        /// Call the method asynchronously, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public async Task<T> CallAsync<T>() => await CallAsync<T>(owner?.InstanceFor(ObjectType));

        /// <summary>
        /// Call the method, and cast the result to T.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <exception cref="InvalidCastException"></exception>
        public T Call<T>(object invoker) => IsAsync
            ? CallAsync<T>(invoker).GetAwaiter().GetResult()
            : (T)Method.Invoke(invoker, arguments);

        /// <summary>
        /// Call the method asynchronously, and cast the result to T.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <exception cref="InvalidCastException"></exception>
        public async Task<T> CallAsync<T>(object invoker) => IsAsync
            ? await (Task<T>)Method.Invoke(invoker, arguments)
            : Call<T>(invoker);
    }

#if false
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Answer<T>
    {
        /// <summary>
        /// The description provided by <see cref="SentenceAttribute.Description"/> or <see cref="SentenceBuilder.Description(string)"/>.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// The language of the matched sentence.
        /// </summary>
        public string Language { get; private set; }
        /// <summary>
        /// The ID provided by <see cref="SentenceAttribute.ID"/> or <see cref="SentenceBuilder.ID(string)"/>.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Whether this method should be called asynchronously.
        /// </summary>
        public bool IsAsync { get; private set; }
        /// <summary>
        /// The <see cref="Type"/> of the object returned by <see cref="Call()"/>.
        /// </summary>
        public Type ReturnType { get { return typeof(T); } }
        /// <summary>
        /// The <see cref="Type"/> of the class which defined the method to call.
        /// </summary>
        public Type ObjectType { get; private set; }

        internal MethodInfo Method { get; set; }
        internal object[] Parameters { get; set; }

        internal Func<dynamic, object> Handler { get; set; }

        internal Engine Owner { get; set; }

        internal Answer(Answer a)
        {
            Language = a.Language;
            Description = a.Description;
            ID = a.ID;

            IsAsync = a.IsAsync;
            Method = a.Method;
            Parameters = a.Parameters;
            ObjectType = a.ObjectType;
            Handler = a.Handler_2;
            Owner = a.Owner;
        }

        /// <summary>
        /// Call the method, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public T Call()
        {
            if (Owner == null)
                return Call(null);
            return Call(Owner.InstanceFor(ObjectType));
        }

        /// <summary>
        /// Call the method asynchronously, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public async Task<T> CallAsync()
        {
            if (Owner == null)
                return await CallAsync(null);
            return await CallAsync(Owner.InstanceFor(ObjectType));
        }

        /// <summary>
        /// Call the method.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <returns></returns>
        public T Call(object invoker)
        {
            if (Handler != null)
                return (T)Handler(Parameters[0]);
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            if (IsAsync)
                return CallAsync(invoker).GetAwaiter().GetResult();
            return (T)Method.Invoke(invoker, Parameters);
        }

        /// <summary>
        /// Call the method asynchronously.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <returns></returns>
        public async Task<T> CallAsync(object invoker)
        {
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            if (!IsAsync)
                return Call(invoker);
            return await (Task<T>)Method.Invoke(invoker, Parameters);
        }
    }

    /// <summary>
    /// An Answer generated by <see cref="Engine.Ask(string)"/>.
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// The description provided by <see cref="SentenceAttribute.Description"/> or <see cref="SentenceBuilder.Description(string)"/>.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// The language of the matched sentence.
        /// </summary>
        public string Language { get; private set; }
        /// <summary>
        /// The ID provided by <see cref="SentenceAttribute.ID"/> or <see cref="SentenceBuilder.ID(string)"/>.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Whether this method should be called asynchronously.
        /// </summary>
        public bool IsAsync { get; private set; }
        /// <summary>
        /// The <see cref="Type"/> of the object returned by <see cref="Call()"/>.
        /// </summary>
        public Type ReturnType { get; private set; }
        /// <summary>
        /// The <see cref="Type"/> of the class which defined the method to call.
        /// </summary>
        public Type ObjectType { get; private set; }

        internal MethodInfo Method { get; set; }
        internal object[] Parameters { get; set; }

        internal Action<dynamic> Handler_1 { get; set; }
        internal Func<dynamic, object> Handler_2 { get; set; }

        internal Engine Owner { get; set; }

        internal Answer(Action<dynamic> handler, object parameter, string lang, string descr, string id, Engine en)
        {
            var method = handler.GetMethodInfo();

            Description = descr;
            ID = id;
            Language = lang;

            IsAsync = false;
            ReturnType = typeof(void);
            Method = method;
            Parameters = new[] { parameter };
            ObjectType = null;
            Handler_1 = handler;
            Owner = en;
        }

        internal Answer(Func<dynamic, object> handler, object parameter, string lang, string descr, string id, Engine en)
        {
            var method = handler.GetMethodInfo();

            Description = descr;
            ID = id;
            Language = lang;

            IsAsync = false;
            ReturnType = method.ReturnType;
            Method = method;
            Parameters = new[] { parameter };
            ObjectType = null;
            Handler_2 = handler;
            Owner = en;
        }

        internal Answer(MethodInfo method, object[] parameters, string lang, string descr, string id, Engine en)
        {
            Description = descr;
            ID = id;
            Language = lang;

            IsAsync = typeof(Task<>).GetTypeInfo().IsAssignableFrom(method.ReturnType.GetGenericTypeDefinition().GetTypeInfo());
            ReturnType = IsAsync ? method.ReturnType.GenericTypeArguments[0] : method.ReturnType;
            Method = method;
            Parameters = parameters;
            ObjectType = method.IsStatic ? null : method.DeclaringType;
            Owner = en;
        }

        /// <summary>
        /// Call the method, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public object Call()
        {
            if (Owner == null)
                return Call(null);
            return Call(Owner.InstanceFor(ObjectType));
        }

        /// <summary>
        /// Call the method asynchronously, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public async Task<object> CallAsync()
        {
            if (Owner == null)
                return await CallAsync(null);
            return await CallAsync(Owner.InstanceFor(ObjectType));
        }

        /// <summary>
        /// Call the method, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public T Call<T>()
        {
            if (Owner == null)
                return Call<T>(null);
            return Call<T>(Owner.InstanceFor(ObjectType));
        }

        /// <summary>
        /// Call the method asynchronously, trying to automagically resolve
        /// its instance if it isn't static.
        /// </summary>
        public async Task<T> CallAsync<T>()
        {
            if (Owner == null)
                return await CallAsync<T>(null);
            return await CallAsync<T>(Owner.InstanceFor(ObjectType));
        }

        /// <summary>
        /// Call the method, and cast the result to T.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public T Call<T>(object invoker)
        {
            if (Handler_2 != null && ReturnType == typeof(T))
                return (T)Handler_2(Parameters[0]);
            if (Handler_1 != null || ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            if (IsAsync)
                return CallAsync<T>(invoker).GetAwaiter().GetResult();
            if (ReturnType != typeof(T))
                throw new InvalidCastException($"{Method.ReturnType} cannot be converted to {typeof(T)}");
            return (T)Method.Invoke(invoker, Parameters);
        }

        /// <summary>
        /// Call the method.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public object Call(object invoker)
        {
            if (Handler_1 != null)
            {
                Handler_1(Parameters[0]);
                return null;
            }
            if (Handler_2 != null)
                return Handler_2(Parameters[0]);
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            if (IsAsync)
                return CallAsync(invoker).GetAwaiter().GetResult();
            return Method.Invoke(invoker, Parameters);
        }

        /// <summary>
        /// Call the method asynchronously, and cast the result to T.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public async Task<T> CallAsync<T>(object invoker)
        {
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            if (!IsAsync)
                return Call<T>(invoker);
            if (ReturnType != typeof(T))
                throw new InvalidCastException($"{Method.ReturnType} cannot be converted to {typeof(T)}");
            return await (Task<T>)Method.Invoke(invoker, Parameters);
        }

        /// <summary>
        /// Call the method asynchronously.
        /// </summary>
        /// <param name="invoker">The object on which to invoke the method.</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        public async Task<object> CallAsync(object invoker)
        {
            if (ObjectType != null && invoker?.GetType() != ObjectType)
                throw new InvalidCastException();
            if (!IsAsync)
                return Call(invoker);
            return await (Task<object>)Method.Invoke(invoker, Parameters);
        }
    }
#endif
}
