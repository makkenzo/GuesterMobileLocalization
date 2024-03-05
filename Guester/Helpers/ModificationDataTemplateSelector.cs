using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Helpers
{
    public class ModificationDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SinglTemplate { get; set; }
        public DataTemplate MultiTemplate { get; set; }
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item.GetType() == typeof(Modifiers))
                return ((Modifiers)item).IsOnlyOne ? SinglTemplate : MultiTemplate;
            else
                return ((SelectedModifiers)item).ModifierId?.IsOnlyOne==true ? SinglTemplate : MultiTemplate;

        }
    }
   
}
