using LiveSplit.Racetime.Model;
using LiveSplit.Web;
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
        

        public string AccessToken
        {
            get { return CredentialManager.ReadCredential("LiveSplit_racetimegg_accesstoken")?.Password; }
            set { CredentialManager.WriteCredential("LiveSplit_racetimegg_accesstoken", "", value); }
        }
        public string RefreshToken
        {
            get { return CredentialManager.ReadCredential("LiveSplit_racetimegg_refreshtoken")?.Password; }
            set { CredentialManager.WriteCredential("LiveSplit_racetimegg_refreshtoken", "", value); }
        }
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
        //public abstract Task<bool> RevokeAccess();

    }
}
