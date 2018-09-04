using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WeiXinMvc.Models
{
    public class TalkModel:PHOTO_COMMENT
    {
        //用户微信名字
        public string WechatNickName { get; set; }

        //微信头像
        public string Headimgurl { get; set; }

        //用户OpenID
        public string OpenId { get; set; }

        //点赞状态
        public string status { get; set; }
    }
}