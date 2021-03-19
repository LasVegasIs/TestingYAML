using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Crey.DBModels
{
    public partial class Meta
    {
        [Key]
        [StringLength(256)]
        public string Key { get; set; }

        [StringLength(256)]
        public string Value { get; set; }
    }
}
