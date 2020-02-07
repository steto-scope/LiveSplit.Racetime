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

    public class RacetimeAuthenticator
    {
        private const string clientID = "Rt8ey6rjcQblDxxuERCxZOfJ9p59ANnFWF92HueU";
        private const string clientSecret = "JYVuPmXmylFc3J8vNN3W8egUmeJ9bRPQGCTvZqEjc24KMaCpihhRA0cK7GNZCOxADkJyYo3mjxvuVcaj4OKUv8dZDUmMqa91kuQWf8eV9YwEmalsedMbZZcfAgAW6rAT";
        public const string authServer = "http://192.168.178.70:8000/";
        private const string successTargetUri = "http://192.168.178.70:8000/o/done";
        private const string authorizationEndpoint = "o/authorize";
        private const string tokenEndpoint = "o/token";
        private const string userInfoEndpoint = "o/userinfo";
        private const string revokeEndpoint = "o/revoke_token/";
        private readonly string[] scopes =/* new string[] { "read", "write" };*/{ "read", "chat_message", "race_action"};
        private readonly IPAddress redirectAddress = IPAddress.Loopback;
        private int redirectPort = 6969;
        private const string challengeMethod = "S256";

        private readonly Regex parameterRegex = new Regex(@"(\w+)=([-_A-Z0-9]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private TcpListener localEndpoint;
        private TcpClient serverConnection;

        private string Code { get; set; }
        public string RedirectUri
        {
            get
            {
                return string.Format("http://{0}:{1}/", redirectAddress.ToString(), redirectPort);
            }
        }
        public string CodeVerifier { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public RacetimeUser Identity { get; set; }
        public string Error { get; set; }
        public DateTime TokenExpireDate { get; set; }
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
                return AccessToken != null;
            }
        }



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

        private static Task SendRedirectResponseAsync(TcpClient client)
        {
            return Task.Run(() =>
            {
                using (var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)))
                {
                    writer.WriteLine("HTTP/1.0 301 Moved Permanently");
                    writer.WriteLine("Location: " + successTargetUri);
                }
            });
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

        public void Reset()
        {
            Code = null;
            AccessToken = null;
            RefreshToken = null;
            CodeVerifier = null;
            TokenExpireDate = new DateTime(0);
            Finalize();
        }

        public void Finalize()
        {
            if (localEndpoint != null)
            {
                localEndpoint.Stop();
                localEndpoint = null;
            }
            if (serverConnection != null)
            {
                serverConnection.Close();
                serverConnection.Dispose();
                serverConnection = null;
            }
        }

        public async Task<bool> Authorize()
        {
            Error = null;
            Reset();

            string reqState = null;
            var state = GenerateRandomBase64Data(32);
            CodeVerifier = GenerateRandomBase64Data(32);
            string code_challenge = SHA256(CodeVerifier);

            localEndpoint = new TcpListener(redirectAddress, redirectPort);
            localEndpoint.Start();

            string authorizationRequest = string.Format("{6}{0}?response_type=code&scope={7}&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}",
                authorizationEndpoint,
                RedirectUri,
                clientID,
                state,
                code_challenge,
                challengeMethod,
                authServer,
                string.Join("%20", scopes));

            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            serverConnection = await localEndpoint.AcceptTcpClientAsync();

            // Read response.

            var response = ReadResponse(serverConnection);

            // Sends an HTTP response to the browser.
            SendRedirectResponseAsync(serverConnection).ContinueWith(t =>
            {
                serverConnection.Dispose();
                localEndpoint.Stop();
            });

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
                    return await RequestAccessToken(RefreshToken) != null;
                }
                Reset();
                return false;
            }

            return state == reqState;
        }

        public async Task<Tuple<int, dynamic>> Request(string endpoint, string body)
        {
            string state = GenerateRandomBase64Data(32);
            body += "&state=" + state;
            HttpWebRequest tokenRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}{1}", authServer, tokenEndpoint));

            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            byte[] _byteVersion = Encoding.ASCII.GetBytes(body);
            tokenRequest.ContentLength = _byteVersion.Length;
            Stream stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(_byteVersion, 0, _byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                WebResponse tokenResponse = await tokenRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(tokenResponse.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();
                    var response = JSON.FromString(responseText);
                    //Dictionary<string, string> tokenEndpointDecoded = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseText);
                    return new Tuple<int, dynamic>(200, response);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = ex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            // reads response body
                            string responseText = await reader.ReadToEndAsync();
                            try
                            {
                                var r = JSON.FromString(responseText);
                                Console.WriteLine(responseText);
                                return new Tuple<int, dynamic>(400, r);
                            }
                            catch
                            {
                                return new Tuple<int, dynamic>(401, null);
                            }
                        }
                    }
                }
                return new Tuple<int, dynamic>(500, null);
            }
        }
        public async Task<string> RequestAccessToken(string refreshToken = null)
        {
            if (!IsAuthenticated)
                return null;

            string tokenRequestBody = null;
            if (refreshToken == null)
            {
                tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope={5}&grant_type=authorization_code",
                    Code,
                    RedirectUri,
                    clientID,
                    CodeVerifier,
                    clientSecret,
                    string.Join("%20",scopes)
                    );
            }
            else
            {
                tokenRequestBody = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&grant_type=refresh_token&refresh_token={5}",
                    Code,
                    RedirectUri,
                    clientID,
                    CodeVerifier,
                    clientSecret,
                    RefreshToken
                    );
            }
            var result = await Request(tokenEndpoint, tokenRequestBody);
            if (result.Item1 == 200)
            {


                AccessToken = result.Item2.access_token;
                RefreshToken = result.Item2.refresh_token;
                TokenExpireDate = DateTime.Now.AddSeconds(result.Item2.expires_in);

                
               

                return AccessToken;
            }

            return null;
        }

        public RacetimeUser RequestUserInfo()
        {
            if (!IsAuthorized)
                return null;

            try
            {
                var request = WebRequest.Create(authServer + userInfoEndpoint);
                request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {AccessToken}");
                using (var response = request.GetResponse())
                {
                    var userdata = JSON.FromResponse(response);
                    Console.WriteLine(userdata);
                    Identity = RTModelBase.Create<RacetimeUser>(userdata);
                    Console.WriteLine(Identity.Name);
                    Console.WriteLine(userdata);
                    return Identity;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
                return null;
            }
        }

        public async Task<bool> RevokeAccess()
        {
            string tokenRequestBody = string.Format("token={0}&client_id={1}&client_secret+{2}&grant_type=authorization_code&code={3}", AccessToken, clientID, clientSecret, Code);

            var result = await Request(revokeEndpoint, tokenRequestBody);
            if (result.Item1 == 400)
            {

                if (result.Item2.error == "invalid_grant")
                {
                    AccessToken = null;
                    RefreshToken = null;
                    TokenExpireDate = DateTime.MinValue;
                    return true;
                }
            }

            return false;
        }
    }

}
