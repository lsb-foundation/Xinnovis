using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommonLib.Mvvm
{
    /// <summary>
    /// 提供统一ViewModel单例工厂的ViewModelBase基类
    /// </summary>
    public class ViewModelBase : BindableBase
    {
        private static readonly List<ViewModelBase> viewModelInstances;

        static ViewModelBase()
        {
            viewModelInstances = new List<ViewModelBase>();
            CreateViewModelInstances();
        }

        /// <summary>
        /// 运行时自动注册ViewModel
        /// </summary>
        private static void CreateViewModelInstances()
        {
            var types = Assembly.GetEntryAssembly().GetTypes();
            foreach (var type in types)
            {
                if (type.BaseType == typeof(ViewModelBase))
                {
                    var instance = Activator.CreateInstance(type) as ViewModelBase;
                    viewModelInstances.Add(instance);
                }
            }
        }

        /// <summary>
        /// 获取需要的ViewModel实例
        /// </summary>
        /// <typeparam name="T">T必须是ViewModelBase的子类</typeparam>
        /// <returns></returns>
        public static T GetViewModelInstance<T>() where T : ViewModelBase
        {
            foreach (var instance in viewModelInstances)
            {
                if (instance.GetType() == typeof(T))
                    return instance as T;
            }
            return default;
        }
    }
}
