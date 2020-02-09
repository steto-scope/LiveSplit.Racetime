using LiveSplit.Racetime.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Controller
{
    public abstract class AuthenticatorBase
    {
        protected readonly IAuthentificationSettings s;

        public AuthenticatorBase(IAuthentificationSettings settings)
        {
            s = settings;
        }

        protected string Code { get; set; }
        protected string RedirectUri
        {
            get
            {
                return $"http://{s.RedirectAddress}:{s.RedirectPort}/";
            }
        }
        public string AccessToken { get; protected set; }
        public string RefreshToken { get; protected set; }
        public RacetimeUser Identity { get; protected set; }
        public string Error { get; protected set; }
        public DateTime TokenExpireDate { get; protected set; }
        public bool IsAuthenticated
        {
            get
            {
                return Code != null;
            }
        }
        public bool IsAuthorized
        {
            get
            {
                return (AccessToken != null);
            }
        }

        public bool IsRefreshRequired
        {
            get
            {
                return IsAuthorized && (DateTime.Now > TokenExpireDate);
            }
        }

        public void Reset()
        {
            Code = null;
            AccessToken = null;
            RefreshToken = null;
            TokenExpireDate = DateTime.MaxValue;
        }

        protected async Task<bool> SendRedirectAsync(TcpClient client, string target)
        {
            await Task.Run(() =>
            {
                using (var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)))
                {
                    writer.WriteLine("HTTP/1.0 301 Moved Permanently");
                    writer.WriteLine($"Location: {s.AuthServer}{target}");
                }
            });
            return true;
        }

        public abstract Task<bool> Authorize(bool forceRefresh = false);
        public abstract Task<bool> RevokeAccess();

    }
}
