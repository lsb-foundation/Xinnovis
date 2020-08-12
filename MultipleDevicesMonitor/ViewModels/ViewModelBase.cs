using System;
using System.Collections.Generic;
using System.Reflection;
using CommonLib.Mvvm;

namespace MultipleDevicesMonitor.ViewModels
{
    public class ViewModelBase : BindableBase
    {
        private static readonly List<ViewModelBase> viewModelInstances;

        static ViewModelBase()
        {
            viewModelInstances = new List<ViewModelBase>();
            CreateViewModelInstances();
        }

        private static void CreateViewModelInstances()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach(var type in types)
            {
                if(type.BaseType == typeof(ViewModelBase))
                {
                    var instance = Activator.CreateInstance(type) as ViewModelBase;
                    viewModelInstances.Add(instance);
                }
            }
        }

        public static ViewModelBase GetViewModelInstance<T>()
        {
            foreach(var instance in viewModelInstances)
            {
                if (instance.GetType() == typeof(T))
                    return instance;
            }
            return default;
        }
    }
}
