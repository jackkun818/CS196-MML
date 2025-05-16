using Microsoft.Identity.Client.NativeInterop;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.extension
{
    public class Crs_BindableBase : BindableBase, INavigationAware, IRegionMemberLifetime
    {
        /// <summary>
        /// 是否在RegionManager保存View实例
        /// </summary>
        public bool KeepAlive => false;

        /// <summary>
        ///  指定当前View是否是导航目标，返回值为true则导航至该View实例。如果不是则返回false，RegionManager创建目标View的新实例。
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        /// <summary>
        /// 进入新View时调用
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// 从当前View离开时调用
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
