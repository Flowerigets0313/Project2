//------------------------------------------------------------------------------
// <auto-generated>
//     此代码已从模板生成。
//
//     手动更改此文件可能导致应用程序出现意外的行为。
//     如果重新生成代码，将覆盖对此文件的手动更改。
// </auto-generated>
//------------------------------------------------------------------------------

namespace WeiXinMvc.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PHOTO_COMMENT
    {
        public string Id { get; set; }
        public string WechatUserId { get; set; }
        public string MatchId { get; set; }
        public string CommentContent { get; set; }
        public System.DateTime CommentTime { get; set; }
        public System.DateTime Ctime { get; set; }
        public System.DateTime Mtime { get; set; }
        public string Createdby { get; set; }
        public string Modifiedby { get; set; }
        public int Loved { get; set; }
    }
}
