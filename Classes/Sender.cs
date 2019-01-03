using FlihtMesseger.Classes;
using System;
using System.Net;

namespace FlihtMesseger.Chat
{
    class Sender
    {
        private string _address;
        private string _message;
        private bool _errorb;
        private WebException _error;
        public Sender(string address)
        {
            _address = address;
        }

        internal void Send(string message, User user)
        {
            //Very Good Date Converter XD
            DateTime rawDate = DateTime.Now;
            string date =
                (rawDate.Hour.ToString().Length >= 2 ? rawDate.Hour.ToString() : 0 + rawDate.Hour.ToString()) + "-" +
                (rawDate.Minute.ToString().Length >= 2 ? rawDate.Minute.ToString() : 0 + rawDate.Minute.ToString()) + "-" +
                (rawDate.Second.ToString().Length >= 2 ? rawDate.Second.ToString() : 0 + rawDate.Second.ToString()) + "-" +
                (rawDate.Day.ToString().Length >= 2 ? rawDate.Day.ToString() : 0 + rawDate.Day.ToString()) + "-" +
                (rawDate.Month.ToString().Length >= 2 ? rawDate.Month.ToString() : 0 + rawDate.Month.ToString()) + "-" +
                (rawDate.Year.ToString().Length >= 2 ? rawDate.Year.ToString() : 0 + rawDate.Year.ToString());

            _message = $"\n{user.username}@{user.usertoken}~{date}~{Base64.Encode(message)}";
            try
            {
                WebClient client = new WebClient();
                string rawReply = client.DownloadString(_address + "/MessengerWrite.php?isclient=true&msg=" + _message);
            }
            catch (WebException ex)
            {
                _errorb = true;
                _error = ex;
            }
        }

        public bool GetError(out WebException error)
        {
            error = _error;
            if (_errorb == false)
                return false;
            else
                return true;
        }

        internal void ChangeAddress(string address)
        {
            _address = address;
        }
    }
}
