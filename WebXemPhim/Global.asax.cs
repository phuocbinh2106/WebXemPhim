using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using WebXemPhim.Helpers;
using WebXemPhim.Models;

namespace WebXemPhim
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// SCRUM-15: khi mở lại trình duyệt, Session mới bị trống nhưng cookie "Ghi nhớ đăng nhập"
        /// vẫn còn. Nạp lại Session từ tài khoản trong cookie để người dùng vẫn ở trạng thái đăng nhập.
        /// </summary>
        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context == null || context.Session == null) return;
            if (context.Session["UserId"] != null) return;
            if (context.User == null || context.User.Identity == null || !context.User.Identity.IsAuthenticated) return;

            var username = context.User.Identity.Name;
            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Username == username);
                if (user != null && string.Equals(user.Username, username, StringComparison.Ordinal))
                {
                    AuthHelper.FillSession(context.Session, user);
                }
                else
                {
                    // Tài khoản không còn tồn tại: hủy cookie
                    System.Web.Security.FormsAuthentication.SignOut();
                }
            }
        }

        /// <summary>
        /// SCRUM-13: ký tự nguy hiểm như "&lt;" trong URL/form không còn hiện trang lỗi hệ thống
        /// mà chuyển sang trang thông báo thân thiện.
        /// </summary>
        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            var httpEx = ex as HttpException;
            bool invalidInput = ex is HttpRequestValidationException
                || ex?.InnerException is HttpRequestValidationException
                || (httpEx != null && httpEx.GetHttpCode() == 400);

            // Gửi form giả mạo / thiếu mã chống CSRF (vd khách gửi POST xóa phim trực tiếp): từ chối và về trang chủ
            if (ex is HttpAntiForgeryException || ex?.InnerException is HttpAntiForgeryException)
            {
                Server.ClearError();
                Response.Clear();
                Response.Redirect("~/", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (invalidInput)
            {
                Server.ClearError();
                Response.Clear();
                Response.Redirect("~/Home/InvalidInput", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }
    }
}
