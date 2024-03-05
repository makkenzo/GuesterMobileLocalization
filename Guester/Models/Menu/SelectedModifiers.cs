using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.Models
{
    public class SelectedModifiers:EmbeddedObject
    {
      
        [MapTo("modifier_id")]
        public Modifiers  ModifierId { get; set; }

        [MapTo("selected_goods")]
        public IList<Goods> SelectedGoodsId { get;  }

       
    }


}
