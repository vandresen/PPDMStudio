using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPDMStudio.Models
{
    public class AreaItem
    {
        public string AreaId { get; set; } = string.Empty;
        public string PreferredName { get; set; } = string.Empty;

        // So MudAutocomplete can display the name as the selected value
        public override string ToString() => PreferredName;
    }
}
