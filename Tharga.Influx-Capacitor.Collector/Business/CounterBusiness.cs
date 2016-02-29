﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Tharga.InfluxCapacitor.Collector.Event;
using Tharga.InfluxCapacitor.Collector.Interface;

namespace Tharga.InfluxCapacitor.Collector.Business
{
    public class CounterBusiness : ICounterBusiness
    {
        private readonly PerformanceCounterProvider _perfCounterProvider;

        private readonly Dictionary<string, IPerformanceCounterProvider> _additionalProviders;

        public event EventHandler<GetPerformanceCounterEventArgs> GetPerformanceCounterEvent;

        public CounterBusiness()
        {
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            }

            _perfCounterProvider = new PerformanceCounterProvider(this.OnGetPerformanceCounters, null);
            _additionalProviders = new Dictionary<string, IPerformanceCounterProvider>(StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<IPerformanceCounterGroup> GetPerformanceCounterGroups(IConfig config)
        {
            if (config.Groups == null) throw new NullReferenceException("No groups in config.");

            if (config.Providers != null)
            {
                foreach (var providerConfig in config.Providers)
                {
                    if (!_additionalProviders.ContainsKey(providerConfig.Name))
                    {
                        var provider = providerConfig.Load(typeof(PerformanceCounterProvider).Assembly, typeof(PerformanceCounterProvider).Namespace);
                        provider.Setup(providerConfig);
                        _additionalProviders.Add(providerConfig.Name, provider);
                    }
                }
            }

            foreach (var group in config.Groups)
            {
                var providerName = group.Provider;

                if (string.IsNullOrEmpty(providerName) || string.Equals(providerName, _perfCounterProvider.Name, StringComparison.OrdinalIgnoreCase))
                {
                    // use default performance counter provider
                    yield return _perfCounterProvider.GetGroup(group);
                }
                else
                {
                    // use specific provider
                    IPerformanceCounterProvider provider;
                    if (_additionalProviders.TryGetValue(providerName, out provider))
                    {
                        yield return provider.GetGroup(group);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("There is no registered provider named '{0}'. Registered providers are: {1}", providerName, string.Join(", ", GetAllProviders().Select(p => p.Key))));
                    }
                }
            }
        }

        public IEnumerable<string> GetCategoryNames()
        {
            return GetAllProviders().SelectMany(provider => provider.Value.GetCategoryNames());
        }

        public IEnumerable<string> GetCounterNames(string categoryName, string machineName)
        {
            return GetAllProviders().SelectMany(provider => provider.Value.GetCounterNames(categoryName, machineName));
        }

        public IEnumerable<string> GetInstances(string category, string counterName, string machineName)
        {
            return GetAllProviders().SelectMany(provider => provider.Value.GetInstances(category, counterName, machineName));
        }

        protected virtual void OnGetPerformanceCounters(string message)
        {
            var handler = GetPerformanceCounterEvent;
            if (handler != null)
            {
                handler(this, new GetPerformanceCounterEventArgs(message));
            }
        }

        private IEnumerable<KeyValuePair<string, IPerformanceCounterProvider>> GetAllProviders()
        {
            yield return new KeyValuePair<string, IPerformanceCounterProvider>("PerformanceCounter", _perfCounterProvider);

            foreach (var provider in _additionalProviders)
            {
                yield return provider;
            }
        }
    }
}