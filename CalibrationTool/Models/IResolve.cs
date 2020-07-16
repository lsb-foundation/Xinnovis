using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibrationTool.Models
{
    /// <summary>
    /// 用于约定解析类的接口
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TDest"></typeparam>
    public interface IParse<TSource, TDest>
    {
        TDest Resolve(TSource data);
    }
}
