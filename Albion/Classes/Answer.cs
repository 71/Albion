using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    public class Answer : IDisposable
    {
        public bool Failed { get; private set; }
        public Type Returns { get; private set; }
        public Exception Error { get; private set; }
        public bool IsAsync { get; private set; }

        private MethodInfo Infos { get; set; }
        private Dictionary<string, object> Parameters { get; set; }
        private Sentence _s { get; set; }
        private object Base { get; set; }

        internal Answer(Exception err)
        {
            Failed = true;
            Error = err;
            Returns = null;
            Infos = null;
            Parameters = null;
            _s = null;
            Base = null;
            IsAsync = false;
        }

        internal Answer(Sentence s, Dictionary<string, object> pa)
		{
			IsAsync = s.Method.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task");
			Returns = IsAsync ? s.Method.ReturnType.GenericTypeArguments[0] : s.Method.ReturnType;
            Infos = s.Method;
            Parameters = pa;
            Error = null;
            Failed = false;
            _s = s;
            Base = s.Base;
        }

        private List<dynamic> GetParameters()
        {
            List<dynamic> param = new List<dynamic>();

            foreach (var i in Infos.GetParameters())
            {
                ConverterAttribute a = i.GetCustomAttribute<ConverterAttribute>(true);

                if (i.ParameterType == typeof(Sentence))
                    param.Add(_s);
                else if (i.IsOptional && !Parameters.ContainsKey(i.Name))
                    param.Add(i.DefaultValue);
                else if (!Parameters.ContainsKey(i.Name))
                    throw new Exception("Parameters don't match");
                else
                    param.Add(Parameters[i.Name]);
            }

            return param;
        }

        /// <summary>
        /// Call the method.
        /// </summary>
        /// <returns>What the method returns (this.Returns)</returns>
        public T Call<T>()
        {
			if (Failed)
				throw Error;
			else if (Returns != typeof(T))
				throw new InvalidCastException ();
			else if (IsAsync)
				return CallAsync<T> ().GetAwaiter ().GetResult ();

            List<dynamic> param = GetParameters();

            try
            {
                return (T)Infos.Invoke(Base, param.ToArray());
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public async Task<T> CallAsync<T>()
        {
			if (Failed)
				throw Error;
			else if (!IsAsync)
				return Call<T> ();
			else if (Returns != typeof(T))
				throw new InvalidCastException ();
            else
            {
                List<object> param = GetParameters();

                try
                {
                    Task<T> t = (Task<T>)Infos.Invoke(Base, param.ToArray());
					//t.Start();
                    return await t;
                }
                catch (Exception)
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// Call the method.
        /// </summary>
        /// <returns>What the method returns (this.Returns)</returns>
        public object Call()
        {
			if (Failed)
				throw Error;
			else if (IsAsync)
				return CallAsync ().GetAwaiter ().GetResult ();

            List<dynamic> param = GetParameters();

            try
            {
                return Infos.Invoke(Base, param.ToArray());
            }
            catch (Exception e)
            {
                return e;
            }
        }

        public async Task<object> CallAsync()
        {
            if (Failed) throw Error;
            else if (!IsAsync) return Call();
            else
            {
                List<object> param = GetParameters();

                try
                {
                    Task<dynamic> t = (Task<dynamic>)Infos.Invoke(Base, param.ToArray());
                    return await t;
                }
                catch (Exception e)
                {
                    return e;
                }
            }
        }

        public void Dispose()
        {
            this._s = null;
            this.Base = null;
            this.Error = new ObjectDisposedException("answer");
            this.Failed = true;
            this.Infos = null;
            this.IsAsync = false;
            this.Parameters = null;
            this.Returns = null;
        }
    }
}
