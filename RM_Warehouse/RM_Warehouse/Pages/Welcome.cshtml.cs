using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RM_Warehouse.Pages
{
    public class WelcomeModel : PageModel
    {
        // THIS FUNCTION IS CALLED FROM LEFT MENU BAR'S LOGOUT OPTION.IT CLEARS SESSIONS.IT REDIRECTS TO LOGIN PAGE.

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Remove("username");
            HttpContext.Session.Remove("warehouse");
            HttpContext.Session.Remove("password");
            HttpContext.Session.Remove("login_user_email");
            HttpContext.Session.Remove("Is_Mechanic");
            HttpContext.Session.Clear();
            return RedirectToPage("Index");
        }

        // THIS PAGE IS REDIRECTED FROM LOGIN PAGE.IT CHECKS SESSION PARAMETER username.IF FOUND SHOWS THIS WELCOME
        // PAGE.ELSE,REDIRECT BACK TO LOGIN PAGE.

        public IActionResult OnGet()
        {
            bool flag_username = string.IsNullOrEmpty(HttpContext.Session.GetString("username"));
  
            if (flag_username)
            {
                return RedirectToPage("Index");
            }
            return Page();
        }
    }
}

