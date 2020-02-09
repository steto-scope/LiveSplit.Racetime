using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Controller
{
    internal class RTAuthentificationSettings : IAuthentificationSettings
    {
        public virtual string ClientID => "Rt8ey6rjcQblDxxuERCxZOfJ9p59ANnFWF92HueU";
        public virtual string ClientSecret => "JYVuPmXmylFc3J8vNN3W8egUmeJ9bRPQGCTvZqEjc24KMaCpihhRA0cK7GNZCOxADkJyYo3mjxvuVcaj4OKUv8dZDUmMqa91kuQWf8eV9YwEmalsedMbZZcfAgAW6rAT";
        public virtual string AuthServer => "http://192.168.178.70:8000/";
        public virtual string SuccessEndpoint => "o/done";
        public virtual string FailureEndpoint => "o/done?error=access_denied";
        public virtual string AuthorizationEndpoint => "o/authorize";
        public virtual string TokenEndpoint => "o/token";
        public virtual string UserInfoEndpoint => "o/userinfo";
        public virtual string RevokeEndpoint => "o/revoke_token/";
        public virtual string Scopes => "read chat_message race_action";
        public virtual IPAddress RedirectAddress => IPAddress.Loopback;
        public virtual int RedirectPort => 6969;
        public virtual string ChallengeMethod => "S256";
    }
}
