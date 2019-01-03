using FlihtMesseger.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FlihtMesseger.Classes
{
    class ReadChatClass
    {
        //Windows
        MessagesBox msgBox = new MessagesBox();

        //Vars
        string _readedtext = "";
        string[] replies = {};

        //--//ȠoChat//--//
        //--//User1@1234~18-55-01-19-06-2018~What?//--//
        //--//18-55-01> ( User1 ) What? Visible//--//
        //--//1234 + 19-06-2018 Hidden//--//
        public string ReadChat(string address, ChatStyle style)
        {
            _readedtext = "";
            try
            {
                WebClient client = new WebClient();
                string rawReply = client.DownloadString(address);

                replies = rawReply.Replace("\r\n", "▪").Split('▪');
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
                        //ALL
                        string originreplyALL = replywords[1] + "> ( " + replywords[0] + "#" + token + " ) " + replywords[2];
                        //IRC
                        string originreplyIRC = replywords[1] + "> ( " + replywords[0] + " ) " + replywords[2];
                        //Modern
                        string originreply = replywords[0] + "> " + replywords[2];
                        if (style == ChatStyle.ALL)
                            _readedtext += "\r\n" + originreplyALL;
                        else if (style == ChatStyle.IRC)
                            _readedtext += "\r\n" + originreplyIRC;
                        else
                            _readedtext += "\r\n" + originreply;
                    }
                    //throw new OutOfMemoryException();
                }
                return _readedtext;
            }
            catch (Exception ex)
            {
                msgBox.main.Text = ex.ToString();
                msgBox.Title = ex.Message;
                msgBox.Show();
                return "Error! New request in few seconds...";
                //throw ex;
            }
        }
    }
}
