using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinMvc.Models
{
    /// <summary>
    /// 图片信息
    /// </summary>

    public class DetailModel:PHOTO_PICTURE
    {
        public string mnonceStr { get; set; }
        public string mtimestamp { get; set; }
        public string msignature { get; set; }
    }
}