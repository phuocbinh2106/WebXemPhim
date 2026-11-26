using System;
using System.Web;
using System.Web.Security;
using WebXemPhim.Models;
using WebXemPhim.Services;

namespace WebXemPhim.Helpers
{
    /// <summary>Gom thao tác ghi/xóa trạng thái đăng nhập (Session + cookie .ASPXAUTH).</summary>
    public static class AuthHelper
    {
        public static void FillSession(HttpSessionStateBase session, User user)
        {
            session["UserId"] = user.UserId;
            session["Username"] = user.Username;
            session["Avatar"] = user.Avatar;
            session["Role"] = user.Role;
            session["Nickname"] = AccountRules.DisplayName(user.Nickname, user.Username);
        }

        public static void FillSession(System.Web.SessionState.HttpSessionState session, User user)
        {
            session["UserId"] = user.UserId;
            session["Username"] = user.Username;
            session["Avatar"] = user.Avatar;
            session["Role"] = user.Role;
            session["Nickname"] = AccountRules.DisplayName(user.Nickname, user.Username);
        }

        /// <summary>Đăng xuất hoàn toàn: xóa Session và hủy cookie xác thực (SCRUM-8).</summary>
        public static void SignOut(HttpContextBase context)
        {
            FormsAuthentication.SignOut();
            if (context.Session != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
            }

            var expired = new HttpCookie(FormsAuthentication.FormsCookieName, string.Empty)
            {
                Expires = DateTime.Now.AddYears(-1),
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };
            context.Response.Cookies.Add(expired);

            var sessionCookie = new HttpCookie("ASP.NET_SessionId", string.Empty) { Expires = DateTime.Now.AddYears(-1) };
            context.Response.Cookies.Add(sessionCookie);
        }
    }
}
