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
        public string ID { get; private set; }
        public bool Failed { get; private set; }
        public Type Returns { get; private set; }
        public Exception Error { get; private set; }
        public bool IsAsync { get; private set; }
        public string ExtensionID { get; private set; }

        private MethodInfo Infos { get; set; }
        private Dictionary<string, string> Parameters { get; set; }
        private Sentence _s { get; set; }
        private object Base { get; set; }

        internal Answer(Exception err)
        {
            Failed = true;
            Error = err;
            Returns = null;
            Infos = null;
            Parameters = null;
            ExtensionID = null;
            ID = null;
            _s = null;
            Base = null;
            IsAsync = false;
        }

        internal Answer(Sentence s, Dictionary<string, string> pa)
		{
			IsAsync = s.Method.ReturnType.FullName.StartsWith("System.Threading.Tasks.Task");
			Returns = IsAsync ? s.Method.ReturnType.GenericTypeArguments[0] : s.Method.ReturnType;
            Infos = s.Method;
            Parameters = pa;
            ExtensionID = s.ExAttr.ID;
            ID = s.Attr.ID;
            Error = null;
            Failed = false;
            _s = s;
            Base = s.Base;
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

            List<dynamic> param = new List<dynamic>();
            foreach (var i in Infos.GetParameters())
            {
                ConverterAttribute a = (ConverterAttribute)i.GetCustomAttributes(typeof(ConverterAttribute), true).FirstOrDefault();

                if (a == null && i.ParameterType == typeof(Sentence))
                    param.Add(_s);
                else if (i.IsOptional && !Parameters.ContainsKey(i.Name))
                    param.Add(i.DefaultValue);
                else
                {
                    if (!Parameters.ContainsKey(i.Name))
                        throw new Exception("Parameters don't match.");
                    else if (a == null)
                        param.Add(Parameters[i.Name]);
                    else if (a.MethodName == "Auto")
                        param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                    else if (a.ConvertMethod != null && a.ConvertMethod.ReturnType == i.ParameterType)
                        param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                }
            }

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
                List<object> param = new List<object>();
                foreach (var i in Infos.GetParameters())
                {
                    ConverterAttribute a = (ConverterAttribute)i.GetCustomAttributes(typeof(ConverterAttribute), true).FirstOrDefault();

                    if (i.ParameterType == typeof(Sentence))
                        param.Add(_s);
                    else if (i.IsOptional && !Parameters.ContainsKey(i.Name))
                        param.Add(i.DefaultValue);
					else if (!Parameters.ContainsKey(i.Name))
						throw new Exception("Parameters don't match.");
                    else if (a == null)
                        param.Add(Parameters[i.Name]);
                    else if (a.MethodName == "Auto" || (a.ConvertMethod != null && a.ConvertMethod.ReturnType == i.ParameterType))
                    	param.Add(a.ConvertMethod.Invoke(null, new string[] { Parameters[i.Name] }));
                }

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
			
            List<dynamic> param = new List<dynamic>();
            foreach (var i in Infos.GetParameters())
            {
                ConverterAttribute a = (ConverterAttribute)i.GetCustomAttributes(typeof(ConverterAttribute), true).FirstOrDefault();

                if (a == null && i.ParameterType == typeof(Sentence))
                    param.Add(_s);
                else if (i.IsOptional && !Parameters.ContainsKey(i.Name))
                    param.Add(i.DefaultValue);
                else
                {
                    if (!Parameters.ContainsKey(i.Name)) throw new Exception("Parameters don't match.");
                    else if (a == null) param.Add(Parameters[i.Name]);
                    else if (a.MethodName == "Auto")
                        param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                    else if (a.ConvertMethod != null && a.ConvertMethod.ReturnType == i.ParameterType)
                        param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                }
            }

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
                List<object> param = new List<object>();
                foreach (var i in Infos.GetParameters())
                {
                    ConverterAttribute a = (ConverterAttribute)i.GetCustomAttributes(typeof(ConverterAttribute), true).FirstOrDefault();

                    if (a == null && i.ParameterType == typeof(Sentence))
                        param.Add(_s);
                    else if (i.IsOptional && !Parameters.ContainsKey(i.Name))
                        param.Add(i.DefaultValue);
                    else
                    {
                        if (!Parameters.ContainsKey(i.Name))
                            throw new Exception("Parameters don't match.");
                        else if (a == null)
                            param.Add(Parameters[i.Name]);
                        else if (a.MethodName == "Auto" || (a.ConvertMethod != null && a.ConvertMethod.ReturnType == i.ParameterType))
                            param.Add(a.ConvertMethod.Invoke(null, new string[1] { Parameters[i.Name] }));
                    }
                }

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
            this.ExtensionID = null;
            this.Failed = true;
            this.ID = null;
            this.Infos = null;
            this.IsAsync = false;
            this.Parameters = null;
            this.Returns = null;
        }
    }
}
