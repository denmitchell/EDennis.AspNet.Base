using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetStandard.Base {
    public class IOptionsMonitorFactory {

        public static IOptionsMonitor<T> Create<T>(T obj)
            where T : class, new()
            => new OptionsMonitor<T>(obj);

        internal class OptionsMonitor<T> : IOptionsMonitor<T> 
            where T: class, new() {

            public OptionsMonitor (T currentValue) {
                CurrentValue = currentValue;
            }

            public T CurrentValue { get; }

            public T Get(string name) => CurrentValue;

            public IDisposable OnChange(Action<T, string> listener) {
                throw new NotImplementedException();
            }
        }

    }
}
