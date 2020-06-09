using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibles.DataResources.Models.Dictionaries
{
    public class DictionaryEntity
    {
        public string ModelKey { get; set; }

        public byte[] ModelValue { get; set; }
    }
}
