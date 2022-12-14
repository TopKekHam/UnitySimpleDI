
using System;

namespace SimpleDI
{
    public class FunctionResolver : IDIResolver
    {
        private Func<object> _func;
    
        public FunctionResolver(Func<object> func)
        {
            this._func = func;
        }
    
        public object Resolve()
        {
            return _func();
        }
    }
}