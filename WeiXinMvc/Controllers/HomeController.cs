using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WeiXinMvc.App_Start;
using System.Security;
using System.Threading;
using System.Security.Cryptography;
using System.Drawing.Imaging;
using System.Drawing;
using WeiXinMvc.Models;
/******************************************************************************
*
* Unit Name: 		HomeController.cs
* Author:			Gets Dev Team
* Create Date:		2018-08-20
* Purpose:		    微信登录授权
* History:          Created by Xiaohua
*
* Copyright (c) Gets 2018
* www.igets.net
*
******************************************************************************/

namespace WeiXinMvc.Controllers
{
    public class HomeController : Controller
    {
        #region [全局参数]
        static string _AccessTokenLogin = "";//网页登录授权
        static string _AccessTokenInterface = "";//网页接口认证
        static string _OpenId = "";//用户OpenId
        static string _NickName = "";//用户姓名
        static string _HeadImage = "";//用户头像
        static string _MatchId = "";//赛事编号
        List<IndexModel> IndexResources = new List<IndexModel>();//定义一个公共List变量(用于显示首页)
        List<DetailModel> DetailResources = new List<DetailModel>();//定义一个公共List变量(用于显示列表页)
        List<TalkModel> TalkResources = new List<TalkModel>();//定义一个公共List变量(用于显示列表页)
        MatchPhotoEntities db = new MatchPhotoEntities();

        #endregion

        #region[页面]

        /// <summary>
        /// Login页面是用于微信用户登录跳转的
        /// </summary>
        public ActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Index页面接收微信传来的code参数，并获取用户信息和配置微信签名
        /// </summary>
        [HttpGet]
        public ActionResult Index(string matchid)
        {
            //331b6a1c6be64655a79c81d969827915
            //如果没有用户信息,代表还没有进行登录授权
            if (matchid != null)
            {
                _MatchId = matchid;
                CacheHelper.SetCache("match_id", matchid);//添加缓存
                return Redirect("/Home/Login");
            }
            else {
                //这是Shou方法回调后的判断，如果为True，则说明这是直接登录/Home/Index链接，则找不到地址
                if (_MatchId == ""||_MatchId==null)
                {
                    return Redirect("/Home/Error404");
                }
                else {
                    try { 
                        ShowIndex(_MatchId);
                        //合成签名
                        WeXinSigature();
                        AddWeChatUser();//加入微信用户表
                        return View(IndexResources);
                    }
                    catch
                    {
                        return View();
                    }
                }
            }
        }

        /// <summary>
        /// List页面展示所有照片
        /// </summary>
        public ActionResult List()
        {
            ShowDetail("331b6a1c6be64655a79c81d969827915");
            return View(DetailResources);
        }

        /// <summary>
        /// Talk页面用于展示所有评论
        /// </summary>
        public ActionResult Talk()
        {
            ShowTalk(_MatchId);
            return View();
        }

        public ActionResult Test()
        {
            return View();
        }

        /// <summary>
        /// Error404页面用于友好提示用户
        /// </summary>
        public ActionResult Error404()
        {
            return View();
        }

        #endregion

        #region[微信方法]
        /// <summary>
        /// 此方法用于接收微信给我的回调函数code与state，然后回调至主页
        /// </summary>
        public ActionResult Shou(string code, string state)
        {
            //GetAandO(code);
            //GetUerInfo();//获取用户信息
            return Redirect("/Home/Index");
        }

        /// <summary>
        /// 合成签名
        /// </summary>
        private void WeXinSigature()
        {
            string lurl = System.Web.HttpContext.Current.Request.Url.AbsoluteUri.ToString();//获取网站当前的地址
            string lnn = GetRandCode(16);
            string ltt = TimeStamp();
            ViewBag.mnonceStr = lnn;
            ViewBag.mtimestamp = ltt;
            ViewBag.msignature= MakeSha1Sign(TicKet(), lnn, ltt, lurl);//合成签名
        }

        //获取Access_Token和OpenId(这里的access_token码和接口的是不一样的)
        private void GetAandO(string code)
        {

            string lpageHtml = "";
            WebClient MyWebClient = new WebClient();
            MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
            string lgetAO = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid=wx0ceb887ea81f6fed&secret=ba80a0458381d1f1b4297a84487a1897&code={0}&grant_type=authorization_code", code);
            Byte[] pageData = MyWebClient.DownloadData(lgetAO); //从指定网站下载数据
            MemoryStream ms = new MemoryStream(pageData);
            using (StreamReader sr = new StreamReader(ms, Encoding.GetEncoding("GB2312")))
            {
                lpageHtml = sr.ReadLine();
            }
            //收取accesstoken和openid
            JObject jo = (JObject)JsonConvert.DeserializeObject(lpageHtml);
            _AccessTokenLogin = jo["access_token"].ToString();
            _OpenId = jo["openid"].ToString();
        }

