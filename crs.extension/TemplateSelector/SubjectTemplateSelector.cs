using crs.extension.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace crs.extension.TemplateSelector
{
    public class SubjectTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (element != null && item is SubjectItem _item)
            {
                var templateName = _item.TemplateName;
                if (element.TemplatedParent is FrameworkElement parentElement && parentElement.Tag != null)
                {
                    templateName = $"{templateName}.{parentElement.Tag?.ToString()}";
                }

                var template = (element.FindResource(templateName ?? string.Empty) as DataTemplate) ?? (element.FindResource("MMSE_MoCA_Null") as DataTemplate);
                return template;
            }
            return null;
        }
    }
}
