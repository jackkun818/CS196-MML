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
        /// Is it thereRegionManagerkeepViewExample
        /// </summary>
        public bool KeepAlive => false;

        /// <summary>
        ///  Specify the currentViewWhether it is a navigation target, the return value istrueNavigate to thisViewExample. If not, returnfalse，RegionManagerCreate a targetViewnew instance of .
        /// </summary>
        /// <param name="navigationContext"></param>
        /// <returns></returns>
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        /// <summary>
        /// Enter newViewCalled on time
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        /// <summary>
        /// From the currentViewCalled when leaving
        /// </summary>
        /// <param name="navigationContext"></param>
        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