        //获取用户信息
        private void GetUerInfo()
        {
            string lUserInfo = "";
            WebClient getlast = new WebClient();
            getlast.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
            string lgetUserInfo = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN", _AccessTokenLogin, _OpenId);
            Byte[] pageDataUser = getlast.DownloadData(lgetUserInfo); //从指定网站下载数据
            MemoryStream ms2 = new MemoryStream(pageDataUser);
            using (StreamReader sr2 = new StreamReader(ms2, Encoding.GetEncoding("utf-8")))
            {
                lUserInfo = sr2.ReadLine();
            }
            JObject jo2 = (JObject)JsonConvert.DeserializeObject(lUserInfo);
            _OpenId = jo2["openid"].ToString();
            _HeadImage = jo2["headimgurl"].ToString();
            _NickName = jo2["nickname"].ToString();
        }

        /// <summary>
        /// 这一段生成微信签名
        /// </summary>
        #region[签名授权参数]

        //生成时间戳
        public static string TimeStamp()
        {
            TimeSpan lts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(lts.TotalSeconds).ToString();
        }

        //获取接口Access_Token
        public string Access_Token()
        {
            /// <summary>
            /// 取到Access_Token码
            /// </summary>
            /// <param name="lpageHtml">接收微信发出信号的页面</param>
            /// <param name="jo">获取到的Access_Token</param>
            try
            {
                if (CacheHelper.GetCache("accesstoken") == null)
                {
                    string lpageHtml = "";
                    WebClient MyWebClient = new WebClient();
                    MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                    string getAO = string.Format("https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid=wx0ceb887ea81f6fed&secret=ba80a0458381d1f1b4297a84487a1897");
                    Byte[] pageData = MyWebClient.DownloadData(getAO); //从指定网站下载数据
                    MemoryStream ms = new MemoryStream(pageData);
                    using (StreamReader sr = new StreamReader(ms, Encoding.GetEncoding("UTF-8")))
                    {
                        lpageHtml = sr.ReadLine();
                    }
                    //收取accesstoken
                    JObject jo = (JObject)JsonConvert.DeserializeObject(lpageHtml);
                    CacheHelper.SetCache("accesstoken", jo["access_token"], 7200);
                    _AccessTokenInterface = jo["access_token"].ToString();
                    return _AccessTokenInterface;
                }
                else
                {
                    return CacheHelper.GetCache("accesstoken").ToString();
                }
            }
            catch
            {
                return "NO";
            }
        }

        //生成随机串
        public static string GetRandCode(int iLength)
        {
            string lCode = "";
            if (iLength == 0)
            {
                iLength = 4;
            }
            string codeSerial = "0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            string[] arr = codeSerial.Split(',');
            int randValue = -1;
            Random rand = new Random(unchecked((int)DateTime.Now.Ticks));
            for (int i = 0; i < iLength; i++)
            {
                randValue = rand.Next(0, arr.Length - 1);
                lCode += arr[randValue];
            }
            return lCode;
        }

        //获取ticket
        public string TicKet()
        {
            /// <summary>
            /// 将上半部分获取的Access_Token码来换取TicKet，并存入缓存
            /// </summary>
            /// <param name="lpageHtml">接收微信发出信号的页面</param>
            /// <param name="jo">获取到的TicKet</param>
            if (CacheHelper.GetCache("commonData_Ticket") == null)
            {
                string lpageHtml = "";
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
                string getAO = string.Format("http://api.weixin.qq.com/cgi-bin/ticket/getticket?type=jsapi&access_token={0}", Access_Token());
                Byte[] pageData = MyWebClient.DownloadData(getAO); //从指定网站下载数据
                MemoryStream ms = new MemoryStream(pageData);
                using (StreamReader sr = new StreamReader(ms, Encoding.GetEncoding("utf-8")))
                {
                    lpageHtml = sr.ReadLine();
                }
                //收取accesstoken
                JObject jo = (JObject)JsonConvert.DeserializeObject(lpageHtml);
                string ticket = jo["ticket"].ToString();
                CacheHelper.SetCache("commonData_Ticket", ticket, 7200);//添加缓存
                //var jsit = CacheHelper.GetCache("commonData_Ticket");//读取缓存
                return jo["ticket"].ToString();
            }
            else
            {
                return CacheHelper.GetCache("commonData_Ticket").ToString();
            }
        }

