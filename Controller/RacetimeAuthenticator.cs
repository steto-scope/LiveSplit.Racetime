using LiveSplit.Racetime.Model;
using LiveSplit.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveSplit.Racetime.Controller
{

    internal class RacetimeAuthenticator
    {
        protected readonly IAuthentificationSettings s;
        
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
        

        public RacetimeAuthenticator(IAuthentificationSettings s)
        {
            this.s = s;
        }

        private readonly Regex parameterRegex = new Regex(@"(\w+)=([-_A-Z0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                
        
        private static string ReadResponse(TcpClient client)
        {
            try
            {
                var readBuffer = new byte[client.ReceiveBufferSize];
                string fullServerReply = null;

                using (var inStream = new MemoryStream())
                {
                    var stream = client.GetStream();

                    while (stream.DataAvailable)
                    {
                        var numberOfBytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                        if (numberOfBytesRead <= 0)
                            break;

                        inStream.Write(readBuffer, 0, numberOfBytesRead);
                    }

                    fullServerReply = Encoding.UTF8.GetString(inStream.ToArray());
                }

                return fullServerReply;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string SHA256(string inputStirng)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(inputStirng);
            SHA256Managed sha256 = new SHA256Managed();
            sha256.ComputeHash(bytes);
            string base64 = Convert.ToBase64String(bytes);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }

        private static string GenerateRandomBase64Data(uint length)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            rng.GetBytes(bytes);
            string base64 = Convert.ToBase64String(bytes);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }
               

        public async Task<bool> Authorize(bool forceRefresh = false)
        {
            //token refresh currently not implemented 

            Error = null;
            string reqState, state, verifier = null, challenge, request, response;
            TcpListener localEndpoint = null;

            if (RefreshToken != null && forceRefresh)
            {
                AccessToken = null;
                goto reauthorize;
            }

reauthenticate:            

            //Step 1: Getting authenticated
            reqState = null;
            state = GenerateRandomBase64Data(32);
            verifier = GenerateRandomBase64Data(32);
            challenge = SHA256(verifier);

            localEndpoint = new TcpListener(s.RedirectAddress, s.RedirectPort);
            localEndpoint.Start();

            request = $"{s.AuthServer}{s.AuthorizationEndpoint}?response_type=code&client_id={s.ClientID}&scope={s.Scopes}&redirect_uri={RedirectUri}&state={state}&code_challenge={challenge}&code_challenge_method={s.ChallengeMethod}";
                

            System.Diagnostics.Process.Start(request);

            using (TcpClient serverConnection = await localEndpoint.AcceptTcpClientAsync())
            {
                response = ReadResponse(serverConnection);
                
                localEndpoint.Stop();
                localEndpoint.Server.Close();
                localEndpoint = null;

                foreach (Match m in parameterRegex.Matches(response))
                {
                    switch (m.Groups[1].Value)
                    {
                        case "state": reqState = m.Groups[2].Value; break;
                        case "code": Code = m.Groups[2].Value; break;
                        case "error": Error = m.Groups[2].Value; break;
                    }
                }

                if (Error != null)
                {
                    if (Error == "invalid_token" && RefreshToken != null)
                    {
                        Error = "Access Token expired";
                        goto failure;
                    }
                    else
                    {
                        await SendRedirectAsync(serverConnection, s.FailureEndpoint);
                        Error = "Unable to authenticate: The server rejected the request";
                        goto failure;
                    }
                }

                if (state != reqState)
                {
                    await SendRedirectAsync(serverConnection, s.FailureEndpoint);
                    Error = "Unable to authenticate: The server hasn't responded correctly. Possible protocol error";
                    goto failure;
                }

                await SendRedirectAsync(serverConnection, s.SuccessEndpoint);
                serverConnection.Close();
            }
            if (localEndpoint != null)
            {
                localEndpoint.Stop();
                localEndpoint.Server.Close();
                localEndpoint = null;
            }

reauthorize:
            //Step 2: Getting authorized     
            request = $"code={Code}&redirect_uri={RedirectUri}&client_id={s.ClientID}&code_verifier={verifier}&client_secret={s.ClientSecret}" + (AccessToken==null&&RefreshToken!=null ? $"&refresh_token={RefreshToken}&grant_type=refresh_token" : $"&scope={s.Scopes}&grant_type=authorization_code");
            
            var result = await RestRequest(s.TokenEndpoint, request);
            if (result.Item1 == 400)
            {
                RefreshToken = null;
                Error = "Access has been revoked. Reauthentication required";
                goto failure;
            }
            if (result.Item1 != 200)
            {
                Error = "Authentication successful, but access wasn't granted by the server";
                goto failure;
            }
           

            AccessToken = result.Item2.access_token;
            RefreshToken = result.Item2.refresh_token;
            TokenExpireDate = DateTime.Now.AddSeconds(result.Item2.expires_in);

            if (AccessToken == null || RefreshToken == null)
            {
                Error = "Final access check failed. Server responded with success, but hasn't delivered a valid Token.";
                goto failure;
            }



            //Step 3: Update User Information       
            try
            {
                Identity = GetUserInfo(s, AccessToken);
                if(Identity == null)
                {
                    Error = "Access denied. Reauthorizing required.";
                    AccessToken = null;
                    RefreshToken = null;
                    goto failure;
                }
            }
            catch (Exception ex)
            {
                Error = "Access has been granted, but retrieving User Information failed";
                goto failure;
            }

            return true;


failure:
            if(localEndpoint !=null)
            {
                localEndpoint.Stop();
                localEndpoint = null;
            }
            Reset();
            return false;
        }

        public RacetimeUser GetUserInfo(IAuthentificationSettings s, string AccessToken)
        {
            var userInfoRequest = WebRequest.Create($"{s.AuthServer}{s.UserInfoEndpoint}");
            userInfoRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {AccessToken}");
            using (var r = userInfoRequest.GetResponse())
            {
                var userdata = JSON.FromResponse(r);
                return RTModelBase.Create<RacetimeUser>(userdata);
            }            
        }

        public void UpdateUserInfo()
        {
            Identity = GetUserInfo(s, AccessToken);
        }

        public async Task<Tuple<int, dynamic>> RestRequest(string endpoint, string body)
        {
            string state = GenerateRandomBase64Data(32);
            body += "&state=" + state;
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create($"{s.AuthServer}{s.TokenEndpoint}");

            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] buf = Encoding.ASCII.GetBytes(body);
            tokenRequest.ContentLength = buf.Length;
            using (Stream stream = tokenRequest.GetRequestStream())
            {
                await stream.WriteAsync(buf, 0, buf.Length);
                stream.Close();
            }
                
            try
            {
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                dynamic response = JSON.FromResponse(tokenResponse);
                return new Tuple<int, dynamic>(200, response);                
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    WebResponse response = ex.Response as HttpWebResponse;
                    try
                    {
                        var r = JSON.FromResponse(response);
                        return new Tuple<int, dynamic>(400, r);
                    }
                    catch
                    {
                        return new Tuple<int, dynamic>(500, null);
                    }
                }                
            }
            catch
            {

            }

            return new Tuple<int, dynamic>(500, null);
        }
              

        /*public override async Task<bool> RevokeAccess()
        {
            string request = $"token={AccessToken}&client_id={s.ClientID}&client_secret+{s.ClientSecret}&grant_type=authorization_code&code={Code}";

            var result = await RestRequest(s.RevokeEndpoint, request);
            if (result.Item1 == 400)
            {
                if (result.Item2.error == "invalid_grant")
                {
                    AccessToken = null;
                    RefreshToken = null;
                    TokenExpireDate = DateTime.MaxValue;
                    return true;
                }
            }

            return false;
        }*/
    }

}
