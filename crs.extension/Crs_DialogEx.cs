using HandyControl.Controls;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crs.extension
{
    public class Crs_DialogEx
    {
        public static IContainerProvider ContainerProvider { get; set; }

        static void VerifyContainerProvider()
        {
            if (ContainerProvider == null)
            {
                throw new ArgumentNullException(nameof(IContainerProvider), "containerProvider is null");
            }
        }

        public static Dialog Show(string viewName, string token = "")
        {
            VerifyContainerProvider();

            if (string.IsNullOrWhiteSpace(token))
            {
                token = Crs_DialogToken.TopContent;
            }

            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException(nameof(viewName), "viewName is null");
            }
            return Dialog.Show(ContainerProvider.Resolve<object>(viewName), token);
        }

        public static Dialog ProgressShow(string token = "")
        {
            VerifyContainerProvider();

            if (string.IsNullOrWhiteSpace(token))
            {
                token = Crs_DialogToken.TopProgress;
            }

            return Dialog.Show(ContainerProvider.Resolve<object>(Crs_Dialog.Loading), token);
        }

        public static Dialog MessageBoxShow(string token = "")
        {
            VerifyContainerProvider();

            if (string.IsNullOrWhiteSpace(token))
            {
                token = Crs_DialogToken.TopMessageBox;
            }

            return Dialog.Show(ContainerProvider.Resolve<object>(Crs_Dialog.MessageBox), token);
        }
    }
}
