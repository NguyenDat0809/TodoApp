// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace Todo.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;

        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            //'??=' : null-conditional assignment operator - toán tử gán có điều kiện null
            // Giá trí sau dấu bằng sẽ dc gán khi giá trị bên trái dấu bằng là null
            
            //Url.Content(): tạo đường dẫn dựa trên đường dẫn hiện thời của web
            //("nội dụng") sẽ được nối tiếp vào tùy theo cách sử dụng
            //Ví dụ: Url.Content("~/image") -> tạo ra đường dẫn mới đến image
            returnUrl ??= Url.Content("~/"); 

            //lấy ra danh sách AuthenticationScheme đã được khai báo ở program.cs
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                //method được khởi tạo ở phía dưới -> trả về 1 instance của IdentityUser
                var user = CreateUser();

                //sau khi tạo user sẽ cập nhật UserName và Email cho user
                //cập nhật UserName thông qua instance của IUserStore
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                //cập nhật Email thông qua instance của IUserEmailStore (một class extends từ IUserStore)
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                //CancellationToken - sử dụng để thông báo hủy hành động
                //TẠI SAO KHÔNG TRUY CẬP TRỰC TIẾP USER RỒI CẬP NHẬT USERNAME VÀ EMAIL ???
                //-> đảm bảo tính bảo mật,...

                //tạo và lưu trữ user vào db
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    //lấy user id ra
                    var userId = await _userManager.GetUserIdAsync(user);

                    //lấy ra một token ngẫu nhiên để confirm email từ user
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //mã code được mã hóa bằng Base64 để đảm bảo an toàn khi truyền qua URL
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    //Url.Page ~~ Url.Action
                    //tạo một url đến page comfirmEmail gồm các option dc chỉnh như dưới
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    //interface cung ấp API hỗ trợ Identity gửi email confirm và reset
                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        //LocalRedirect - gửi người dùng về với status code 302 - Found HTTP
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        /// <summary>
        /// Hàm tạo ra 1 instance cho IdentityUser
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IdentityUser CreateUser()
        {
            try
            {
                //System.Activator cho phép tạo 1 instance của 1 class với độ tùy biến cao 
                //Exception hay gặp của hàm CreateInstance() là MissingMethodException -> trong trường hợp class đó ko tồn tại
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        /// <summary>
        /// Hàm trả về 1 instance cho IUserEmailStore
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            //ép kiểu IUserStore instance qua IUserEmailStore rồi trả về
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
