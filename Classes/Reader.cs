using FlihtMesseger.Classes;

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Reflection;

namespace FlihtMesseger.Chat
{
    class Reader
    {
        //Vars
        private Random rand = new Random();
        private string[] replies = { };
        public int counter = 0;
        public bool errorb;

        private string _readedtext = "";
        private string _address;
        private Exception _error;
        private string _readedbyclient = "{EMPTY}";

        private WebClient client = new WebClient();
        private string _message;

        public string _username { get; private set; }

        public Reader(string address)
        {
            _address = address;
            client.DownloadStringCompleted += Client_DownloadStringCompleted;
        }

        public void ChangeAddress(string address)
        {
            _address = address;
        }

        public string GetString()
        {
            return _readedtext;
        }

        public string GetMessage()
        {
            return _message;
        }

        public string GetUser()
        {
            return _username;
        }

        public async Task Read(ChatStyle style)
        {
            _readedbyclient = "{EMPTY}";
            _readedtext = "";
            try
            {
                Uri url = new Uri(_address + "/MessengerRead.php?isclient=true&byte=" + counter);
                client.DownloadStringAsync(url);

                while (_readedbyclient == "{EMPTY}")
                {
                    await Task.Delay(100);
                }
                if (_readedbyclient != "{BREAK}")
                {
                    string rawReply = _readedbyclient;

                    string rawbyte = "";
                    int size = 0;
                    foreach (char r in rawReply)
                    {
                        if (r != '~')
                        {
                            size++;
                            rawbyte += r;
                        }
                        else
                        {
                            break;
                        }
                    }
                    rawReply = rawReply.Substring(size + 1);
                    counter = int.Parse(rawbyte);

                    replies = rawReply.Replace("\n", "▪").Split('▪');
                    bool noComment = false;
                    string newreply = "";
                    string token = "";
                    foreach (string reply in replies)
                    {
                        newreply = reply;

                        //Comment Filter
                        foreach (char r in newreply)
                        {
                            noComment = r == '#' ? false : true;
                            break;
                        }

                        //Filter
                        int nextint = 0;
                        int currint = 0;

                        foreach (char r in newreply)
                        {
                            currint++;
                            nextint--;

                            if (r == '@')
                            {
                                token = newreply.Substring(currint, 4);
                                newreply = newreply.Remove(currint - 1, 5);
                                nextint = 8;
                            }
                            if (nextint == 0)
                            {
                                newreply = newreply.Remove(currint - 1, 11);
                            }
                        }
                        if (noComment == true)
                        {
                            string[] replywords = newreply.Replace("-", ":").Split('~');

                            _message = Base64.Decode(replywords[2]);
                            _username = replywords[0];

                            if (style == ChatStyle.ALL)
                                _readedtext += "\r\n" + replywords[1] + "> ( " + _username + "#" + token + " ) " + _message;
                            else if (style == ChatStyle.IRC)
                                _readedtext += "\r\n" + replywords[1] + "> ( " + _username + " ) " + _message;
                            else
                                _readedtext += "\r\n" + _username + "> " + _message;
                        }
                    }
                    _error = null;
                    errorb = false;
                    //_readedtext = _readedtext;
                }
            }
            catch (Exception ex)
            {
                errorb = true;
                _error = ex;
            }
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                _readedbyclient = e.Result;
            }
            catch (TargetInvocationException ex)
            {
                errorb = true;
                Exception ex1 = ex.InnerException;
                _error = ex1;
                _readedbyclient = "{BREAK}";
                _readedtext = "";
            }
            catch (Exception ex)
            {
                errorb = true;
                _error = ex;
                _readedbyclient = "{BREAK}";
                _readedtext = "";
            }
            //throw new NotImplementedException();
        }

        public bool GetError(out Exception error)
        {
            error = _error;
            if (errorb == false)
                return false;
            else
                return true;
        }

        private static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            Ping pinger = new Ping();
            try
            {
                PingReply reply = pinger.Send(nameOrAddress);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }
            return pingable;
        }

        internal void Refresh()
        {
            rand = new Random();
            replies = null;
            counter = 0;
            errorb = false;

            _readedtext = "";
            _error = null;
            _readedbyclient = "{EMPTY}";
        }
    }
}
