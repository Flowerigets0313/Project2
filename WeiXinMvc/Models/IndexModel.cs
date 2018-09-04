using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinMvc.Models
{
    public class IndexModel : PHOTO_MATCH
    {
        public string mnonceStr { get; set; }
        public string mtimestamp { get; set; }
        public string msignature { get; set; }

    }
}