using HandyControl.Controls;
using HandyControl.Tools;
using HandyControl.Tools.Extension;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace crs.theme.Extensions
{
    public static class DialogEx
    {
        public static Action<string, Exception> WriteDialogLog;

        public sealed class DialogException<TResult>
        {
            public string Message { get; set; }
            public Func<Exception, TResult> Exception { get; set; }
        }

        public static Dialog UseConfig_NoMaskBrush(this Dialog dialog)
        {
            dialog.SetValue(Dialog.MaskBrushProperty, null);
            return dialog;
        }

        public static Dialog UseConfig_ContentStretch(this Dialog dialog)
        {
            dialog.SetValue(Dialog.MaskBrushProperty, null);
            dialog.SetValue(Dialog.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            dialog.SetValue(Dialog.VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
            return dialog;
        }

        public static async Task<TResult> GetProgressResultAsync<TResult>(this Dialog dialog, Func<DialogException<TResult>, TResult> action, int minWaitTime = 200)
        {
            DialogException<TResult> exception = new();
            Stopwatch stopwatch = new();
            stopwatch.Start();

            try
            {
                dialog.GetViewModel<IDialogResultable<object>>().Result = await Task.Run(() => action.Invoke(exception));
            }
            catch (Exception ex)
            {
                if (exception.Exception == null)
                {
                    throw new Exception(ex.Message, ex);
                }

                dialog.GetViewModel<IDialogResultable<object>>().Result = exception.Exception.Invoke(ex);
                WriteDialogLog?.Invoke(exception.Message, ex);
            }

            stopwatch.Stop();
            if ((minWaitTime = minWaitTime - (int)stopwatch.ElapsedMilliseconds) > 0)
            {
                await Task.Delay(minWaitTime);
            }

            dialog.Close();
            return (TResult)Convert.ChangeType(await dialog.GetResultAsync<object>(), typeof(TResult));
        }

        public static async Task<TResult> GetProgressResultAsync<TResult>(this Dialog dialog, Func<DialogException<Task<TResult>>, Task<TResult>> action, int minWaitTime = 200)
        {
            DialogException<Task<TResult>> exception = new();
            Stopwatch stopwatch = new();
            stopwatch.Start();

            try
            {
                dialog.GetViewModel<IDialogResultable<object>>().Result = await Task.Run(async () => await action.Invoke(exception));
            }
            catch (Exception ex)
            {
                if (exception.Exception == null)
                {
                    throw new Exception(ex.Message, ex);
                }

                dialog.GetViewModel<IDialogResultable<object>>().Result = await exception.Exception.Invoke(ex);
                WriteDialogLog?.Invoke(exception.Message, ex);
            }

            stopwatch.Stop();
            if ((minWaitTime = minWaitTime - (int)stopwatch.ElapsedMilliseconds) > 0)
            {
                await Task.Delay(minWaitTime);
            }

            dialog.Close();
            return (TResult)Convert.ChangeType(await dialog.GetResultAsync<object>(), typeof(TResult));
        }

        public static async Task<bool?> GetMessageBoxResultAsync(this Dialog dialog, string message, MessageBoxButton button = MessageBoxButton.OK)
        {
            dialog.Initialize<IMessageBox>(vm =>
            {
                vm.Button = button;
                vm.Message = message?.Trim();
            });

            return await dialog.GetResultAsync<bool?>();
        }
    }
}