        //合成微信配置签名
        public string MakeSha1Sign(string ticket, string nonce, string timest, string url)
        {
            string signature = string.Format("jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}", ticket, nonce, timest, url);
            byte[] StrRes = Encoding.Default.GetBytes(signature);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            StrRes = iSHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte iByte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", iByte);
            }
            return EnText.ToString();
        }
        #endregion

        #endregion

        #region[资源方法]
        /// <summary>
        /// 加载首页的资源
        /// </summary>
        /// <param name="matchid">赛事编号（相当于相册编号）</param>
        public void ShowIndex(string matchid)
        {
            IndexResources = db.PHOTO_MATCH.Where(p => p.Id.Contains(matchid))
               .Select(p => new IndexModel
               {
                   MatchCover = p.MatchCover,
                   MatchName = p.MatchName,
                   MatchTime = p.MatchTime,
                   MatchDescription = p.MatchDescription
               }).ToList();
        }

        //添加(Talk)的资源
        public void ShowTalkAdd(string comment)
        {
            var mguid = Guid.NewGuid().ToString("N");//生成guid
            PHOTO_COMMENT ad = new PHOTO_COMMENT();
            ad.Id = mguid;
            ad.WechatUserId = "cbb68e3fffe6426c931d59a80ab0ba32";
            ad.MatchId = _MatchId;
            ad.CommentContent = comment;
            ad.CommentTime = DateTime.Now;
            ad.Ctime = DateTime.Now;
            ad.Mtime = DateTime.Now;
            ad.Createdby = "peng";
            ad.Modifiedby = "xiao";
            ad.Loved = 0;
            using (MatchPhotoEntities db = new MatchPhotoEntities())
            {
                db.PHOTO_COMMENT.Add(ad);
                db.SaveChanges();
            }
        }

        //展示(Detail)相册的资源
        public void ShowDetail(string matchid)
        {
            DetailResources = db.PHOTO_PICTURE.Where(p => p.MatchId.Contains(matchid))
                .Select(p => new DetailModel
                {
                    PictureAddress = p.PictureAddress,
                    ImageWidth = p.ImageWidth,
                    ImageHeight = p.ImageHeight,
                    CameraBrand = p.CameraBrand,
                    CameraLens = p.CameraLens,
                    CameraModel = p.CameraModel,
                    PictureDescription = p.PictureDescription
                }).ToList();
        }

        public void ShowTalk(string matchid) {
            var aa = from p in db.PHOTO_COMMENT.Where(p => p.MatchId.Contains("331b6a1c6be64655a79c81d969827915"))
                     join pp in db.PHOTO_WECHAT_USER on p.WechatUserId equals pp.Id
                     orderby p.Loved descending
                     select new
                     {
                         pp.WechatNickName,
                         p.CommentContent,
                         p.Loved,
                         p.CommentTime,
                         p.Id
                     };
            foreach (var item in aa)
            {
                TalkModel add = new TalkModel();
                add.WechatNickName = item.WechatNickName;
                add.CommentContent = item.CommentContent;
                add.Loved = item.Loved;
                add.CommentTime = item.CommentTime;
                add.Id = item.Id;
                var bb = from a in db.PHOTO_LOVED.Where(a => a.OpenId.Contains("adwa2323hjkh13hjk"))
                         select new
                         {
                             a.CommentId
                         };
                foreach (var item2 in bb)
                {
                    if (add.Id == item2.CommentId)
                    {
                        add.status = "1";
                        break;
                    }
                    else
                    {
                        add.status = "0";
                    }
                }
                TalkResources.Add(add);
            }
        }

        //将用户信息加入微信用户表
        public void AddWeChatUser()
        {
            try
            {
                //将用户信息加入微信用户表
                PHOTO_WECHAT_USER mwe = new PHOTO_WECHAT_USER();
                mwe.Id = Guid.NewGuid().ToString("N");//生成guid
                mwe.OpenId = _OpenId;
                mwe.WechatNickName = _NickName;
                mwe.Headimgurl = _HeadImage;
                mwe.Ctime = DateTime.Now;
                mwe.Mtime = DateTime.Now;
                mwe.Createdby = "peng";
                mwe.Modifiedby = "nobody";
                using (MatchPhotoEntities db = new MatchPhotoEntities())
                {
                    db.PHOTO_WECHAT_USER.Add(mwe);
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
        #endregion

    }
}