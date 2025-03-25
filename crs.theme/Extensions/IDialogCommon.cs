using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.theme.Extensions
{
    /// <summary>
    /// 通用操作
    /// </summary>
    public interface IDialogCommon
    {
        void Execute();
    }

    /// <summary>
    /// 通用操作
    /// </summary>
    public interface IDialogCommon<TParameter>
    {
        void Execute(TParameter parameter);
    }

    /// <summary>
    /// 通用操作
    /// </summary>
    public interface IDialogCommon<TParameter1, TParameter2>
    {
        void Execute(TParameter1 parameter1, TParameter2 parameter2);
    }

    /// <summary>
    /// 通用操作
    /// </summary>
    public interface IDialogCommon<TParameter1, TParameter2, TParameter3>
    {
        void Execute(TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3);
    }

    /// <summary>
    /// 通用操作
    /// </summary>
    public interface IDialogCommon<TParameter1, TParameter2, TParameter3, TParameter4>
    {
        void Execute(TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3, TParameter4 parameter4);
    }

    /// <summary>
    /// 通用操作
    /// </summary>
    public interface IDialogCommon<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5>
    {
        void Execute(TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3, TParameter4 parameter4, TParameter5 parameter5);
    }
}
