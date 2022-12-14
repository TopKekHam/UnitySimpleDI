
using System;

namespace SimpleDI
{
    public class FunctionResolver : IDIResolver
    {
        private Func<object> func;
    
        public FunctionResolver(Func<object> func)
        {
            this.func = func;
        }
    
        public object Resolve()
        {
            return func();
        }
    }
}